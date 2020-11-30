using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class Program {

        public static RpcServer server;

        static async Task Main(string[] args) {

            Console.Write("Starting the chat server. Kill the process to stop it.");

            server = new RpcServer("http://localhost:7000/chat/", new List<Type> { typeof(ChatServerRpc) },
                auth: new ChatAuth(), new RpcOptions { TimeoutMs = 1000 });
            await server.Start();

        }

    }

}