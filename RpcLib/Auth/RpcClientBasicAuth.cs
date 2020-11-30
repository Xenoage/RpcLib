using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
namespace Xenoage.RpcLib.Auth {

    /// <summary>
    /// Implementation of <see cref="IRpcClientAuth"/> using
    /// a Basic Authentication header in the first HTTP request.
    /// </summary>
    public class RpcClientBasicAuth : IRpcClientAuth {

        /// <summary>
        /// Creates a new HTTP Basic Auth based authentication using the given credentials.
        /// </summary>
        public RpcClientBasicAuth(string username, string password) {
            this.username = username;
            this.password = password;
        }

        public void Authenticate(ClientWebSocket webSocket) {
            webSocket.Options.SetRequestHeader("Authorization", "Basic " +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));
        }

        private string username;
        private string password;

    }

}
