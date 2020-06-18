using DemoServer.Utils;
using Microsoft.AspNetCore.Http;
using RpcLib.Peers.Server;

namespace DemoServer.Rpc {

    /// <summary>
    /// Simple demo implementation of the <see cref="IRpcAuth"/> interface, using the information
    /// in the HTTP Basic Auth header.
    /// The username is the client ID, which is accepted, when the password matches "{clientID}-PW".
    /// </summary>
    public class DemoRpcAuth : IRpcAuth {

        public string? GetClientID(HttpRequest request) {
            if (Credentials.FromBasicAuth(request) is Credentials credentials) {
                string clientID = credentials.Username;
                if (credentials.Password != $"{clientID}-PW")
                    return null; // Wrong password
                return clientID;
            }
            return null; // No HTTP Basic Auth header found
        }

    }

}
