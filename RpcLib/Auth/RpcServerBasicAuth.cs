using System;
using System.Net;
using System.Text;
using Xenoage.RpcLib.Logging;

namespace Xenoage.RpcLib.Auth {

    /// <summary>
    /// Implementation of <see cref="IRpcServerAuth"/> using
    /// a Basic Authentication header in the HTTP request.
    /// Implement the <see cref="AreCredentialsCorrect"/> method with your own logic,
    /// e.g. comparing the credentials with database entries.
    /// </summary>
    public abstract class RpcServerBasicAuth : IRpcServerAuth {

        /// <summary>
        /// Returns true, iff the given credentials are correct.
        /// Override this method
        /// </summary>
        public abstract bool AreCredentialsCorrect(string username, string password);

        public AuthResult Authenticate(HttpListenerRequest request) {
            string? username = null;
            try {
                string[] authHeader = request.Headers.Get("Authorization")?.Split(" ")
                    ?? throw new Exception("Authentication header missing");
                if (authHeader.Length != 2 || authHeader[0] != "Basic")
                    throw new Exception("Basic Authentication mssing");
                byte[] credentialBytes = Convert.FromBase64String(authHeader[1]);
                string[] credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                username = credentials[0];
                string password = credentials[1];
                if (false == AreCredentialsCorrect(username, password))
                    throw new Exception("Incorrect credentials");
                RpcMain.Log($"Client authentication of user {username} successful", LogLevel.Debug);
                return new AuthResult(username, success: true);
            } catch (Exception ex) {
                RpcMain.Log($"Client authentication {(username != null ? $"of user {username} " : "")} " +
                    $"failed: {ex.Message}", LogLevel.Debug);
                return new AuthResult(username, success: false);
            }
        }

    }

}
