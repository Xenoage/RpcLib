using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Connections {


    public class WebSocketRpcConnection : IRpcConnection {

        public WebSocketRpcConnection(RpcPeerInfo remoteInfo, WebSocket webSocket) {
            this.remoteInfo = remoteInfo;
            this.webSocket = webSocket;
        }

        public bool IsOpen() =>
            webSocket.State == WebSocketState.Open;

        public async Task Send(RpcMessage message, CancellationToken cancellationToken) =>
            await webSocket.SendAsync(new ArraySegment<byte>(message.Data),
                    WebSocketMessageType.Binary, endOfMessage: true, cancellationToken);

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            var messagePart = new MemoryStream();
            while (IsOpen()) {
                // Receive message from remote side
                var received = await webSocket.ReceiveAsync(buffer, cancellationToken);
                if (received.MessageType == WebSocketMessageType.Close) {
                    // Closed by remote peer
                    Log.Debug($"Connection closed by {remoteInfo}");
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);
                } else if (received.MessageType == WebSocketMessageType.Binary) {
                    // Received message
                    messagePart.Write(buffer);
                    if (received.EndOfMessage) {
                        // Message is finished now
                        var message = RpcMessage.FromData(messagePart.ToArray());
                        return message;
                    } else {
                        // Wait for more data
                        Log.Trace($"Message part received from {remoteInfo}, {buffer.Count} bytes");
                    }
                }
            }
            return null;
        }

        public async Task Close() =>
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);

        private RpcPeerInfo remoteInfo;
        private WebSocket webSocket;
        private ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

    }
}
