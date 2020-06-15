using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RpcLibTest {

    /// <summary>
    /// Test based on the demo projects. One server is started and several hundreds of clients are spawned.
    /// Simple calculation tasks are sent between the peers and are logged in files.
    /// After some time, all peers are closed and the log files are evaluated. Sine the numbers of the
    /// calculations follow a certain rule, it can be checked that no command was left out or executed twice.
    /// </summary>
    [TestClass]
    public class Test {

        [TestMethod]
        public void TestCalculations() {

            // Number of clients.
            // Can be increased to 500 e.g. on a machine with 32GB RAM. This is not a limitation of RPCLib
            // (it should work with thousands of clients), but of the test machine which has to run hundreds of
            // individual .NET Core App instances.
            var clientsCount = 10; 

            // Remove all *.calclog files
            var thisDir = new DirectoryInfo(".");
            foreach (var file in thisDir.GetFiles("*.calclog"))
                file.Delete();

            // Server (started later)
            Process server = null;

            // Start clients
            var clients = new List<Process>();
            var baseDir = new DirectoryInfo(thisDir.FullName).Parent.Parent.Parent.Parent;
            for (int i = 0; i < clientsCount; i++) {

                // After first 20% of clients, start server
                if (i == clientsCount / 5)
                    server = Launch(Path.Combine(baseDir.FullName,
                        "DemoServer/bin/Debug/netcoreapp3.1/DemoServer.exe"));

                var client = Launch(Path.Combine(baseDir.FullName,
                    "DemoClient/bin/Debug/netcoreapp3.1/DemoClient.exe"), $"{i}");
                clients.Add(client);
                Thread.Sleep(200);
            }

            // Run for 10 seconds, then close all processes
            Thread.Sleep(10 * 1000);
            server.Kill();
            foreach (var client in clients)
                client.Kill();

            // TODO: evaluate
        }

        private static Process Launch(string path, string arguments = "") {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            psi.Arguments = arguments;

            // Uncomment the following lines to make all program consoles visible (slow!)
            /*
            psi.UseShellExecute = true;
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal; //*/

            return Process.Start(psi);
        }

    }
}
