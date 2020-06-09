using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Rpc {

    /// <summary>
    /// Encoded RPC command. Together with the current state, the method name and JSON-encoded parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// As soon as the call finishes successfully or failed, the result is also stored here.
    /// </summary>
    public class RpcCommand {

        // Unique ID of this command, ascending over time
        public ulong ID { get; }
        private static ulong lastNumber = 0;
        private static readonly object syncLock = new object();

        // Method name and parameter (if any)
        public string MethodName { get; }
        public List<string> MethodParameters { get; } // Encoded in JSON, contains $type (see Newtonsoft.JSON docs)

        // Current state, and if finished, result value
        public RpcCallState State { get; set; } = RpcCallState.Created;
        public RpcCommandResult? Result { get; set; } = null;


        public RpcCommand(string methodName, params object[] methodParameters) {
            lock(syncLock) {
                ID = lastNumber;
                lastNumber++;
            }
            MethodName = methodName;
            MethodParameters = methodParameters.Select(it => JsonConvert.SerializeObject(it)).ToList();
        }

        public void Finish(RpcCommandResult result) {
            State = result.State;
            Result = result;
        }

    }

}
