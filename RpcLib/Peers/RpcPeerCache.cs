using RpcLib.Logging;
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
        private const int maxQueueSize = 100;

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
        /// Creates a cache for the given client ID (or "" for the server peer)
        /// and optionally a backlog for the failed commands for retrying.
        /// </summary>
        public RpcPeerCache(string clientID, IRpcCommandBacklog? commandBacklog) {
            ClientID = clientID;
            this.commandBacklog = commandBacklog;
        }

        /// <summary>
        /// The ID of this client peer, or "" for the server peer.
        /// </summary>
        public string ClientID { get; }

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
            RpcCommand? nextCommand = null;
            // Peek the next element from the normal queue, if any
            var queueCommand = queue.Peek();
            // When there is a non-empty command backlog, peek its first item
            // and compare its ID with the command from the normal queue
            if (commandBacklog?.PeekCommand(ClientID) is RpcCommand backlogCommand) { // Just peek, not dequeue. Only dequeue when finished.
                if (queueCommand == null || backlogCommand.ID <= queueCommand.ID) {
                    // Backlog command comes first!
                    // If the command in the backlog is the same as the next command in the normal queue (this can happen, because we
                    // immediately put retryable commands in both the backlog and the normal queue),
                    // prefer the command from the normal queue, because it is able to return a value to the caller.
                    if (queueCommand?.ID == backlogCommand.ID) {
                        nextCommand = await queue.Dequeue(-1); // Before it was only peeked, now dequeue it
                        RpcMain.Log($"Next command dequeued from queue (= same command as in backlog): " +
                            $"{nextCommand?.ID} {nextCommand?.MethodName}", LogLevel.Trace);
                    }
                    else {
                        nextCommand = backlogCommand;
                        RpcMain.Log($"Next command dequeued from backlog: {nextCommand.ID} {nextCommand.MethodName}", LogLevel.Trace);
                    }
                }
            }
            // Otherwise, get or wait for next item in the normal queue
            if (nextCommand == null) {
                nextCommand = await queue.Dequeue(timeoutMs);
                if (nextCommand != null)
                    RpcMain.Log($"Next command dequeued from queue: {nextCommand.ID} {nextCommand.MethodName}", LogLevel.Trace);
                else
                    RpcMain.Log($"Next command: None (timeout)", LogLevel.Trace);
            }
            CurrentCommand = nextCommand;
            return CurrentCommand;
        }

        /// <summary>
        /// Enqueues the given command in the queue.
        /// When the queue would become too long, an <see cref="RpcException"/> of type
        /// <see cref="RpcFailureType.LocalQueueOverflow"/> is thrown.
        /// </summary>
        public void EnqueueCommand(RpcCommand command) {
            // Synchronize enqueuing the commands, because writing in the backlog
            // can require some time, while in the meantime a more recent command
            // may already be enqueued in the normal queue. Then, we would have
            // an out-of-order command.
            lock (this) {
                try {
                    // When it is a command which should be retried in case of network failure, enqueue it in the command backlog.
                    if (command.RetryStrategy != null && command.RetryStrategy != RpcRetryStrategy.None)
                        CommandBacklog.EnqueueCommand(ClientID, command);
                }
                catch (Exception ex) {
                    string errorMessage = $"Could not enqueue command into backlog " +
                            (ClientID != null ? $"on the client { ClientID}" : "on the server: " + ex.Message);
                    RpcMain.Log(errorMessage, LogLevel.Warn);
                    throw new RpcException(new RpcFailure(
                        RpcFailureType.Other, errorMessage));
                }
                try {
                    // Always (additionally to the command backlog) add it to our normal query for immediate execution
                    queue.Enqueue(command);
                    command.SetState(RpcCommandState.Enqueued);
                }
                catch {
                    string errorMessage = $"Queue already full " +
                            (ClientID != null ? $"for the client { ClientID}" : "for the server");
                    RpcMain.Log(errorMessage, LogLevel.Warn);
                    throw new RpcException(new RpcFailure(
                        RpcFailureType.QueueOverflow, errorMessage));
                }
            }
        }

        /// <summary>
        /// Call this method, when the <see cref="CurrentCommand"/> was executed on the remote peer,
        /// whether successful or not. Do not call it, when the command failed because of an network problem.
        /// </summary>
        public void FinishCurrentCommand() {
            if (CurrentCommand?.RetryStrategy != null && CurrentCommand.RetryStrategy != RpcRetryStrategy.None) {
                RpcMain.Log($"Current retryable command {CurrentCommand.ID} finished, dequeuing it from backlog.", LogLevel.Trace);
                commandBacklog?.DequeueCommand(ClientID, CurrentCommand.ID);
            }
            else {
                RpcMain.Log($"Current command {CurrentCommand?.ID} finished", LogLevel.Trace);
            }
        }

        /// <summary>
        /// Gets the cached result of the command with the given ID, if it was executed already,
        /// or null, if it is a new command which has to be executed now.
        /// If the command was already executed, but is too old so that there is no cached result
        /// any more, a failure result with <see cref="RpcFailureType.ObsoleteCommandID"/> is returned.
        /// </summary>
        public RpcCommandResult? GetCachedResult(RpcCommand command) {
            // New command?
            if (command.ID > lastCachedResultCommandID)
                return null;
            // It is an old command. Find the cached result.
            var cachedResults = this.cachedResults.ToList(); // Defensive copy
            var result = cachedResults.Find(it => it.CommandID == command.ID);
            if (result != null)
                return result;

            // GOON! currently we ignore ObsoleteCommandID for retryable commands - fix this immediately
            if (command.RetryStrategy != null && command.RetryStrategy != RpcRetryStrategy.None)
                return null;

            // When there is no result, this is an error in the process
            return RpcCommandResult.FromFailure(command.ID, new RpcFailure(
                RpcFailureType.ObsoleteCommandID, $"Command ID {command.ID} already executed too long ago " +
                    (ClientID != null ? $"on the client {ClientID}" : "for the server")), compression: null);
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
