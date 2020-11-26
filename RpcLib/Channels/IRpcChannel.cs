using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Channels {

    /// <summary>
    /// This interface allows the RPC library to use different communication channels
    /// between the peers. The usual one is the <see cref="WebSocketRpcChannel"/>,
    /// but other ones can be used e.g. for testing.
    /// 
    /// Implementations need not to be thread-safe, because <see cref="Send"/>
    /// and <see cref="Receive"/> will not be executed more than once at a time.
    /// </summary>
    public interface IRpcChannel {

        /// <summary>
        /// Returns true, iff the connection is still open for receiving and
        /// sending messages.
        /// </summary>
        public bool IsOpen();

        /// <summary>
        /// Sends the given message to the remote peer. The given cancellation token
        /// can be used to cancel this process.
        /// </summary>
        public Task Send(RpcMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// Awaits, receives and returns a message from the remote peer.
        /// Null is returned, when the connection was closed before.
        /// </summary>
        public Task<RpcMessage?> Receive(CancellationToken cancellationToken);

        /// <summary>
        /// Requests to close this connection. A notification should be sent
        /// to the other peer, so that it knows that the channel is closed now.
        /// The returned task does not wait for any reaction of the remote peer.
        /// </summary>
        public Task Close();

    }

}
