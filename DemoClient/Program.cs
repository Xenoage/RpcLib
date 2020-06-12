using RpcLib.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DemoClient {

    /// <summary>
    /// Simple demo client. Periodically calls methods on the server
    /// and prints the results on the console.
    /// </summary>
    public class Program {

        private static string clientID = "DemoClient";

        static void Main(string[] args) {
            RpcClientEngine.Start(AuthorizeClient);
        }

        public static void AuthorizeClient(HttpClient httpClient) {
            // Authentication as defined in the class DemoRpcAuth in the DemoServer project
            var username = clientID;
            var password = clientID + "-PW";
            // Set HTTP Basic Auth header
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

    }

}
