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
using RpcLib;
using Client.Rpc.Stubs;
using Shared.Rpc;
using Shared;

namespace Client {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "Client";

        static async Task Main(string[] args) {

            // First argument: client number
            int clientNumber = 0;
            if (args.Length > 0 && int.TryParse(args[0], out int it))
                clientNumber = it;
            else
                throw new Exception("Must be started with client number as parameter");
            clientID += "-" + clientNumber;

            // Welcome message
            Log.Write("Welcome to the test client: " + clientID);

            // RPC initialization
            var serverCalc = new CalcRpcStub();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcMain.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
                new CalcRpc()
            });

            // Each 0-100 ms, send a simple calculation task to the server: a + b = ?
            // a is an ascending number, starting from clientNumber * 1000
            // b is a random number between 1 and 100.
            // Write the calculations in the file "{clientID}.calclog" (used in the RpcLibTest project)
            string filename = $"{clientID}.calclog";
            File.Delete(filename);
            int a = clientNumber * 1000;
            var random = new Random();
            while (true) {
                long startTime = CoreUtils.TimeNow(); ;
                a++;
                int b = random.Next(1, 100);
                try {
                    int result = await serverCalc.AddNumbers(a, b);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"{a}+{b}={result} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"{a}+{b}=? | {runTime}ms | Fail: {ex.Type}: {ex.Message}");
                }
                await Task.Delay(random.Next(0, 100));
            }
        }

        public static void AuthenticateClient(HttpClient httpClient) {
            // Authentication as defined in the class DemoRpcAuth in the Server project
            var username = clientID;
            var password = clientID + "-PW";
            // Set HTTP Basic Auth header
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

    }

}
