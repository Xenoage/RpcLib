using System.Net;

namespace Xenoage.RpcLib.Auth {

    /// <summary>
    /// Client authentication on the server. Based on a given HTTP request,
    /// the implementing class must identify the user or return an authentication failure.
    /// This can be done by using a Basic Auth header for example, as demonstrated
    /// in <see cref="RpcServerBasicAuth"/>.
    /// </summary>
    public interface IRpcServerAuth {

        /// <summary>
        /// Tries to authenticate the client by the given HTTP request.
        /// </summary>
        AuthResult Authenticate(HttpListenerRequest request);

    }

}
