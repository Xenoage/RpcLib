using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RpcLibTest {

    /// <summary>
    /// Simple load test. One server is started and several hundreds of clients are spawned.
    /// Simple calculation tasks are sent between the peers and are logged in files.
    /// After some time, all peers are closed and the log files are evaluated. Sine the numbers of the
    /// calculations follow a certain rule, it can be checked that no command was left out or executed twice.
    /// </summary>
    [TestClass]
    public class LoadTest {

        [TestMethod]
        public void TestCalculations() {

            // Number of clients.
            // Can be increased on machines with much RAM and computing power. This is not a limitation of RPCLib
            // (it should work with thousands of clients), but of the test machine which has to run lots of
            // individual .NET Core App instances.
            var clientsCount = 100; 

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
                        "Server/bin/Debug/netcoreapp3.1/Server.exe"));

                var client = Launch(Path.Combine(baseDir.FullName,
                    "Client/bin/Debug/netcoreapp3.1/Client.exe"), $"{i}");
                clients.Add(client);
                Thread.Sleep(200);
            }

            // Run for 30 seconds, then close all processes
            Thread.Sleep(30 * 1000);
            server.Kill();
            foreach (var client in clients)
                client.Kill();

            // TODO: evaluate
        }

        private static Process Launch(string path, string arguments = "") {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            psi.Arguments = arguments;

            // Uncomment the following lines to make all program consoles visible (very slow!)
            /*
            psi.UseShellExecute = true;
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal; //*/

            return Process.Start(psi);
        }

    }
}
