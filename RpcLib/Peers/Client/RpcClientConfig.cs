namespace RpcLib.Peers.Client {

    /// <summary>
    /// Settings of an RPC client.
    /// </summary>
    public class RpcClientConfig {

        /// <summary>
        /// The ID of this client. Each client in the RPC network must have a unique ID.
        /// </summary>
        public string ClientID { get; set; } = "Unknown";

        /// <summary>
        /// Base URL of the RPC server, including the protocol (http or https),
        /// and the path of the RPC API endpoints. By default "http://localhost:5000/rpc".
        /// </summary>
        public string ServerUrl { get; set; } = "http://localhost:5000/rpc";

    }

}
