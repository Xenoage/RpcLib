using RpcLib.Logging;
using RpcLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RpcLib.Model {

    /// <summary>
    /// Encoded RPC command. Together with the target peer ID, the current state, the method name and JSON-encoded parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// As soon as the call finishes (whether successfully or failed), the result is also stored here.
    /// </summary>
    public class RpcCommand {

        /// <summary>
        /// Creates a new encoded RPC command to be run on the server, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        public static RpcCommand CreateForServer(string methodName, params object[] methodParameters) =>
            new RpcCommand(methodName, methodParameters);

        /// <summary>
        /// Creates a new encoded RPC command to be run on the client with the given ID,
        /// using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        public static RpcCommand CreateForClient(string clientID, string methodName, params object[] methodParameters) =>
            new RpcCommand(methodName, methodParameters) { TargetPeerID = clientID };

        /// <summary>
        /// Creates a new encoded RPC command, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        private RpcCommand(string methodName, params object[] methodParameters) {
            lock (syncLock) {
                loop++;
                if (loop > 999)
                    loop = 0;
                ID = ((ulong)CoreUtils.TimeNow()) * 1000 + loop;
            }
            MethodName = methodName;
            MethodParameters = methodParameters.Select(it => JsonLib.ToJson(it)).ToList();
        }

        /// <summary>
        /// Default constructor. For JSON deserialization only.
        /// In the program code, use 
        /// </summary>
        public RpcCommand() {
        }

        /// <summary>
        /// Unique ID of this command.
        /// The ID looks like this: ({unix time in ms}{000-999}),
        /// i.e. ascending over time.
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// The ID of the target peer where to run this command on, i.e. the
        /// client ID or "" for the server.
        /// </summary>
        public string TargetPeerID { get; set; } = "";

        /// <summary>
        /// The name of the method.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// The list of JSON-encoded parameters.
        /// See <see cref="JsonLib"/> how the parameters are encoded.
        /// </summary>
        public List<string> MethodParameters { get; set; }

        /// <summary>
        /// Gets the decoded message parameter with the given index.
        /// </summary>
        public T GetParam<T>(int index) =>
            JsonLib.FromJson<T>(MethodParameters[index]);

        /// <summary>
        /// Individual timeout for this command.
        /// By default <see cref="defaultTimeoutMs"/> is used.
        /// </summary>
        public int? TimeoutMs { get; set; } = null;

        /// <summary>
        /// Strategy used for automatic retrying of this command,
        /// when it has failed because of network problems.
        /// </summary>
        public RpcRetryStrategy? RetryStrategy { get; set; } = null;

        /// <summary>
        /// Strategy used for compress messages to reduce traffic between the peers.
        /// </summary>
        public RpcCompressionStrategy? Compression { get; set; } = null;

        /// <summary>
        /// The current state of this command.
        /// The RPC engine calls <see cref="SetState"/> and <see cref="Finish"/> to update
        /// the state while it processes the command.
        /// </summary>
        public RpcCommandState State { get; set; } = RpcCommandState.Created;

        /// <summary>
        /// Returns true, iff the result or exception for the command call was already received.
        /// </summary>
        public bool IsFinished() =>
            State == RpcCommandState.Successful || State == RpcCommandState.Failed;

        /// <summary>
        /// The result of the call, which is available as soon as <see cref="IsFinished"/> is true.
        /// </summary>
        public RpcCommandResult GetResult() =>
            result ?? throw new Exception("Command not finished yet");

        /// <summary>
        /// Call this method to change the current state when it is not yet finished.
        /// To finish it, use <see cref="Finish"/> instead.
        /// </summary>
        public void SetState(RpcCommandState state) {
            if (state == RpcCommandState.Successful || state == RpcCommandState.Failed)
                throw new Exception("Method call not allowed for finished commands");
            RpcMain.Log($"Command {ID}: State changed to {state}", LogLevel.Trace);
            State = state;
        }

        /// <summary>
        /// Call this method to set this command as finished, using the given result.
        /// </summary>
        public void Finish(RpcCommandResult result) {
            State = result.State;
            this.result = result;
            runningTask.SetResult(result);
        }

        /// <summary>
        /// When this method is called with a class implementing <see cref="IRpcFunctions"/>
        /// in the calling stack, the <see cref="RpcOptionsAttribute"/> (if any) of the method
        /// with this command name are read and applied.
        /// </summary>
        public void ApplyRpcOptionsFromCallStack() {
            // Find attributes (e.g. custom timeout, retry strategy) for this method definition.
            // In the call stack, find a caller (e.g. "RpcServerStub") implementing an interface (e.g. "IRpcServer")
            // based on the IFunctions interface.
            // Find a method with the command's name and have a look at its RpcOptions attribute.
            foreach (var stackFrame in new StackTrace().GetFrames()) {
                var frameType = stackFrame.GetMethod().DeclaringType;
                if (frameType.GetInterfaces().FirstOrDefault(it => it.GetInterfaces().Contains(typeof(IRpcFunctions))) is Type intf) {
                    var method = intf.GetMethod(MethodName);
                    if (method != null && method.GetCustomAttribute<RpcOptionsAttribute>() is RpcOptionsAttribute options) {
                        if (options.TimeoutMs != RpcOptionsAttribute.useDefaultTimeout)
                            TimeoutMs = options.TimeoutMs;
                        if (options.RetryStrategy is RpcRetryStrategy retryStrategy)
                            RetryStrategy = retryStrategy;
                        if (options.Compression is RpcCompressionStrategy compression)
                            Compression = compression;
                        break; // Method found, do not traverse call stack any further
                    }
                }
            }
        }

        /// <summary>
        /// Call this method after enqueuing the command to wait for the result of its execution.
        /// The returned task finishes when the call was either successfully executed and
        /// acknowledged, or failed (e.g. because of a timeout).
        /// The result is stored in the given command itself. If successful, the return value
        /// is also returned, otherwise an <see cref="RpcException"/> is thrown.
        /// </summary>
        public async Task<T> WaitForResult<T>() {
            try {
                // Wait for result until timeout
                await Task.WhenAny(runningTask.Task, Task.Delay(TimeoutMs ?? RpcMain.DefaultSettings.TimeoutMs));
                // Timeout?
                if (false == IsFinished())
                    throw new RpcException(new RpcFailure(RpcFailureType.Timeout, "Timeout"));
                // Failed? Then throw RPC exception
                var result = GetResult();
                if (result.Failure is RpcFailure failure)
                    throw new RpcException(failure);
                // Return JSON-encoded result (or null for void return type)
                if (result.ResultJson is string json)
                    return JsonLib.FromJson<T>(json);
                else
                    return default!;
            }
            catch (RpcException) {
                throw; // Rethrow RPC exception
            }
            catch (Exception ex) {
                throw new RpcException(new RpcFailure(RpcFailureType.Other, ex.Message)); // Wrap any other exception
            }
        }
        private TaskCompletionSource<RpcCommandResult> runningTask = new TaskCompletionSource<RpcCommandResult>();


        // Helper fields
        private static ulong loop = 0; // Looping between 0 and 999 to allow up to 1000 commands per ms
        private static readonly object syncLock = new object();
        private RpcCommandResult? result = null;

    }

}
