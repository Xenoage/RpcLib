using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Chat {

    public class ChatServerRpc : RpcMethods, IChatServerRpc {

        public async Task SendPublicMessage(string message) {
            var clients = Context.Clients!;
            Console.WriteLine($"Deploy received message '{message}' to all other {clients.Count - 1} clients");
            var chatMessage = new ChatMessage {
                Text = message,
                Sender = Context.RemotePeer.PeerID!
            };
            // Option 1: Send as method invocation to the remote peer
            foreach (var client in clients) {
                // Do not send the message to the sending client
                if (client.PeerID != Context.RemotePeer.PeerID)
                    await new ChatClientRpcStub(Program.server, client.PeerID!).ReceiveMessage(chatMessage);
            }
            // Option 2: Send as event to the remote peer, if it registered for it
            MessageReceived(chatMessage);
        }

        // TODO: Auto-generate
        public ChatServerRpc() {
            MessageReceived += OnMessageReceived;
        }

        public Task<bool> SendPrivateMessage(string message, string username) {
            Console.WriteLine($"TODO: Deploy received message '{message}' to only {username}");
            return Task.FromResult(true);
        }

        // TODO: Auto-generate - not used on the local side.
        public event Action<ChatMessage> MessageReceived = delegate { };

        // TODO: Auto-generate
        private void OnMessageReceived(ChatMessage message) {
            var clients = Context.Clients!;
            foreach (var client in clients) {
                // Do not send the message to the sending client
                if (client.PeerID != Context.RemotePeer.PeerID)
                    _ = new ChatClientRpcStub(Program.server, client.PeerID!).OnMessageReceived(message);
            }  
        }

        /// <summary>
        /// TODO: Auto-generate.
        /// Mapping of <see cref="RpcMethod"/> to real method calls (just boilerplate code;
        /// we could auto-generate this method later in .NET 5 with source generators)
        /// </summary>
        public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {
            "SendPublicMessage" => SendPublicMessage(method.GetParam<string>(0)).Serialize(),
            "SendPrivateMessage" => SendPrivateMessage(method.GetParam<string>(0), method.GetParam<string>(1)).Serialize(),
            _ => null
        };

        
    }

}
