using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Chat {

    public class ChatClientRpc : RpcMethods, IChatClientRpc {

        public Task ReceiveMessage(string message, string username) {
            Console.WriteLine(username + ": " + message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Mapping of <see cref="RpcMethod"/> to real method calls (just boilerplate code;
        /// we could auto-generate this method later in .NET 5 with source generators)
        /// </summary>
        public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {
            "ReceiveMessage" => ReceiveMessage(method.GetParam<string>(0), method.GetParam<string>(1)).Serialize(),
            _ => null
        };

    }

}
