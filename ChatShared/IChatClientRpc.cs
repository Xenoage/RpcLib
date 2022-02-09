using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;

namespace Chat {

    /// <summary>
    /// RPC methods on the client side.
    /// </summary>
    public interface IChatClientRpc : IRpcMethods {

        /// <summary>
        /// Call this method from the server to send the given message to this user.
        /// As an alternative, see the <see cref="IChatServerRpc.MessageReceived"/> event.
        /// </summary>
        Task ReceiveMessage(ChatMessage message);

    }

}
