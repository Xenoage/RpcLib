using System;
using System.Threading.Tasks;

namespace Chat {

    public class ChatServerRpc : ChatServerRpcBase, IChatServerRpc {

        public override async Task SendPublicMessage(string message) {
            var clients = Context.Clients!;
            Console.WriteLine($"Deploy received message '{message}' to all other {clients.Count - 1} clients");
            foreach (var client in clients) {
                // Do not send the message to the sending client
                if (client.PeerID != Context.RemotePeer.PeerID)
                    await new ChatClientRpcStub(Program.server, client.PeerID!).ReceiveMessage(message, Context.RemotePeer.PeerID!);
            }
        }

        public override Task<bool> SendPrivateMessage(string message, string username) {
            Console.WriteLine($"TODO: Deploy received message '{message}' to only {username}");
            return Task.FromResult(true);
        }
        
    }

}
