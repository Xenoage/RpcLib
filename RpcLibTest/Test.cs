using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RpcLibTest {

    [TestClass]
    public class Test {

        [TestMethod]
        public void Test100Clients() {

            // Remove all *.calclog files
            foreach (var file in new DirectoryInfo(".").GetFiles("*.calclog"))
                file.Delete();

            // Server (started later)
            Process server = null;

            // Start clients
            var clientsCount = 10;
            var clients = new List<Process>();
            for (int i = 0; i < clientsCount; i++) {

                // After first 5 clients, start server
                if (i == 5)
                    server = Launch("C:/Users/Andi/Documents/Projekte/KKB/Iovent-Material/Labs/RPC/DemoServer/bin/Debug/netcoreapp3.1/DemoServer.exe");

                var client = Launch("C:/Users/Andi/Documents/Projekte/KKB/Iovent-Material/Labs/RPC/DemoClient/bin/Debug/netcoreapp3.1/DemoClient.exe", $"{i}");
                clients.Add(client);
                Thread.Sleep(200);
            }

            Thread.Sleep(60 * 1000);
            server.Kill();
            foreach (var client in clients)
                client.Kill();
        }

        private static Process Launch(string path, string arguments = "") {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            psi.Arguments = arguments;
            /*psi.UseShellExecute = true;
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal;*/
            return Process.Start(psi);
        }

    }
}
