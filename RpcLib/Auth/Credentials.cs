using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace RpcLib.Auth {

    /// <summary>
	/// HTTP Basic Auth credentials. Of course any other mechanism for authentication
    /// can be used within this library, see <see cref="IRpcAuth"/>.
	/// </summary>
	public class Credentials {

        public string Username { get; set; }
        public string Password { get; set; }

        public Credentials(string username, string password) {
            Username = username;
            Password = password;
        }

        public static Credentials? FromBasicAuth(HttpRequest httpRequest) {
            if (httpRequest.Headers.ContainsKey("Authorization")) {
                try {

                    var authHeader = AuthenticationHeaderValue.Parse(httpRequest.Headers["Authorization"]);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                    return new Credentials(credentials[0], credentials[1]);
                }
                catch (Exception) {
                }
            }
            return null;
        }
    }

}
