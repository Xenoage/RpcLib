using RpcLib.Model;
using Shared.Rpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RpcServer.Rpc {

    /// <summary>
    /// This class stores the message queues for each client.
    /// </summary>
    public class RpcClientQueues {

        // The maximum number of open commands in each queue
        private int maxQueueSize = 10;

        // For each client (identified by client ID) its current command queue
        private IDictionary<string, ConcurrentQueue<RpcCommand>> queues = new Dictionary<string, ConcurrentQueue<RpcCommand>>();

        /// <summary>
        /// Gets the current command in the queue of the given client, or null,
        /// if there is none.
        /// </summary>
        public RpcCommand? GetCurrentCommand(string clientID) {
            if (queues.TryGetValue(clientID, out var queue))
                if (queue.TryPeek(out var command))
                    return command;
            return null;
        }

        /// <summary>
        /// Finishes the current command in the queue of the given client,
        /// i.e. removes it from the queue.
        /// </summary>
        public void FinishCurrentCommand(string clientID) {
            if (queues.TryGetValue(clientID, out var queue))
                queue.TryDequeue(out _);
        }

        /// <summary>
        /// Enqueues the given command in the queue of the given client.
        /// If the client is unknown yet, it is created.
        /// When the queue would become too long, an <see cref="RpcException"/> of type
        /// <see cref="RpcFailureType.LocalQueueOverflow"/> is thrown.
        /// </summary>
        public void EnqueueCommand(string clientID, RpcCommand command) {
            if (false == queues.TryGetValue(clientID, out var queue))
                queues[clientID] = queue = new ConcurrentQueue<RpcCommand>();
            if (queue.Count + 1 > maxQueueSize)
                throw new RpcException(new RpcFailure(RpcFailureType.LocalQueueOverflow, $"Queue for client {clientID} already full"));
            command.State = RpcCommandState.Enqueued;
            queue.Enqueue(command);
        }

    }

}
