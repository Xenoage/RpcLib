using RpcLib.Rpc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpcLib.Model {

    /// <summary>
    /// Encoded RPC command. Together with the current state, the method name and JSON-encoded parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// As soon as the call finishes (whether successfully or failed), the result is also stored here.
    /// </summary>
    public class RpcCommand {

        /// <summary>
        /// Creates a new encoded RPC command, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        public RpcCommand(string methodName, params object[] methodParameters) {
            lock(syncLock) {
                ID = lastNumber;
                lastNumber++;
            }
            MethodName = methodName;
            MethodParameters = methodParameters.Select(it => JsonLib.ToJson(it)).ToList();
        }

        /// <summary>
        /// Unique ID of this command, ascending over time
        /// </summary>
        public ulong ID { get; }

        /// <summary>
        /// The name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// The list of JSON-encoded parameters.
        /// See <see cref="JsonLib"/> how the parameters are encoded.
        /// </summary>
        public List<string> MethodParameters { get; }

        /// <summary>
        /// The current state of this command.
        /// The RPC engine calls <see cref="SetState"/> and <see cref="Finish"/> to update
        /// the state while it processes the command.
        /// </summary>
        public RpcCommandState State { get; private set; } = RpcCommandState.Created;

        /// <summary>
        /// Returns true, iff the result or exception for the command call was already received.
        /// </summary>
        public bool IsFinished =>
            State == RpcCommandState.Successful || State == RpcCommandState.Failed;

        /// <summary>
        /// The result of the call, which is available as soon as <see cref="IsFinished"/> is true.
        /// </summary>
        public RpcCommandResult Result =>
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

        // Helper fields
        private static ulong lastNumber = 0;
        private static readonly object syncLock = new object();
        private RpcCommandResult? result = null;

    }

}
