using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xenoage.RpcLib.Auth;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;
using Xenoage.RpcLib.Queue;

namespace Chat {

    public class Program {

        static async Task Main(string[] args) {

            Console.Write("Your user name: ");
            string username = Console.ReadLine();
            Console.Write("Your passwort (hint: use username in uppercase letters): ");
            string password = Console.ReadLine();
            Console.WriteLine("");
            Console.WriteLine("Type your message and press enter.");
            Console.WriteLine("Start the line with @foo to write a private message only to foo.");
            Console.WriteLine("Type exit to close the chat client.");
            Console.WriteLine("");

            var client = new RpcClient("ws://localhost:7000/chat/", new List<Type> { typeof(ChatClientRpc) },
                new RpcClientBasicAuth(username, password), new RpcPeerSettings {
                    DefaultOptions = new RpcOptions { TimeoutMs = 1000 },
                    ReconnectTimeMs = 5000,
                    Backlog = new JsonFileRpcBacklog(new DirectoryInfo("RpcBacklog"))
                });
            _ = client.Start();

            var server = new ChatServerRpcStub(client);

            bool isRunning = true;
            while (isRunning) {
                string message = Console.ReadLine();
                if (message == "exit") {
                    Console.WriteLine("Good bye.");
                    isRunning = false;
                } else {
                    try {
                        await server.SendPublicMessage(message);
                    } catch (RpcException ex) {
                        Console.WriteLine("Error: Could not send message: " + ex.Message);
                    }
                }
            }
            client.Stop();
        }

    }

}
