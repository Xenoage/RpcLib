using System.Net.WebSockets;

namespace Xenoage.RpcLib.Auth {

    /// <summary>
    /// Client authentication set on the client side. A <see cref="ClientWebSocket"/>
    /// is given before the connection is established. This socket can be modified
    /// in any way to include the authentication information.
    /// This can be done by using a Basic Auth header for example, as demonstrated
    /// in <see cref="RpcClientBasicAuth"/>.
    /// </summary>
    public interface IRpcClientAuth {

        /// <summary>
        /// Sets the authentication information to the given websocket.
        /// </summary>
        void Authenticate(ClientWebSocket socket);

    }

}
