using DemoClient.Rpc;
using DemoShared;
using DemoShared.Model;
using RpcLib.Client;
using RpcLib.Model;
using RpcLib.Rpc.Utils;
using RpcLib.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DemoClient {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "DemoClient-" + new Random().Next(1000);

        static async Task Main(string[] args) {
            Log.Write("Welcome to the RPCLib Demo Client");
            // Connect to the server
            var server = new DemoRpcServerStub();
            var client = new DemoRpcClient();
            var demoRpcConfig = new RpcClientConfig {
                ClientID = clientID,
                ServerUrl = "http://localhost:5000/rpc"
            };
            RpcClientEngine.Start(client, demoRpcConfig, AuthenticateClient);
            // Each few seconds, send commands to the server. Log the result.
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
