using RpcLib.Model;
using RpcLib.Peers;
using RpcLib.Rpc.Utils;
using RpcLib.Server;
using System;
using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// Call this method at the beginning to enable the communication to the server, so that this client
        /// can both receive commands from the server and send commands to the server.
        /// </summary>
        /// <param name="client">The implementation of the RPC commands on the client</param>
        /// <param name="clientConfig">The settings of this client</param>
        /// <param name="authAction">An action which authenticates the used HTTP client, e.g. by adding HTTP Basic Auth
        /// information according to the client.</param>
        public static void Start(IRpcClient client, RpcClientConfig clientConfig, Action<HttpClient> authAction) {
            if (isRunning)
                return;
            isRunning = true;
            // Remember client and settings
            RpcClientEngine.client = client;
            RpcClientEngine.clientConfig = clientConfig;
            // Create and authorize HTTP client
            http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(RpcServerEngine.longPollingSeconds + 10); // Give some more seconds for timeout
            authAction(http);
            // Loop to pull the next command for this client from the server, execute it (if not already executed before)
            // and send the response together with the next pull.
            _ = Task.Run(async () => {
                RpcCommandResult? lastResult = null;
                while (isRunning) {
                    RpcCommand? next = null;
                    try {
                        next = await PullFromServer(lastResult);
                    }
                    catch {
                        // Could not reach server. Try the same result report again.
                        await Task.Delay(1000); // Wait a second before trying again
                    }
                    if (next != null) {
                        lastResult = await ExecuteLocallyNow(next);
                    }
                }
            });
            // Loop to execute (i.e. send to server) the next command in the queue.
            _ = Task.Run(async () => {
                while (isRunning) {
                    RpcCommand? next = await server.GetCurrentCommand(timeoutMs: -1); // No timeout
                    if (next != null) {
                        next.SetState(RpcCommandState.Sent);
                        try {
                            await ExecuteOnServerNow(next);
                            await server.FinishCurrentCommand();
                        }
                        catch {
                            // Could not reach server. Try the same command again.
                            await Task.Delay(1000); // Wait a second before trying again
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Runs the given RPC command on the server as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// See <see cref="RpcCommand.WaitForResult{T}"/>
        /// </summary>
        public static async Task<T> ExecuteOnServer<T>(RpcCommand command) {
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
        /// Call this method to stop the communication with the server as soon as possible.
        /// </summary>
        public static void Stop() {
            isRunning = false;
        }

        /// <summary>
        /// Sends the given command to the server now, reads the result or catches the
        /// exception, and sets the command's state accordingly.
        /// This method does only throw an exception, when there was a problem reaching the server.
        /// In this case, the command should be repeated. When there was an exception on the server,
        /// the command should not be repeated, and so we do not throw an exception here, but store
        /// the failure in the command's response state.
        /// </summary>
        private static async Task ExecuteOnServerNow(RpcCommand command) {
            var bodyJson = JsonLib.ToJson(command);
            var httpResponse = await http.PostAsync(clientConfig.ServerUrl + "/push",
                new StringContent(bodyJson, Encoding.UTF8, "application/json"));
            if (httpResponse.IsSuccessStatusCode) {
                // Response (either success or remote failure) received.
                var result = JsonLib.FromJson<RpcCommandResult>(await httpResponse.Content.ReadAsStringAsync());
                command.Finish(result);
            }
            else {
                // Remote exception.
                throw new Exception("Server responded with status code " + httpResponse.StatusCode);
            }
        }

        /// <summary>
        /// Polls the next command to execute locally from the server. The result of the
        /// last executed command must be given, if there is one. The returned Task may block
        /// some time, because the server uses the long polling technique to reduce network traffic.
        /// If the server can not be reached, an exception is thrown (because the last result
        /// has to be transmitted again).
        /// </summary>
        private static async Task<RpcCommand?> PullFromServer(RpcCommandResult? lastResult) {
            // Long polling. The server returns null after the long polling time.
            var bodyJson = lastResult != null ? JsonLib.ToJson(lastResult) : null;
            var httpResponse = await http.PostAsync(clientConfig.ServerUrl + "/pull",
                bodyJson != null ? new StringContent(bodyJson, Encoding.UTF8, "application/json") : null);
            if (httpResponse.IsSuccessStatusCode) {
                // Last result was received. The server responded with the next command or null,
                // if there is currently none.
                if (httpResponse.Content.Headers.ContentLength > 0)
                    return JsonLib.FromJson<RpcCommand>(await httpResponse.Content.ReadAsStringAsync());
                else
                    return null;
            }
            else {
                // Remote exception.
                throw new Exception("Server responded with status code " + httpResponse.StatusCode);
            }
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

        // Queue for the commands and cached command results of the server
        private static RpcPeerCache server = new RpcPeerCache();

        // HTTP client
        private static HttpClient http = new HttpClient();

        // RPC client methods and settings
        private static IRpcPeer client;
        private static RpcClientConfig clientConfig;

        // True, as long as the client engine is running
        private static bool isRunning = false;

    }

}
