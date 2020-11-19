namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// An RPC call, i.e. a <see cref="RpcMethod"/> invocation with additional
    /// information who is called and including the current state and settings of the call.
    /// </summary>
    public class RpcCall {

        #region Invocation

        /// <summary>
        /// The called method.
        /// </summary>
        public RpcMethod Method { get; set; } = null!; // Hide compiler warning

        /// <summary>
        /// The ID of the target peer where to run this call on,
        /// i.e. the client ID or null for the server.
        /// </summary>
        public string? TargetPeerID { get; set; }

        #endregion

        #region State

        /// <summary>
        /// The current state of this call.
        /// The RPC engine calls <see cref="SetState"/> and <see cref="Finish"/> to update
        /// the state while it processes the call.
        /// </summary>
        public RpcCallState State { get; set; } = RpcCallState.Created;

        /// <summary>
        /// The return value of a successful call. It is set for non-void methods
        /// as soon as the <see cref="State"/> is <see cref="RpcCallState.Successful"/>,
        /// otherwise it is null.
        /// </summary>
        public byte[]? Result { get; set; } = null;

        #endregion

        #region Settings

        /// <summary>
        /// Strategy used for an automatic retry of this call,
        /// when it has failed because of network problems
        /// (see <see cref="RpcFailureTypeEx.IsRetryable"/>).
        /// </summary>
        public RpcRetryStrategy? RetryStrategy { get; set; } = null;

        /// <summary>
        /// Individual timeout for this call.
        /// By default <see cref="defaultTimeoutMs"/> is used.
        /// </summary>
        public int? TimeoutMs { get; set; } = null;

        /// <summary>
        /// Individual strategy used for serializing messages, e.g.
        /// a method using compression to reduce traffic between the peers.
        /// </summary>
        public string? SerializerID { get; set; } = null; // TODO 

        #endregion

    }

}
