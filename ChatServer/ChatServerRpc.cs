using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Chat {

    public class ChatServerRpc : RpcMethods, IChatServerRpc {

        public async Task SendPublicMessage(string message) {
            var clients = Context.Clients!;
            Console.WriteLine($"Deploy received message '{message}' to all other {clients.Count - 1} clients");
            foreach (var client in clients) {
                // Do not send the message to the sending client
                if (client.PeerID != Context.RemotePeer.PeerID)
                    await new ChatClientRpcStub(Program.server, client.PeerID!).ReceiveMessage(message, Context.RemotePeer.PeerID!);
            }
        }

        public Task<bool> SendPrivateMessage(string message, string username) {
            Console.WriteLine($"TODO: Deploy received message '{message}' to only {username}");
            return Task.FromResult(true);
        }

        /// <summary>
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
