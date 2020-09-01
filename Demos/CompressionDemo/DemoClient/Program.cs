using DemoShared;
using DemoShared.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using RpcLib;
using DemoClient.Rpc.Stubs;
using RpcLib.Peers.Client;
using DemoShared.Rpc;

namespace DemoClient {

    /// <summary>
    /// Simple demo client. Sends texts using the different compression strategies
    /// and message sizes to the server and receives back the capitalized versions.
    /// </summary>
    public class Program {

        private static string clientID = "DemoClient";

        static async Task Main(string[] args) {
            // Welcome message
            Log.Write("Welcome to the compression demo client!");
            Log.Write("Please use a tool like Fiddler to check the transmitted requests and responses.\n");

            // RPC initialization
            var server = new TextRpcStub();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcMain.InitRpcClient(demoRpcConfig, AuthenticateClient, () => new List<RpcFunctions> {
                new TextRpc()
            }, new RpcSettings {
                CompressionThresholdBytes = 1500 // Auto-compress messages >= 1.5 kB in this test
            });

            // Compression strategies
            var strategies = new List<(string, Func<string, Task<string>>)> {
                ("Auto", server.CapitalizeText),
                ("Compressed", server.CapitalizeText_Compressed),
                ("Uncompressed", server.CapitalizeText_Uncompressed)
            };
            // Message in different sizes
            var texts = TestText.GetDemoTexts();

            // Call server
            foreach (var strategy in strategies) {
                Log.Write("Using strategy: " + strategy.Item1);
                foreach (var text in texts) {
                    Log.Write($"  Sending text with {text.Length} chars");
                    var result = await strategy.Item2.Invoke(text);
                    // Check result
                    if (result.Length != text.Length)
                        Log.Write("WARNING: Unexpected result: different length");
                    else if (result != text.ToUpper())
                        Log.Write("WARNING: Unexpected result: wrong content");
                    // Wait a second before the next call
                    await Task.Delay(1000);
                }
            }

            // Stay open, because the server also sends test messages to this client.
            // This will need about 30 seconds.
            await Task.Delay(30_000);

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
