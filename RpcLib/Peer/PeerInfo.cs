namespace Xenoage.RpcLib.Peer {

    /// <summary>
    /// Information on a remote peer.
    /// </summary>
    public class PeerInfo {

        /// <summary>
        /// ID of the remote client, or null for the remote server.
        /// </summary>
        public string? PeerID { get; }

        /// <summary>
        /// IP address of the remote peer.
        /// </summary>
        public string IP { get; }


        public PeerInfo(string? peerID, string iP) {
            PeerID = peerID;
            IP = iP;
        }

        public override string ToString() => PeerID == null
            ? $"server at {IP}"
            : $"client {PeerID} at {IP}";
    }

}
