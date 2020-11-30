using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;
using Xenoage.RpcLib.Queue;

namespace Chat {

    public class Program {

        public static RpcServer server;

        static async Task Main(string[] args) {

            Console.Write("Starting the chat server. Kill the process to stop it.");

            server = new RpcServer("http://localhost:7000/chat/", new List<Type> { typeof(ChatServerRpc) },
                new ChatAuth(), new RpcPeerSettings {
                    DefaultOptions = new RpcOptions { TimeoutMs = 1000 },
                    Backlog = new JsonFileRpcBacklog(new DirectoryInfo("RpcBacklog"))
                });
            await server.Start();

        }

    }

}