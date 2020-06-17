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

        // The next commands to execute on the peer, at maximum 10 items.
        private BlockingQueue<RpcCommand> queue = new BlockingQueue<RpcCommand>(size: maxQueueSize);

        // The cached results of the last peer's calls, at maximum 10 items.
        // If the same command is received again from the peer (because the response get lost), it can be
        // answered without executing the command again.
        private ConcurrentQueue<RpcCommandResult> cachedResults = new ConcurrentQueue<RpcCommandResult>();

        // The ID of the last command, which result was cached, or 0. Since the IDs are ascending,
        // already executed commands can be easily determined.
        private ulong lastCachedResultCommandID = 0;

        /// <summary>
        /// Creates a cache for the given client ID (or null for the server peer).
        /// </summary>
        public RpcPeerCache(string? clientID = null) {
            ClientID = clientID;
        }

        /// <summary>
        /// The ID of this client peer, or null for the server peer.
        /// </summary>
        public string? ClientID { get; }

        /// <summary>
        /// Gets the current command in the queue, or null, if there is none.
        /// The method returns as soon as there is a value, or with null when the
        /// given timeout in milliseconds is hit.
        /// </summary>
        public async Task<RpcCommand?> GetCurrentCommand(int timeoutMs) {
            return await queue.Peek(timeoutMs);
        }

        /// <summary>
        /// Finishes the current command in the queue, i.e. removes it from the queue.
        /// </summary>
        public async Task FinishCurrentCommand() {
            await queue.Dequeue(0); // Should be removed immediately, because we called GetCurrentCommand before
        }

        /// <summary>
        /// Enqueues the given command in the queue.
        /// When the queue would become too long, an <see cref="RpcException"/> of type
        /// <see cref="RpcFailureType.LocalQueueOverflow"/> is thrown.
        /// </summary>
        public void EnqueueCommand(RpcCommand command) {
            try {
                queue.Enqueue(command);
                command.SetState(RpcCommandState.Enqueued);
            }
            catch {
                throw new RpcException(new RpcFailure(
                    RpcFailureType.LocalQueueOverflow, $"Queue already full " +
                        (ClientID != null ? "for the client { ClientID}" : "for the server")));
            }
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

    }

}
