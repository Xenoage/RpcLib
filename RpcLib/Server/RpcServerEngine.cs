using RpcLib.Model;
using RpcLib.Rpc.Utils;
using RpcLib.Utils;
using System;
using System.Threading.Tasks;

namespace RpcLib.Server {

    /// <summary>
    /// This is the core part of the RPC engine on the server side.
    /// It sends the commands to the clients, i.e. it stores the command queues (one for each client),
    /// sends the commands as soon as possible and processes the results.
    /// It also receives the calls from the clients and responds to them.
    /// </summary>
    public static class RpcServerEngine {

        // Maximum time in seconds a sent command may take to be executed and acknowledged. This
        // includes the time where it is still in the queue.
        private const int clientTimeoutSeconds = 30;

        private static RpcClientQueues clients = new RpcClientQueues();

        /// <summary>
        /// Call this method, when the client reported the result of the current command.
        /// </summary>
        public static void ReportClientResult(string clientID, RpcCommandResult? lastResult) {
            // Get current command on this client
            RpcCommand? currentCommand = clients.GetCurrentCommand(clientID);
            // Handle reported result. Ignore wrong reports (e.g. when received two times or too late)
            if (currentCommand != null && lastResult != null && lastResult.ID == currentCommand.ID) {
                // Response for this command received.
                currentCommand.Finish(lastResult);
                clients.FinishCurrentCommand(clientID);
            }
        }

        /// <summary>
        /// Gets the current command for the given client, or null, if there is none.
        /// </summary>
        public static RpcCommand? GetClientCommand(string clientID) {
            return clients.GetCurrentCommand(clientID);
        }

        /// <summary>
        /// Runs the given RPC command on the client with the given ID as soon as possible.
        /// The returned task finishes when the call was either successfully executed and
        /// acknowledged, or failed (e.g. because of timeout).
        /// The result is stored in the given command itself. If successful, the JSON-encoded return value
        /// is also returned, otherwise an <see cref="RpcException"/> is thrown.
        /// </summary>
        public static async Task<T> ExecuteOnClient<T>(string clientID, RpcCommand command) where T : class {
            try {
                // Enqueue (and execute)
                clients.EnqueueCommand(clientID, command);
                // Wait for result until timeout
                long timeoutTime = CoreUtils.TimeNow() + clientTimeoutSeconds * 1000;
                while (false == command.IsFinished && CoreUtils.TimeNow() < timeoutTime)
                    await Task.Delay(100); // TODO: More elegant waiting then this "active waiting", e.g. by callback
                // Timeout?
                if (false == command.IsFinished)
                    throw new RpcException(new RpcFailure(RpcFailureType.LocalTimeout, "Client timeout"));
                // Failed? Then throw RPC exception
                if (command.Result.Failure is RpcFailure failure)
                    throw new RpcException(failure);
                // Return JSON-encoded result (or null for void return type)
                if (command.Result.ResultJson is string json)
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

    }

}
