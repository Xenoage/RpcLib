using DemoServer.Rpc;
using DemoShared;
using DemoShared.Model;
using RpcLib.Model;
using RpcLib.Server.Client;
using RpcLib.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using DemoShared.Rpc;
using RpcLib;
using DemoClient.Rpc.Stubs;

namespace DemoServer {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "DemoClient";

        static async Task Main(string[] args) {
            // Welcome message
            Log.Write("Welcome to the simple demo client!");

            // RPC initialization, using two server function classes
            var serverDemo = new DemoServerRpcStub();
            var serverCalc = new CalcRpcStub();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcMain.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
                new DemoClientRpc(),
                new CalcRpc()
            });

            // Each few seconds, send commands to the server. Log the result.
            var random = new Random();
            while (true) {
                try {

                    // Send greeting
                    Log.Write("Sending greeting...");
                    var greeting = new Greeting { Name = "Andi",
                        MoreData = new SampleData { Text = $"Hi server, now it is {DateTime.Now}" }};
                    await serverDemo.SayHelloToServer(greeting);

                    // Send calculation task. May fail on the remote side, when there is division by zero.
                    Log.Write("Successfully greeted. Now sending a little calculation task:");
                    int a = random.Next(1, 100);
                    int b = random.Next(0, 10);
                    long startTime = CoreUtils.TimeNow();
                    try {
                        int result = await serverCalc.DivideNumbers(a, b);
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.Write($"{a}/{b}={result} (runtime: {runTime}ms)");
                    }
                    catch (RpcException ex) {
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.Write($"{a}+{b}=Fail! (runtime: {runTime}ms; {ex.Type}: {ex.Message})");
                    }
                }
                catch (RpcException ex) {
                    Log.Write("Error: " + ex.Failure.Type + ": " + ex.Message);
                }

                // Wait a second before the next round
                await Task.Delay(1000);
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
