namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// An RPC command call, i.e. a <see cref="RpcCommand"/> with additional information
    /// who is called and including the current state of the call.
    /// </summary>
    public class RpcCall {

        /// <summary>
        /// The called command.
        /// </summary>
        public RpcCommand Command { get; set; } = null!; // Hide compiler warning

        /// <summary>
        /// The ID of the target peer where to run this command on,
        /// i.e. the client ID or null for the server.
        /// </summary>
        public string? TargetPeerID { get; set; }

        /// <summary>
        /// Strategy used for an automatic retry of this command,
        /// when it has failed because of network problems
        /// (see <see cref="RpcFailureTypeEx.IsRetryable"/>).
        /// </summary>
        public RpcRetryStrategy? RetryStrategy { get; set; } = null;

        /// <summary>
        /// Individual timeout for this command.
        /// By default <see cref="defaultTimeoutMs"/> is used.
        /// </summary>
        public int? TimeoutMs { get; set; } = null;

        /// <summary>
        /// Individual strategy used for serializing messages, e.g.
        /// a method using compression to reduce traffic between the peers.
        /// </summary>
        public string? SerializerID { get; set; } = null; // TODO 

        /// <summary>
        /// The current state of this command.
        /// The RPC engine calls <see cref="SetState"/> and <see cref="Finish"/> to update
        /// the state while it processes the command.
        /// </summary>
        public RpcCallState State { get; set; } = RpcCallState.Created;

        /// <summary>
        /// The return value of a successful call. It is set for non-void methods
        /// as soon as the <see cref="State"/> is <see cref="RpcCallState.Successful"/>,
        /// otherwise it is null.
        /// </summary>
        public byte[]? Result { get; set; } = null;

    }

}
