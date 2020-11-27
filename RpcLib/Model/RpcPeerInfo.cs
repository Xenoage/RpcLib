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


        public static RpcPeerInfo Server(string serverUrl) =>
            new RpcPeerInfo(peerID: null, new Uri(serverUrl).Host);

        public RpcPeerInfo(string? peerID, string iP) {
            PeerID = peerID;
            IP = iP;
        }

        public override string ToString() => PeerID == null
            ? $"server at {IP}"
            : $"client {PeerID} at {IP}";
    }

}
