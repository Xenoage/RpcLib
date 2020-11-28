using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Auth;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class Program {

        static async Task Main(string[] args) {

            Console.Write("Your user name: ");
            string username = Console.ReadLine();
            Console.WriteLine("Type your message and press enter.");
            Console.WriteLine("Start the line with @foo to write a private message only to foo.");
            Console.WriteLine("Type exit to close the chat client.");

            var client = new RpcClient("ws://localhost:7000/chat", new List<RpcMethods> { new ChatClientRpc() },
                auth: new RpcClientBasicAuth(username, username), reconnectTimeMs: 5000, null);
            await client.Start();

            var server = new ChatServerRpcStub(client);

            bool isRunning = true;
            while (isRunning) {
                string message = Console.ReadLine();
                if (message == "exit") {
                    Console.WriteLine("Good bye.");
                    isRunning = false;
                } else {
                    await server.SendPublicMessage(message);
                }
            }
            await client.Stop();
        }

    }

}
