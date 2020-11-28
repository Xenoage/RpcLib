using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;

namespace Chat {

    /// <summary>
    /// RPC methods on the server side.
    /// </summary>
    public interface IChatServerRpc : IRpcMethods {

        /// <summary>
        /// Call this method from the client to send a message
        /// to all users in the chat.
        /// </summary>
        Task SendPublicMessage(string message);

        /// <summary>
        /// Call this method from the client to send a message
        /// only to the user with the given username.
        /// Returns true, iff the message could be dispatched to the
        /// given user.
        /// </summary>
        Task<bool> SendPrivateMessage(string message, string username);

    }

}
