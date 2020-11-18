using System;
using System.Net;
using System.Net.Http;

namespace Xenoage.RpcLib.Utils {

    /// <summary>
    /// Helpers for HTTP communication tests.
    /// </summary>
    public class HttpTestUtils {

        private const int port = 9999;

        // Avoid multiple calls at the same time
        private static object listenerLock = new object();

        /// <summary>
        /// Creates "real" <see cref="HttpListenerRequest"/>s by opening an <see cref="HttpListener"/>,
        /// sending a given request to this listener, receiving and returning it.
        /// </summary>
        public static HttpListenerRequest CreateHttpListenerRequest(Action<HttpClient>? modifyClient = null) {
            lock (listenerLock) {
                string uri = $"http://localhost:{port}/";
                // Start listener
                using var server = new HttpListener();
                server.Prefixes.Add(uri);
                server.Start();
                // Create client and send request
                using var client = new HttpClient();
                modifyClient?.Invoke(client);
                client.GetAsync(uri);
                // Receive request and return it
                var context = server.GetContext();
                return context.Request;
            }
        }

    }

}
