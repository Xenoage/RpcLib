using BankClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcLib.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RpcLibTest {

    /// <summary>
    /// Test of the different retry strategies, see <see cref="RpcRetryStrategy"/>.
    /// Here, in "RetryOnServerTest", the bank is the client behind the firewall and the customer is the server.
    /// To test the other way, see the "RetryOnClientTest" demo.
    /// 
    /// See the comments within this test what we do and what we expect.
    /// </summary>
    [TestClass]
    public class RetryOnServerTest {

        [TestInitialize]
        public void Cleanup() {
            // Remove all *.banklog files
            foreach (var file in thisDir.GetFiles("*.banklog"))
                file.Delete();
        }

        [TestMethod]
        public void TestRetry() {

            // Start server. It uses a default timeout of 1 second.
            // Each second, it sends an increasing amount (1 ct, 2ct, 3ct, ...)
            // to the bank (client number = 0), which is still offline. This is done for 10 seconds.
            client = LaunchServer();

            // After about 25 seconds, the client starts. It uses a default timeout of 1 second.
            _ = Task.Run(async () => {
                await Task.Delay(25000);
                server = LaunchClient(0);
            });

            // After about 45 seconds, we close the client and the server.
            Thread.Sleep(45000);
            client.Kill();
            server.Kill();

            // Check the log files
            // TODO: evaluate
        }

        private Process LaunchServer() =>
            Launch(Path.Combine(baseDir.FullName, "BankServer/bin/Debug/netcoreapp3.1/BankServer.exe"));

        private Process LaunchClient(int number) =>
            Launch(Path.Combine(baseDir.FullName, "BankClient/bin/Debug/netcoreapp3.1/BankClient.exe"), $"{number}");

        private Process Launch(string path, string arguments = "") {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            psi.Arguments = arguments;
            psi.UseShellExecute = true;
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            return Process.Start(psi);
        }

        private Process server = null;
        private Process client = null;

        private DirectoryInfo thisDir = new DirectoryInfo(".");
        private DirectoryInfo baseDir = new DirectoryInfo(".").Parent.Parent.Parent.Parent;

    }
}
