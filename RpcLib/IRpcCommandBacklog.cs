using RpcLib.Model;

namespace RpcLib {

    /// <summary>
    /// Interface for saving commands that should be retried as soon as the other
    /// peer is reachable again. See <see cref="RpcRetryStrategy"/>.
    /// When the implementation of this interface is using persistent storage (file, database, ...),
    /// the retry strategy still works after reboot.
    /// </summary>
    public interface IRpcCommandBacklog {

        /// <summary>
        /// Dequeues and returns the first command from the queue of the given client
        /// (or null for the server), or null when the queue is empty.
        /// </summary>
        RpcCommand? DequeueCommand(string? clientID);

        /// <summary>
        /// Adds the given command to the queue of the given client (or null for the server).
        /// When the retry strategy of the given command allows only a single command of this type
        /// (<see cref="RpcRetryStrategy.RetryNewestWhenOnline"/>), all commands with the
        /// same command name are removed from the queue.
        /// </summary>
        void EnqueueCommand(string? clientID, RpcCommand command);

    }

}
