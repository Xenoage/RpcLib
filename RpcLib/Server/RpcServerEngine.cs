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

        // Long polling time in seconds. After this time, the server returns null when there is
        // no command in the queue.
        private const int longPollingSeconds = 90;

        private static RpcClientQueues clients = new RpcClientQueues();


        /// <summary>
        /// Call this method when the client called the "/rpc/push"-endpoint.
        /// It executes the given RPC command immediately and returns the result.
        /// </summary>
        public static async Task<RpcCommand?> OnClientPull(string clientID, RpcCommand command) {
            // Do not run the same command twice. If the command with this ID was already
            // executed, return the cached result. If the cache is not available any more, return a
            // obsolete function call failure.
            if (clients.)
        }

        /// <summary>
        /// Call this method when the client called the "/rpc/pull"-endpoint.
        /// It does two things: Reporting the last result (optional) and querying the next command.
        /// 
        /// This method retuns the current <see cref="RpcCommand"/> in the queue of the calling client,
        /// by "long polling". Because the server can not call the client directly (firewall...),
        /// instead the client continuously calls this method and waits for new data.
        /// Because the server only responds when there is data available or with null when
        /// a long timeout is hit (e.g. 90 seconds), the traffic in the network is highly limited. 
        /// 
        /// To ensure that a command is received by the client, it contains a unique <see cref="RpcCommand.ID"/>.
        /// The client has to acknowledge that it has received and executed it by
        /// sending this ID and the result (or exception) back as a <see cref="RpcCommandResult"/>
        /// in the body of this API endpoint. Otherwise, the same command would be sent again by the server.
        /// The client can also use this ID to ensure that the command is only evaluated once,
        /// even when it was received two times for any reason.
        /// </summary>
        public static async Task<RpcCommand?> OnClientPull(string clientID, RpcCommandResult lastCommandResult) {
            // When a result is received, process it
            if (lastCommandResult != null)
                ReportClientResult(clientID, lastCommandResult);
            // Wait for next command
            long endTime = CoreUtils.TimeNow() + longPollingSeconds * 1000;
            while (CoreUtils.TimeNow() < endTime) {
                RpcCommand? next = clients.GetCurrentCommand(clientID);
                if (next != null) {
                    next.SetState(RpcCommandState.Sent);
                    return next;
                }
                await Task.Delay(100); // TODO: More elegant waiting then this "active waiting", e.g. by callback
            }
            // No item during long polling time. Return null.
            return null;
        }

        /// <summary>
        /// Call this method, when the client reported the result of the current command.
        /// </summary>
        private static void ReportClientResult(string clientID, RpcCommandResult? lastResult) {
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
