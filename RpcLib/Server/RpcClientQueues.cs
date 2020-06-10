using RpcLib.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RpcLib.Server {

    /// <summary>
    /// This class stores the message queues and cached results for each client.
    /// </summary>
    public class RpcClientQueues {

        // For each client (identified by client ID) its current command queue
        private IDictionary<string, RpcClientQueue> queues = new Dictionary<string, RpcClientQueue>();


        /// <summary>
        /// Gets the queue of the given client.
        /// If the client is unknown yet, it is created.
        /// </summary>
        public RpcClientQueue GetClient(string clientID) {
            if (false == queues.TryGetValue(clientID, out var queue))
                queues[clientID] = queue = new RpcClientQueue(clientID);
            return queue;
        }

    }

}
