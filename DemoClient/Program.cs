using DemoServer.Rpc;
using DemoShared;
using DemoShared.Model;
using RpcLib.Model;
using RpcLib.Server.Client;
using RpcLib.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using DemoShared.Rpc;
using RpcLib;

namespace DemoServer {

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

            // RPC initialization
            var serverDemo = new DemoServerRpcStub();
            var serverCalc = new CalcRpcStub();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcInit.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
                new DemoClientRpc(),
                new CalcRpc()
            });

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

            // Say hello to the server
            await serverDemo.SayHelloToServer(new Greeting { Name = "Andi" });

            // Each 0-100 ms, send a simple calculation task to the server: a + b = ?
            // a is an ascending number, starting from clientNumber * 1000
            // b is a random number between 1 and 100.
            // Write the calculations in the file "{clientID}.calclog" (used in the RpcLibTest project)
            string filename = $"{clientID}.calclog";
            File.Delete(filename);
            int a = clientNumber * 1000;
            var random = new Random();
            while (true) {
                long timeStart = CoreUtils.TimeNow(); ;
                a++;
                int b = random.Next(1, 100);
                try {
                    int result = await serverCalc.AddNumbers(a, b);
                    long rpcTime = CoreUtils.TimeNow() - timeStart;
                    var log = $"{a}+{b}={result} | {rpcTime} ms";
                    Log.Write(log);
                    Log.WriteToFile(filename, log);
                }
                catch (RpcException ex) {
                    long rpcTime = CoreUtils.TimeNow() - timeStart;
                    var log = $"{a}+{b}=? | {rpcTime} ms | Fail: {ex.Type}: {ex.Message}";
                    Log.Write(log);
                    Log.WriteToFile(filename, log);
                }
                await Task.Delay(random.Next(0, 100));
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

    }

}
