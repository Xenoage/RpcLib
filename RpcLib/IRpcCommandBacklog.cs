using RpcLib.Model;

namespace RpcLib {

    /// <summary>
    /// Interface for saving commands that should be retried until they were
    /// executed on the remote peer. See <see cref="RpcRetryStrategy"/>.
    /// When the implementation of this interface is using persistent storage (file, database, ...),
    /// the retry strategy still works after reboot.
    /// </summary>
    public interface IRpcCommandBacklog {

        /// <summary>
        /// Returns (but does not dequeue) the first command from the queue of the given target peer
        /// (client ID or "" for the server), or null when the queue is empty.
        /// </summary>
        RpcCommand? PeekCommand(string targetPeerID);

        /// <summary>
        /// Dequeues the first command from the queue of the given target peer
        /// (client ID or "" for the server), but only if the given command ID matches to this item.
        /// </summary>
        void DequeueCommand(string targetPeerID, ulong commandID);

        /// <summary>
        /// Adds the given command to the queue of the given target peer (client ID or "" for the server).
        /// When the retry strategy of the given command allows only a single command of this type
        /// (<see cref="RpcRetryStrategy.RetryNewestWhenOnline"/>), all commands with the
        /// same command name are removed from the queue.
        /// </summary>
        void EnqueueCommand(string targetPeerID, RpcCommand command);

    }

}
