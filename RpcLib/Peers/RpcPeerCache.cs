using RpcLib.Model;
using RpcLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace RpcLib.Server {

    /// <summary>
    /// This class stores the commands queue and the results of the last
    /// already executed commands of a specific RPC peer.
    /// </summary>
    public class RpcPeerCache {

        // The maximum number of items in the queues
        private const int maxQueueSize = 10;

        // The next commands to execute on the peer
        private BlockingQueue<RpcCommand> queue = new BlockingQueue<RpcCommand>(size: maxQueueSize);

        // Backlog of failed commands for retrying
        private IRpcCommandBacklog? commandBacklog;

        // The cached results of the last peer's calls, at maximum 10 items.
        // If the same command is received again from the peer (because the response get lost), it can be
        // answered without executing the command again.
        private ConcurrentQueue<RpcCommandResult> cachedResults = new ConcurrentQueue<RpcCommandResult>();

        // The ID of the last command, which result was cached, or 0. Since the IDs are ascending,
        // already executed commands can be easily determined.
        private ulong lastCachedResultCommandID = 0;

        /// <summary>
        /// Creates a cache for the given client ID (or null for the server peer)
        /// and optionally a backlog for the failed commands for retrying.
        /// </summary>
        public RpcPeerCache(string? clientID, IRpcCommandBacklog? commandBacklog) {
            ClientID = clientID;
            this.commandBacklog = commandBacklog;
        }

        /// <summary>
        /// The ID of this client peer, or null for the server peer.
        /// </summary>
        public string? ClientID { get; }

        /// <summary>
        /// The currently executing command. This property is updated whenever
        /// <see cref="DequeueCommand"/> returns a new result.
        /// </summary>
        public RpcCommand? CurrentCommand { get; private set; } = null;

        /// <summary>
        /// Dequeues and returns the current command in the backlog or queue, or null, if there is none.
        /// The method returns as soon as there is a value, or with null when the
        /// given timeout in milliseconds is hit. A value of -1 means no timeout.
        /// </summary>
        public async Task<RpcCommand?> DequeueCommand(int timeoutMs) {
            // When there is a non-empty command backlog, dequeue and return its first item
            if (commandBacklog?.PeekCommand(ClientID) is RpcCommand backlogCommand) // Just peek, not dequeue. Only dequeue when finished.
                CurrentCommand = backlogCommand;
            // Otherwise, wait for next item in the normal queue
            else
                CurrentCommand = await queue.Dequeue(timeoutMs);
            return CurrentCommand;
        }

        /// <summary>
        /// Enqueues the given command in the queue.
        /// When the queue would become too long, an <see cref="RpcException"/> of type
        /// <see cref="RpcFailureType.LocalQueueOverflow"/> is thrown.
        /// </summary>
        public void EnqueueCommand(RpcCommand command) {
            try {
                // When it is a command which should be retried in case of network failure, enqueue it in the command backlog.
                if (command.RetryStrategy != null && command.RetryStrategy != RpcRetryStrategy.None)
                    CommandBacklog.EnqueueCommand(ClientID, command);
                // Always (additionally to the command backlog) add it to our normal query for immediate execution
                queue.Enqueue(command);
                command.SetState(RpcCommandState.Enqueued);
            }
            catch {
                throw new RpcException(new RpcFailure(
                    RpcFailureType.QueueOverflow, $"Queue already full " +
                        (ClientID != null ? $"for the client { ClientID}" : "for the server")));
            }
        }

        /// <summary>
        /// Call this method, when the <see cref="CurrentCommand"/> was executed on the remote peer,
        /// whether successful or not. Do not call it, when the command failed because of an network problem.
        /// </summary>
        public void FinishCurrentCommand() {
            commandBacklog?.DequeueCommand(ClientID, CurrentCommand.ID);
        }

        /// <summary>
        /// Gets the cached result of the command with the given ID, if it was executed already,
        /// or null, if it is a new command which has to be executed now.
        /// If the command was already executed, but is too old so that there is no cached result
        /// any more, a failure result with <see cref="RpcFailureType.ObsoleteCommandID"/> is returned.
        /// </summary>
        public RpcCommandResult? GetCachedResult(ulong commandID) {
            // New command?
            if (commandID > lastCachedResultCommandID)
                return null;
            // It is an old command. Find the cached result.
            var cachedResults = this.cachedResults.ToList(); // Defensive copy
            var result = cachedResults.Find(it => it.CommandID == commandID);
            return result ?? RpcCommandResult.FromFailure(commandID, new RpcFailure(
                RpcFailureType.ObsoleteCommandID, $"Command ID {commandID} already executed too long ago " +
                    (ClientID != null ? $"on the client {ClientID}" : "for the server")));
        }

        /// <summary>
        /// Caches the given command result, so that repeated calls of the same
        /// command ID can be answered without executing the command again.
        /// </summary>
        public void CacheResult(RpcCommandResult result) {
            lastCachedResultCommandID = Math.Max(lastCachedResultCommandID, result.CommandID);
            cachedResults.Enqueue(result);
            while (cachedResults.Count > maxQueueSize)
                cachedResults.TryDequeue(out _);
        }

        /// <summary>
        /// Gets the registered backlog for retrying failed commands, or throws an exception if there is none.
        /// </summary>
        private IRpcCommandBacklog CommandBacklog =>
            commandBacklog ?? throw new Exception(nameof(IRpcCommandBacklog) + " required, but none was provided");

    }

}
