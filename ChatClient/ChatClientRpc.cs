using System;
using System.Threading.Tasks;

namespace Chat {

    public class ChatClientRpc : ChatClientRpcBase, IChatClientRpc {

        public override Task ReceiveMessage(string message, string username) {
            Console.WriteLine(username + ": " + message);
            return Task.CompletedTask;
        }

    }

}
