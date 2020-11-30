using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Auth;
using Xenoage.RpcLib.Connections;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Peers {

    public class RpcServer : RpcPeer {

        /// <summary>
        /// The URL where this server listens for clients to connect,
        /// e.g. "http://localhost.com:9998/rpc/".
        /// </summary>
        public string ServerUrl { get; }

        /// <summary>
        /// Creates a new RPC server with the given <see cref="ServerUrl"/>, local-side RPC methods
        /// (i.e. the methods which are executable on this server),
        /// authentication verification method and <see cref="DefaultOptions"/>.
        /// </summary>
        public RpcServer(string serverUrl, IEnumerable<Type> localMethods, IRpcServerAuth auth,
                RpcOptions defaultOptions) : base(localMethods, defaultOptions) {
            ServerUrl = serverUrl;
            this.auth = auth;
        }

        public override async Task Start() {
            // Listen for clients
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(ServerUrl);
            httpListener.Start();
            Log.Info("Server started. Listening for clients.");

            // Accept new clients
            while (false == stopper.IsCancellationRequested) {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest) {
                    _ = ProcessClient(context);
                } else {
                    context.Close(HttpStatusCode.BadRequest);
                }
            }
        }

        private async Task ProcessClient(HttpListenerContext httpContext) {
            string ip = httpContext.GetIP();

            // Check authentication
            var authResult = auth.Authenticate(httpContext.Request);
            if (false == authResult.Success || authResult.ClientID == null) {
                Log.Debug($"Connection from {ip} denied" +
                    (authResult.ClientID != null ? $" (client ID {authResult.ClientID})" : ""));
                httpContext.Close(HttpStatusCode.Unauthorized);
                return;
            }
            string clientID = authResult.ClientID;

            // Accept web socket
            var clientInfo = RpcPeerInfo.Client(clientID, ip);
            WebSocketContext context;
            try {
                context = await httpContext.AcceptWebSocketAsync(subProtocol: null);
                Log.Debug($"Connected {clientInfo}");
            } catch (Exception ex) {
                Log.Debug($"Could not accept WebSocket to {clientInfo}: {ex.Message}");
                httpContext.Close(HttpStatusCode.InternalServerError);
                return;
            }

            // WebSocket loop
            WebSocket webSocket = context.WebSocket;
            try {
                var connection = new WebSocketRpcConnection(clientInfo, webSocket);
                var channel = await RpcChannel.Create(clientInfo, connection, this, backlog: null); // GOON: backlog
                if (channelsByClientID.TryGetValue(clientID, out var oldChannel)) {
                    Log.Debug($"Channel for client {clientID} was already open; close it and open a new one.");
                    oldChannel.Stop();
                }
                channelsByClientID[clientID] = channel;
                await channel.Start();
                Log.Debug($"Connection to {clientInfo} closed");
            } catch (Exception ex) {
                if (ex is WebSocketException wsEx)
                    Log.Debug($"Connection to {clientInfo} unexpectedly closed: " + wsEx.WebSocketErrorCode);
                else
                    Log.Debug($"Connection to {clientInfo} unexpectedly closed: " + ex.Message);
            } finally {
                webSocket?.Dispose();
            }
            channelsByClientID.Remove(clientID);
        }

        public override void Stop() {
            Log.Info("Stop requested");
            stopper.Cancel();
            httpListener?.Stop();
            foreach (var channel in new List<RpcChannel>(channelsByClientID.Values))
                channel.Stop();
        }

        protected override RpcChannel? GetChannel(string? remotePeerID) {
            if (remotePeerID != null && channelsByClientID.TryGetValue(remotePeerID, out var channel))
                return channel;
            else
                return null;
        }

        protected override RpcContext CreateRpcContext(RpcPeerInfo callingPeer) =>
            RpcContext.OnServer(callingPeer, 
                channelsByClientID.Values.Select(it => it.RemotePeer).ToList());

        // Listener for new connections
        private HttpListener? httpListener;
        // The server's authentication technique to verify clients
        private IRpcServerAuth auth;
        // The currently connected clients (client ID is key). Per client ID, only one channel is open.
        // When the same client connects again, the old channel (if still open) is closed.
        private Dictionary<string, RpcChannel> channelsByClientID =
            new Dictionary<string, RpcChannel>();
        // Used for stopping the loop
        private CancellationTokenSource stopper = new CancellationTokenSource();

    }

}
