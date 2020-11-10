using Microsoft.AspNetCore.Http;
using System;

namespace RpcLib.Auth {

    /// <summary>
    /// Client authentication on the server.
    /// Given an <see cref="HttpRequest"/>, a class implementing this interface must
    /// return the ID of the client identified by this request.
    /// </summary>
    [Obsolete("Use SignalR based mechanism instead")]
    public interface IRpcAuth {

        /// <summary>
        /// Tries to authenticate the client by the given HTTP request.
        /// The returned object contains the requested client ID and if the
        /// authentication was successful or not.
        /// </summary>
        AuthResult Authenticate(HttpRequest request);

    }

}
