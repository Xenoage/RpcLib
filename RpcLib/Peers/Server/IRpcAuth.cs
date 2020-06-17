using Microsoft.AspNetCore.Http;

namespace RpcLib.Server.Server {

    /// <summary>
    /// Client authentication on the server.
    /// Given an <see cref="HttpRequest"/>, a class implementing this interface must
    /// return the ID of the client identified by this request.
    /// </summary>
    public interface IRpcAuth {

        /// <summary>
        /// Returns the ID of the client authenticated by this request, or null,
        /// if the client could not be authenticated.
        /// </summary>
        string? GetClientID(HttpRequest request);

    }

}
