using RpcLib.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RpcLib.Server {

    /// <summary>
    /// This server-side class, which is instantiated for each client, stores
    /// the message queue for each client and caches the results of the last
    /// executed commands.
    /// </summary>
    public class RpcClientQueue {

        // The maximum number of open commands in the queue for each client
        private int maxQueueSize = 10;

        // The maximum number of cached command results for each client
        private int maxResultCacheSize = 10;

        // The client's current command queue
        private ConcurrentQueue<RpcCommand> queue = new ConcurrentQueue<RpcCommand>();

        // The cached results of the last client's calls
        private ConcurrentQueue<RpcCommandResult> cachedResults = new ConcurrentQueue<RpcCommandResult>();

        // The ID of the last executed command, or 0
        private ulong lastExecutedCommandID = 0;

        public RpcClientQueue(string clientID) {
            ClientID = clientID;
        }

        /// <summary>
        /// The ID of this client.
        /// </summary>
        public string ClientID { get; }

        /// <summary>
        /// Gets the current command in the queue of the this client, or null,
        /// if there is none.
        /// </summary>
        public RpcCommand? GetCurrentCommand() {
            if (queue.TryPeek(out var command))
                return command;
            return null;
        }

        /// <summary>
        /// Finishes the current command in the queue of this client,
        /// i.e. removes it from the queue.
        /// </summary>
        public void FinishCurrentCommand() {
            queue.TryDequeue(out _);
        }

        /// <summary>
        /// Enqueues the given command in the queue of this client.
        /// When the queue would become too long, an <see cref="RpcException"/> of type
        /// <see cref="RpcFailureType.LocalQueueOverflow"/> is thrown.
        /// </summary>
        public void EnqueueCommand(RpcCommand command) {
            if (queue.Count + 1 > maxQueueSize)
                throw new RpcException(new RpcFailure(
                    RpcFailureType.LocalQueueOverflow, $"Queue for client {ClientID} already full"));
            queue.Enqueue(command);
            command.SetState(RpcCommandState.Enqueued);
        }

        /// <summary>
        /// Gets the cached result of the command with the given ID, if it was executed already,
        /// or null, if it is a new command which has to be executed now.
        /// If the command was already executed, but is too old so that there is no cached result
        /// any more, an <see cref="RpcException"/> of type <see cref="RpcFailureType.ObsoleteCommandID"/> is thrown.
        /// </summary>
        public RpcCommandResult? GetCachedResult(ulong commandID) {
            // New command?
            if (commandID > lastExecutedCommandID)
                return null;
            // It is an old command. Find the cached result.
            var cachedResults = this.cachedResults.ToList(); // Defensive copy
            var result = cachedResults.Find(it => it.ID == commandID);
            return result ?? throw new RpcException(new RpcFailure(
                RpcFailureType.ObsoleteCommandID, $"Command ID {commandID} already executed too long ago on client {ClientID}"));
        }

        /// <summary>
        /// Caches the given command result, so that repeated calls of the same
        /// command ID can be answered without executing the command again.
        /// </summary>
        public void CacheResult(RpcCommandResult result) {
            cachedResults.Enqueue(result);
            while (cachedResults.Count > maxResultCacheSize)
                cachedResults.TryDequeue(out _);
        }

    }

}
