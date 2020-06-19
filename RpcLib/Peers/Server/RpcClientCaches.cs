using RpcLib.Server;
using System.Collections.Generic;
using System.Linq;

namespace RpcLib.Peers.Server {

    /// <summary>
    /// This server-side class stores the message queues and cached results for each client.
    /// </summary>
    public class RpcClientCaches {

        // For each client (identified by client ID) its current command queue
        private IDictionary<string, RpcPeerCache> queues = new Dictionary<string, RpcPeerCache>();

        /// <summary>
        /// Gets the list of the IDs of all clients which are or were connected to the server.
        /// </summary>
        public List<string> GetClientIDs() =>
            queues.Keys.ToList();

        /// <summary>
        /// Gets the queue of the given client.
        /// If the client is unknown yet, it is created with the given backlog.
        /// </summary>
        public RpcPeerCache GetClient(string clientID, IRpcCommandBacklog? commandBacklog) {
            if (false == queues.TryGetValue(clientID, out var queue))
                queues[clientID] = queue = new RpcPeerCache(clientID, commandBacklog);
            return queue;
        }

    }

}
