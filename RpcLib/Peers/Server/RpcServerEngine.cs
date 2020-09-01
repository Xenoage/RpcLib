using RpcLib.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpcLib.Peers.Server {

    /// <summary>
    /// This is the core part of the RPC engine on the server side.
    /// It sends the commands to the clients, i.e. it stores the command queues (one for each client),
    /// sends the commands as soon as possible and processes the results.
    /// It also receives the calls from the clients and responds to them.
    /// </summary>
    public class RpcServerEngine {

        /// <summary>
        /// There is exactly one instance of this class.
        /// </summary>
        public static RpcServerEngine Instance { get; } = new RpcServerEngine();

        /// <summary>
        /// Gets the list of all client IDs which are or were connected to the server.
        /// </summary>
        public List<string> GetClientIDs() =>
            clients.GetClientIDs();

        /// <summary>
        /// Call this method when the client called the "/rpc/push"-endpoint.
        /// It executes the given RPC command immediately and returns the result.
        /// No exception is thrown, but a <see cref="RpcFailure"/> result is set in case of a failure.
        /// </summary>
        public async Task<RpcCommandResult> OnClientPush(string clientID, RpcCommand command, RpcCommandRunner runner) {
            // Do not run the same command twice. If the command with this ID was already
            // executed, return the cached result. If the cache is not available any more, return a
            // obsolete function call failure.
            var client = clients.GetClient(clientID, commandBacklog);
            if (client.GetCachedResult(command.ID) is RpcCommandResult result)
                return result;
            // Execute the command
            try {
                result = await runner.Execute(clientID, command);
            }
            catch (Exception ex) {
                result = RpcCommandResult.FromFailure(command.ID,
                    new RpcFailure(RpcFailureType.RemoteException, ex.Message), command.Compression);
            }
            // Cache result, if there was no network problem
            if (false == (result.Failure?.IsNetworkProblem == true))
                client.CacheResult(result);
            return result;
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
        public async Task<RpcCommand?> OnClientPull(string clientID, RpcCommandResult? lastCommandResult) {
            // When a result is received, process it
            if (lastCommandResult != null)
                await ReportClientResult(clientID, lastCommandResult);
            // Wait for next command
            RpcCommand? next = await clients.GetClient(clientID, commandBacklog).DequeueCommand(longPollingSeconds * 1000);
            if (next != null) {
                next.SetState(RpcCommandState.Sent);
                return next;
            }
            // No item during long polling time. Return null.
            return null;
        }

        /// <summary>
        /// Call this method, when the client reported the result of the current command.
        /// </summary>
        private async Task ReportClientResult(string clientID, RpcCommandResult? lastResult) {
            // Get current command on this client
            var client = clients.GetClient(clientID, commandBacklog);
            // Result matching to expected last command? Ignore wrong reports (e.g. when received two times or too late).
            if (lastResult != null && client.CurrentCommand is RpcCommand command && lastResult.CommandID == command.ID) {
                // When a result was received (i.e. when there was no network problem), the command is finished
                if (false == (lastResult.Failure?.IsNetworkProblem == true))
                    client.FinishCurrentCommand();
                // Response for this command received.
                command.Finish(lastResult);
            }
        }

        /// <summary>
        /// Runs the given RPC command on the client with the given <see cref="RpcCommand.TargetPeerID"/>
        /// as soon as possible and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        public async Task<T> ExecuteOnClient<T>(RpcCommand command) {
            try {
                var clientID = command.TargetPeerID;
                if (clientID.Length == 0)
                    throw new Exception("No client ID given");
                // Apply [RpcOptions(...)] from method declaration
                command.ApplyRpcOptionsFromCallStack();
                // Enqueue (and execute)
                clients.GetClient(clientID, commandBacklog).EnqueueCommand(command);
                // Wait for result until timeout
                return await command.WaitForResult<T>();
            }
            catch (RpcException ex) {
                throw; // Rethrow RPC exception
            }
            catch (Exception ex) {
                throw new RpcException(new RpcFailure(RpcFailureType.Other, ex.Message)); // Wrap any other exception
            }
        }

        // Backlog for retrying commands
        private IRpcCommandBacklog? commandBacklog;
        public void SetCommandBacklog(IRpcCommandBacklog? commandBacklog) => this.commandBacklog = commandBacklog;

        // Long polling time in seconds. After this time, the server returns null when there is
        // no command in the queue.
        public const int longPollingSeconds = 90;

        // The registered clients and their command queues and cached command results
        private RpcClientCaches clients = new RpcClientCaches();

    }

}
