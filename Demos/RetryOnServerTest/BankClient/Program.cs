using RpcLib.Model;
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
using BankShared.Rpc;
using DemoServer.Rpc;
using RpcLib.Peers.Client;

namespace BankClient {

    /// <summary>
    /// In this simple demo, this client plays the role of a bank.
    /// It listens to commands sent from the customer server.
    /// Yes, this is kind of weird setup, but it's just for testing the reverse direction
    /// compared to the RetryOnClientTest.
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
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcMain.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
                new BankClientRpc()
            }, defaultTimeoutMs: 1000, new DemoRpcCommandBacklog());

            // Run until killed
            while (true)
                await Task.Delay(1000);
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
