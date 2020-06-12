using DemoClient.Rpc;
using DemoShared;
using DemoShared.Model;
using RpcLib.Client;
using RpcLib.Model;
using RpcLib.Rpc.Utils;
using RpcLib.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace DemoClient {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "DemoClient";

        static async Task Main(string[] args) {
            // First argument: client number (used in the RpcLibTest project), otherwise 0
            int clientNumber = 0;
            if (args.Length > 0 && int.TryParse(args[0], out int it))
                clientNumber = it;
            clientID += "-" + clientNumber;
            // Welcome message
            Log.Write("Welcome to the RPCLib Demo Client: " + clientID);
            // Connect to the server
            var server = new DemoRpcServerStub();
            var client = new DemoRpcClient();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcClientEngine.Start(client, demoRpcConfig, AuthenticateClient);
            /* // Each few seconds, send commands to the server. Log the result.
            var random = new Random();
            while (true) {
                try {
                    Log.Write("Sending greeting...");
                    var greeting = new Greeting { Name = $"from {clientID} at " + CoreUtils.TimeNow() };
                    await server.SayHelloToServer(greeting);
                    Log.Write("Successfully greeted: " + JsonLib.ToJson(greeting));
                }
                catch (RpcException ex) {
                    Log.Write("Error when greeting: " + ex.Failure.Type + ": " + ex.Message);
                }
                await Task.Delay(random.Next(4000, 6000));
            } */

            // Each 0-1000 ms, send a simple calculation task to the server: a + b = ?
            // a is an ascending number, starting from clientNumber * 1000
            // b is a random number between 1 and 100.
            // Write the calculations in the file "{clientID}.calclog" (used in the RpcLibTest project)
            string filename = $"{clientID}.calclog";
            File.Delete(filename);
            int a = clientNumber * 1000;
            var random = new Random();
            while (true) {
                try {
                    a++;
                    int b = random.Next(1, 100);
                    Log.Write($"Sending calculation: {a} + {b} = ?");
                    long timeStart = CoreUtils.TimeNow();
                    int result = await server.AddNumbers(a, b);
                    long rpcTime = CoreUtils.TimeNow() - timeStart;
                    Log.Write($"Result received: {result}");
                    LogToFile(filename, $"{a}+{b}={result}//responsetime={rpcTime}ms");
                }
                catch (RpcException ex) {
                    Log.Write("Error when sending calculation: " + ex.Failure.Type + ": " + ex.Message);
                    LogToFile(filename, "error:" + ex.Failure.Type + ":" + ex.Message);
                }
                await Task.Delay(random.Next(0, 1000));
            }
        }

        public static void AuthenticateClient(HttpClient httpClient) {
            // Authentication as defined in the class DemoRpcAuth in the DemoServer project
            var username = clientID;
            var password = clientID + "-PW";
            // Set HTTP Basic Auth header
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        private static void LogToFile(string filename, string line) {
            File.AppendAllText(filename, line + "\n");
        }

    }

}
