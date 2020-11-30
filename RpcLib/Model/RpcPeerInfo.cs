using System;

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


        public static RpcPeerInfo Client(string clientID, string ip) =>
            new RpcPeerInfo(peerID: clientID, ip);

        public static RpcPeerInfo Server(string serverUrl) =>
            new RpcPeerInfo(peerID: null, new Uri(serverUrl).Host);

        public RpcPeerInfo(string? peerID, string ip) {
            PeerID = peerID;
            IP = ip;
        }

        public override string ToString() => PeerID == null
            ? $"server at {IP}"
            : $"client {PeerID} at {IP}";
    }

}
