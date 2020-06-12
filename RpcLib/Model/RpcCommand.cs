using RpcLib.Rpc.Utils;
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

        // Maximum time in seconds a sent command may take to be executed and acknowledged. This
        // includes the time where it is still in the queue.
        public const int timeoutSeconds = 30;

        /// <summary>
        /// Creates a new encoded RPC command, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        public RpcCommand(string methodName, params object[] methodParameters) {
            lock(syncLock) {
                lastNumber++;
                ID = lastNumber;
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
        /// The next ID will be the last ID + 1.
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
                long timeoutTime = CoreUtils.TimeNow() + timeoutSeconds * 1000;
                while (false == IsFinished() && CoreUtils.TimeNow() < timeoutTime)
                    await Task.Delay(100); // TODO: More elegant waiting then this "active waiting", e.g. by callback
                // Timeout?
                if (false == IsFinished())
                    throw new RpcException(new RpcFailure(RpcFailureType.LocalTimeout, "Timeout"));
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

        // Helper fields
        private static ulong lastNumber = 0;
        private static readonly object syncLock = new object();
        private RpcCommandResult? result = null;

    }

}
