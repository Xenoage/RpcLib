using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Auth;
using Xenoage.RpcLib.Connections;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Main class of the RPC library on the client side.
    /// 
    /// The "client" is the peer which connects to the server peer over HTTPS to
    /// establish a websocket connection. This does not mean that only the client
    /// may send method calls. These are possible in both directions, once the
    /// connection is established.
    /// </summary>
    public class RpcClient : RpcPeer {

        /// <summary>
        /// The URL of the server endpoint, e.g. "ws://myserver.com:9998/rpc/".
        /// </summary>
        public string ServerUrl { get; }


        /// <summary>
        /// Creates a new RPC client-side endpoint with the given <see cref="ServerUrl"/>,
        /// local-side RPC methods (i.e. the methods which are executable on this client),
        /// authentication method, and other settings.
        /// </summary>
        public RpcClient(string serverUrl, IEnumerable<Type> localMethods, IRpcClientAuth auth,
                RpcPeerSettings settings) : base(localMethods, settings) {
            ServerUrl = serverUrl;
            this.auth = auth;
        }

        public override async Task Start() {
            while (false == stopper.IsCancellationRequested) {
                ClientWebSocket? webSocket = null;
                try {
                    webSocket = new ClientWebSocket();
                    auth.Authenticate(webSocket);
                    await webSocket.ConnectAsync(new Uri(ServerUrl), stopper.Token);
                    Log.Debug($"Connection to server established");
                    serverInfo = RpcPeerInfo.Server(ServerUrl);
                    var connection = new WebSocketRpcConnection(serverInfo, webSocket);
                    channel = await RpcChannel.Create(serverInfo, connection, this, Settings.Backlog);
                    await channel.Start();
                    Log.Debug($"Connection to server closed");
                } catch (Exception ex) {
                    if ((ex as WebSocketException)?.Message.Contains("401") ?? false)
                        Log.Debug($"Connection to server denied: Unauthorized");
                    else if (ex is WebSocketException wsEx)
                        Log.Debug($"Connection to server unexpectedly closed: " + wsEx.WebSocketErrorCode);
                    else
                        Log.Debug($"Connection to server unexpectedly closed: " + ex.Message);
                } finally {
                    webSocket?.Dispose();
                }

                if (false == stopper.IsCancellationRequested) {
                    // Reconnect
                    Log.Info($"Trying to reconnect after {Settings.ReconnectTimeMs} ms");
                    await Task.Delay(Settings.ReconnectTimeMs);
                    if (Settings.ReconnectTimeMs >= 30_000) // Repeat logging after a long pause
                        Log.Info($"Trying to reconnect now");
                }
            }
        }

        public override void Stop() {
            Log.Info("Stop requested");
            stopper.Cancel();
            channel?.Stop();
        }

        protected override RpcChannel? GetChannel(string? remotePeerID) {
            if (channel == null)
                throw new Exception("Not initialized");
            return channel;
        }

        protected override RpcContext CreateRpcContext(RpcPeerInfo callingPeer) =>
            RpcContext.OnClient(callingPeer);

        // Info on the connected server
        private RpcPeerInfo serverInfo;
        // The client's authentication technique
        private IRpcClientAuth auth;
        // The open channel to the server
        private RpcChannel? channel;
        // Used for stopping the loop
        private CancellationTokenSource stopper = new CancellationTokenSource();

    }

}
