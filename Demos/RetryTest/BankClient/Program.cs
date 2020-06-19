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
using BankShared;
using BankClient.Rpc.Stubs;
using BankShared.Rpc;

namespace BankClient {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "BankClient";

        static async Task Main(string[] args) {

            // First argument: client number (otherwise 0)
            int clientNumber = 0;
            if (args.Length > 0 && int.TryParse(args[0], out int it))
                clientNumber = it;
            clientID += "-" + clientNumber;

            // Welcome message
            Log.Write("Welcome to the test client: " + clientID);

            // RPC initialization
            var bankServer = new BankServerRpcStub();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcMain.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
            }, defaultTimeoutMs: 2000, new DemoRpcCommandBacklog());

            // See the RetryTest test project to understand what we are doing now.

            // Send an increasing amount(1 ct, 2ct, 3ct, ...)
            // to the bank, which is still offline. This is done for 10 seconds.
            string filename = $"{clientID}.banklog";
            for (int i = 1; i <= 10; i++) {

                long startTime = CoreUtils.TimeNow(); ;
                try {
                    int newCents = await bankServer.AddMoney(clientNumber, i);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"{i} | {newCents} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"{i} | Fail: {ex.Type}: {ex.Message}");
                }
                await Task.Delay(1000);
            }

            // Finished. Close client.
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
