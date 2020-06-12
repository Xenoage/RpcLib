using RpcLib.Model;
using RpcLib.Peers;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RpcLib.Client {

    /// <summary>
    /// This is the core part of the RPC engine on the client side.
    /// It sends the commands to the server, i.e. it stores the command queue,
    /// sends the commands as soon as possible and processes the results.
    /// It also pulls the commands for this client from the server,
    /// executes them and responds with the results.
    /// </summary>
    public static class RpcClientEngine {

        // Queue for the commands and cached command results of the server
        private static RpcPeerCache server = new RpcPeerCache();

        private static HttpClient http = new HttpClient();

        public static IRpcPeer client; // TODO: inject this property!

        private static bool isRunning = false;


        /// <summary>
        /// Runs the given RPC command on the server as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// See <see cref="RpcCommand.WaitForResult{T}"/>
        /// </summary>
        public static async Task<T> ExecuteOnServer<T>(RpcCommand command) where T : class {
            try {
                // Enqueue (and execute)
                server.EnqueueCommand(command);
                // Wait for result until timeout
                return await command.WaitForResult<T>();
            }
            catch (RpcException) {
                throw; // Rethrow RPC exception
            }
            catch (Exception ex) {
                throw new RpcException(new RpcFailure(RpcFailureType.Other, ex.Message)); // Wrap any other exception
            }
        }

        /// <summary>
        /// Call this method at the beginning to enable the communication to the server, so that this client
        /// can both receive commands from the server and send commands to the server.
        /// An action must be given which authorizes the used HTTP client, e.g. by adding HTTP Basic Auth
        /// information according to the client.
        /// </summary>
        public static void Start(Action<HttpClient> authorizeAction) {
            if (isRunning)
                return;
            isRunning = true;
            // Create and authorize HTTP client
            http = new HttpClient();
            authorizeAction(http);
            // Loop to pull the next command for this client from the server, execute it (if not already executed before)
            // and send the response together with the next pull.
            _ = Task.Run(async () => {
                RpcCommandResult? lastResult = null;
                while (isRunning) {
                    RpcCommand? next = await PullFromServer(lastResult);
                    if (next != null) {
                        lastResult = await ExecuteLocallyNow(next);
                    }
                    else {
                        await Task.Delay(100); // TODO: More elegant waiting than this "active waiting", e.g. by callback
                    }
                }
            });
            // Loop to execute (i.e. send to server) the next command in the queue.
            _ = Task.Run(async () => {
                while (isRunning) {
                    RpcCommand? next = server.GetCurrentCommand();
                    if (next != null) {
                        next.SetState(RpcCommandState.Sent);
                        await ExecuteOnServerNow(next);
                    }
                    else {
                        await Task.Delay(100); // TODO: More elegant waiting than this "active waiting", e.g. by callback
                    }
                }
            });
        }

        /// <summary>
        /// Call this method to stop the communication with the server as soon as possible.
        /// </summary>
        public static void Stop() {
            isRunning = false;
        }

        /// <summary>
        /// Sends the given command to the server now, reads the result or catches the
        /// exception, and sets the command's state accordingly.
        /// This method does not throw exceptions.
        /// </summary>
        private static async Task ExecuteOnServerNow(RpcCommand command) {
            // TODO
        }

        /// <summary>
        /// Polls the next command to execute locally from the server. The result of the
        /// last executed command must be given, if there is one. The returned Task may block
        /// some time, because the server uses the long polling technique to reduce network traffic.
        /// </summary>
        private static async Task<RpcCommand> PullFromServer(RpcCommandResult? lastResult) {
            // Long polling. The server returns null after the long polling time, but we also
            // return null in case of a connection problem.
            var httpResponse = http.GetAsync(Config.Url + "/" + endpoint);
        }

        /// <summary>
        /// Executes the given RPC command locally on the client immediately and returns the result.
        /// No exception is thrown, but a <see cref="RpcFailure"/> result is set in case of a failure.
        /// </summary>
        public static async Task<RpcCommandResult> ExecuteLocallyNow(RpcCommand command) {
            // Do not run the same command twice. If the command with this ID was already
            // executed, return the cached result. If the cache is not available any more, return a
            // obsolete function call failure.
            if (server.GetCachedResult(command.ID) is RpcCommandResult result)
                return result;
            // Execute the command
            try {
                result = await client.Execute(command);
            }
            catch (Exception ex) {
                result = RpcCommandResult.FromFailure(command.ID,
                    new RpcFailure(RpcFailureType.RemoteException, ex.Message));
            }
            // Cache and return result
            server.CacheResult(result);
            return result;
        }

    }

}
