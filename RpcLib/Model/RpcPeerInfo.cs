using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Information on a remote peer.
    /// </summary>
    public class RpcPeerInfo {

        /// <summary>
        /// ID of the remote client, or null for the remote server.
        /// </summary>
        public string? PeerID { get; }

        /// <summary>
        /// IP address of the remote client,
        /// or domain of the remote server.
        /// </summary>
        public string IP { get; }

        /// <summary>
        /// List of events the remote peer has registered on the local peer.
        /// </summary>
        public ISet<string> RegisteredEventNames { get; private set; } = ImmutableHashSet<string>.Empty;


        public static RpcPeerInfo Client(string clientID, string ip) =>
            new RpcPeerInfo(peerID: clientID, ip);

        public static RpcPeerInfo Server(string serverUrl) =>
            new RpcPeerInfo(peerID: null, new Uri(serverUrl).Host);

        public RpcPeerInfo(string? peerID, string ip) {
            PeerID = peerID;
            IP = ip;
        }

        public void SetRegisteredEventNames(IEnumerable<string> eventNames) {
            RegisteredEventNames = new HashSet<string>(eventNames);
        }

        public bool HasRegisteredEvent(string eventName) =>
            RegisteredEventNames.Contains(eventName);

        public override string ToString() => PeerID == null
            ? $"server at {IP}"
            : $"client {PeerID} at {IP}";
    }

}
