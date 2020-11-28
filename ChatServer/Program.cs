using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Auth;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class Program {

        static async Task Main(string[] args) {

            Console.Write("Starting the chat server. Kill the process to stop it.");

            /*var server = new RpcServer("ws://localhost:7000/chat", new List<RpcMethods> { new ChatClientRpc() },
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
            await client.Stop();*/
        }

    }

}