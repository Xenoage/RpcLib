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
        /// Returns the first command from the queue of the given client (or null for the server),
        /// or null when the queue is empty.
        /// The command is not removed from the queue by calling this command.
        /// Call <see cref="FinishCommand"/> when it has been finished.
        /// </summary>
        RpcCommand? GetCommand(string? clientID);

        /// <summary>
        /// Removes the first command from the queue of the given client (or null for the server).
        /// </summary>
        bool FinishCommand(string? clientID);

        /// <summary>
        /// Adds the given command to the queue of the given client (or null for the server).
        /// When the given strategy allows only a single command of this type
        /// (<see cref="RpcRetryStrategy.RetryNewestWhenOnline"/>), all commands with the
        /// same command name are removed from the queue.
        /// </summary>
        void Enqueue(string? clientID, RpcCommand command, RpcRetryStrategy retryStrategy);

    }

}
