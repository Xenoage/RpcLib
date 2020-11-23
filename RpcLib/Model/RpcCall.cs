using System.Collections.Generic;

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
        /// The result of a finished call, whether successful or not.
        /// Null, when the response for this call has not yet been received.
        /// </summary>
        public RpcResult? Result { get; set; } = null;

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

        /// <summary>
        /// Returns true, if a retry strategy (not none) is set.
        /// </summary>
        public bool IsRetryable =>
            RetryStrategy != null && RetryStrategy != RpcRetryStrategy.None;

        #endregion

        #region Comparison

        public override bool Equals(object? obj) {
            return obj is RpcCall call &&
                   Method.Equals(call.Method) &&
                   TargetPeerID == call.TargetPeerID &&
                   EqualityComparer<RpcResult?>.Default.Equals(Result, call.Result) &&
                   RetryStrategy == call.RetryStrategy &&
                   TimeoutMs == call.TimeoutMs &&
                   SerializerID == call.SerializerID &&
                   IsRetryable == call.IsRetryable;
        }

        #endregion

    }

}
