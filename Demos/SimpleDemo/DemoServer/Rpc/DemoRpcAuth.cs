using Microsoft.AspNetCore.Http;
using RpcLib.Auth;
using System;

namespace DemoServer.Rpc {

    /// <summary>
    /// Simple demo implementation of the <see cref="IRpcAuth"/> interface, using the information
    /// in the HTTP Basic Auth header.
    /// The username is the client ID, which is accepted, when the password matches "{clientID}-PW".
    /// </summary>
    public class DemoRpcAuth : IRpcAuth {

        public AuthResult Authenticate(HttpRequest request) {
            if (Credentials.FromBasicAuth(request) is Credentials credentials) {
                string clientID = credentials.Username;
                bool success = credentials.Password == $"{clientID}-PW";
                return new AuthResult(clientID, success);
            }
            return new AuthResult(null, false); // No HTTP Basic Auth header found
        }

    }

}
