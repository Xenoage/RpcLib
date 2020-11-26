using System.Collections.Generic;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// An RPC call, i.e. a <see cref="RpcMethod"/> invocation with additional
    /// information who is called and settings of the call.
    /// </summary>
    public class RpcCall {

        #region Invocation

        /// <summary>
        /// The called method.
        /// </summary>
        public RpcMethod Method { get; set; } = null!; // Hide compiler warning

        /// <summary>
        /// The ID of the remote peer where to run this call on,
        /// i.e. the client ID or null for the server.
        /// </summary>
        public string? RemotePeerID { get; set; }

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

        #region Execution state

        public long StartTime { get; private set; } = CoreUtils.TimeNowMs();
        public RpcResult? Result { get; set; } = null;

        /// <summary>
        /// Resets the start time to the current time and sets the
        /// result back to null.
        /// </summary>
        public void ResetStartTimeAndResult() {
            StartTime = CoreUtils.TimeNowMs();
            Result = null;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns true, if a retry strategy (not none) is set.
        /// </summary>
        public bool IsRetryable() =>
            RetryStrategy != null && RetryStrategy != RpcRetryStrategy.None;

        // GOON: Needed? DOES NOT INCLUDE RESULT/ENQUEUETIME
        public override bool Equals(object? obj) {
            return obj is RpcCall call &&
                   Method.Equals(call.Method) &&
                   RemotePeerID == call.RemotePeerID &&
                   RetryStrategy == call.RetryStrategy &&
                   TimeoutMs == call.TimeoutMs &&
                   SerializerID == call.SerializerID;
        }

        #endregion

    }

}
