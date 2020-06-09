using Shared.Model;
using Shared.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpcServer.Rpc {

    /// <summary>
    /// This is the core part of the RPC engine on the server side.
    /// It sends the commands to the clients, i.e. it stores the command queues (one for each client),
    /// sends the commands as soon as possible and processes the results.
    /// It also receives the calls from the clients and responds to them.
    /// </summary>
    public class RpcServerEngine {

        private RpcClientQueues clients = new RpcClientQueues();

        /// <summary>
        /// Call this method, when the client reported the result of the current command.
        /// </summary>
        public void ReportClientResult(string clientID, RpcCommandResult? lastResult) {
            // Get current command on this client
            RpcCommand? currentCommand = clients.GetCurrentCommand(clientID);
            // Handle reported result. Ignore wrong reports (e.g. when received two times or too late)
            if (currentCommand != null && lastResult != null && lastResult.ID == currentCommand.ID) {
                // Response for this command received.
                currentCommand.Result = lastResult;
                clients.FinishCurrentCommand(clientID);
            }
        }

        /// <summary>
        /// Gets the current command for the given client, or null, if there is none.
        /// </summary>
        public RpcCommand? GetClientCommand(string clientID) {
            return clients.GetCurrentCommand(clientID);
        }

    }
}
