using RpcLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpcLib.Model {

    /// <summary>
    /// Encoded RPC command. Together with the current state, the method name and JSON-encoded parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// As soon as the call finishes (whether successfully or failed), the result is also stored here.
    /// </summary>
    public class RpcCommand {

        // Maximum time in milliseconds a sent command may take to be executed and acknowledged. This
        // includes the time where it is still in the queue.
        public static int defaultTimeoutMs = 30_000;

        /// <summary>
        /// Creates a new encoded RPC command, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        public RpcCommand(string methodName, params object[] methodParameters) {
            lock(syncLock) {
                loop++;
                if (loop > 999)
                    loop = 0;
                ID = ((ulong) CoreUtils.TimeNow()) * 1000 + loop;
            }
            MethodName = methodName;
            MethodParameters = methodParameters.Select(it => JsonLib.ToJson(it)).ToList();
        }

        /// <summary>
        /// Default constructor. For JSON deserialization only.
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
        /// Call this method after enqueuing the command to wait for the result of its execution.
        /// The returned task finishes when the call was either successfully executed and
        /// acknowledged, or failed (e.g. because of a timeout).
        /// The result is stored in the given command itself. If successful, the return value
        /// is also returned, otherwise an <see cref="RpcException"/> is thrown.
        /// </summary>
        public async Task<T> WaitForResult<T>() {
            try {
                // Wait for result until timeout
                await Task.WhenAny(runningTask.Task, Task.Delay(TimeoutMs ?? defaultTimeoutMs));
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
