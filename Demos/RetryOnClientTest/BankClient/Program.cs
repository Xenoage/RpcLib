using RpcLib.Model;
using RpcLib.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using RpcLib;
using BankShared;
using BankClient.Rpc.Stubs;
using BankShared.Rpc;
using RpcLib.Peers.Client;

namespace BankClient {

    /// <summary>
    /// In this simple demo, this client plays the role of a bank customer.
    /// It periodically calls methods on the bank server and prints the results on the console.
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
            }, new RpcSettings {
                TimeoutMs = 1000
            }, new DemoRpcCommandBacklog());

            // See the RetryOnClientTest test project to understand what we are doing now.

            // Repeatedly, get the current account balance and send an increasing amount (1 ct, 2ct, 3ct, ...)
            // to the bank, which is still offline at the beginning. This is done for 20 seconds.
            // Each 5 seconds, change the owner name.
            string filename = $"{clientID}.banklog";
            // TODO bankServer.OnAddMoneyRetryFinished = (command) =>
            //    Log.WriteToFile(filename, $"{command.GetParam<int>(1)} | {command.GetResult().ResultJson} | retried");
            for (int i = 1; i <= 20; i++) {

                // Get current balance (no retry!)
                long startTime = CoreUtils.TimeNow();
                try {
                    int newCents = await bankServer.GetBalance(clientNumber);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Now | {newCents} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Now | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                }

                // Add money (retry for each command)
                startTime = CoreUtils.TimeNow(); ;
                try {
                    int newCents = await bankServer.AddMoney(clientNumber, i);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Add | {i} | {newCents} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Add | {i} | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                }

                // Change owner name (retry for newest call of command)
                if (i % 5 == 0) {
                    startTime = CoreUtils.TimeNow();
                    string newName = "MyName-" + (i / 5);
                    try {
                        await bankServer.ChangeOwnerName(clientNumber, newName);
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.WriteToFile(filename, $"Name | {newName} | {runTime}ms");
                    }
                    catch (RpcException ex) {
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.WriteToFile(filename, $"Name | {newName} | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                    }
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
