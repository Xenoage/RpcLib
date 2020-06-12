using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RpcLibTest {

    [TestClass]
    public class Test {

        [TestMethod]
        public void Test100Clients() {

            // Start server
            var server = Launch("C:/Users/Andi/Documents/Projekte/KKB/Iovent-Material/Labs/RPC/DemoServer/bin/Debug/netcoreapp3.1/DemoServer.exe");

            Thread.Sleep(5000);

            // Start clients
            var clientsCount = 100;
            var clients = new List<Process>();
            for (int i = 0; i < clientsCount; i++) {
                var client = Launch("C:/Users/Andi/Documents/Projekte/KKB/Iovent-Material/Labs/RPC/DemoClient/bin/Debug/netcoreapp3.1/DemoClient.exe");
                clients.Add(client);
                Thread.Sleep(200);
            }

            Thread.Sleep(20 * 1000);
            server.Kill();
            foreach (var client in clients)
                client.Kill();
        }

        private static Process Launch(string path) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            //psi.UseShellExecute = true;
            //psi.CreateNoWindow = false;
            //psi.WindowStyle = ProcessWindowStyle.Normal;
            return Process.Start(psi);
        }

    }
}
