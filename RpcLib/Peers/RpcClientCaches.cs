using System.Collections.Generic;

namespace RpcLib.Peers {

    /// <summary>
    /// This class stores the message queues and cached results for each client.
    /// </summary>
    public class RpcClientCaches {

        // For each client (identified by client ID) its current command queue
        private IDictionary<string, RpcPeerCache> queues = new Dictionary<string, RpcPeerCache>();

        /// <summary>
        /// Gets the queue of the given client.
        /// If the client is unknown yet, it is created.
        /// </summary>
        public RpcPeerCache GetClient(string clientID) {
            if (false == queues.TryGetValue(clientID, out var queue))
                queues[clientID] = queue = new RpcPeerCache(clientID);
            return queue;
        }

    }

}
