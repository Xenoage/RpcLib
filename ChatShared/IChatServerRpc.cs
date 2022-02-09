using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Chat {

    /// <summary>
    /// RPC methods on the server side.
    /// </summary>
    public interface IChatServerRpc : IRpcMethods {

        /// <summary>
        /// Clients can register for this event to get notified when the
        /// server has a message available for them.
        /// </summary>
        event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Call this method from the client to send a message
        /// to all users in the chat.
        /// </summary>
        [RpcOptions(RetryStrategy = RpcRetryStrategy.Retry)]
        Task SendPublicMessage(string message);

        /// <summary>
        /// Call this method from the client to send a message
        /// only to the user with the given username.
        /// Returns true, iff the message could be dispatched to the
        /// given user.
        /// </summary>
        [RpcOptions(RetryStrategy = RpcRetryStrategy.Retry)]
        Task<bool> SendPrivateMessage(string message, string username);

    }

}
