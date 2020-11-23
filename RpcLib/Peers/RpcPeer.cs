using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Queue;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// For each client, one instance of this class is running both on the client
    /// and on the server.
    /// 
    /// It sends calls to the other side (server to client, or vice versa) and
    /// receives their response, and it receives calls from the other side,
    /// executes them and sends the responses.
    /// 
    /// This is done over a given websocket connection. When this connection is
    /// closed, it must be reestablished, i.e. a new instance of this class has
    /// to be launched, using the new connection.
    /// 
    /// The calls and responses are synchronized using a <see cref="Queue"/>.
    /// </summary>
    public class RpcPeer {

        /// <summary>
        /// Information on the connected remote peer.
        /// </summary>
        public PeerInfo RemoteInfo { get; private set; }

        /// <summary>
        /// Creates a new peer with the given information, already connected websocket,
        /// and optionally the given backlog.
        /// </summary>
        public async Task<RpcPeer> Create(PeerInfo remoteInfo, WebSocket webSocket,
                IRpcBacklog backlog) {
            var ret = new RpcPeer(remoteInfo, webSocket);
            ret.queue = await RpcQueue.Create(remoteInfo.PeerID, backlog);
            return ret;
        }

        /// <summary>
        /// Use <see cref="Create"/> for creating new instances.
        /// </summary>
        private RpcPeer(PeerInfo remoteInfo, WebSocket webSocket) {
            RemoteInfo = remoteInfo;
            this.webSocket = webSocket;
        }

        /// <summary>
        /// Starts the communication in a loop. The task returned by this method will be completed
        /// when either the connection is closed or after <see cref="Stop"/> is called.
        /// </summary>
        public async Task Start() {
            await Task.Run(async () => {
                try {
                    var rx = new ArraySegment<byte>(new byte[1024]);
                    var messagePart = new StringBuilder();
                    while (webSocket.State == WebSocketState.Open) {
                        // Receive call or response from remote side
                        var rxResult = await webSocket.ReceiveAsync(rx, cancellationToken.Token);
                        if (rxResult.MessageType == WebSocketMessageType.Close) {
                            // Closed by remote peer
                            Log.Debug($"Connection closed by {RemoteInfo}");
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        } else if (rxResult.MessageType == WebSocketMessageType.Text) {
                            // Received text message
                            string newData = Encoding.UTF8.GetString(rx);
                            messagePart.Append(newData);
                            if (rxResult.EndOfMessage) {
                                // Message is finished now
                                var message = messagePart.ToString();
                                Log.Trace($"Text message from {RemoteInfo}, {message.Length} bytes: {message}");
                                // 
                                /// Respond with the current time
                                /// await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString())),
                                ///    WebSocketMessageType.Text, endOfMessage: true, cancellationToken.Token);
                                // Reset message builder
                                messagePart.Clear();
                            } else {
                                // Wait for more data
                                Log.Trace($"Message part received from {RemoteInfo}");
                            }
                        }
                        // Close nicely
                        if (cancellationToken.IsCancellationRequested)
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);
                    }
                } catch (Exception ex) {
                    Log.Debug($"Connection unexpectedly closed by {RemoteInfo}");
                } finally {
                }
            });
        }

        /// <summary>
        /// Stops the communication.
        /// </summary>
        public async Task Stop() {
            cancellationToken.Cancel();
        }
 
        // The websocket connection
        private WebSocket webSocket;
        // Queue of calls to send to the other side
        private RpcQueue queue = null!; // Async set in Create
        // Cancellation token for stopping the loop
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();


    }

}
