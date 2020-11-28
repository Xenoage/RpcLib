using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;

namespace Chat {

    /// <summary>
    /// RPC methods on the client side.
    /// </summary>
    public interface IChatClientRpc : IRpcMethods {

        /// <summary>
        /// Call this method from the server to send a received message
        /// from a different user with the given username to this user.
        /// </summary>
        Task ReceiveMessage(string message, string username);

    }

}
