using System.Collections.Generic;
using System.Linq;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Response to an <see cref="RpcMethod"/> call.
    /// </summary>
    public class RpcResult {

        /// <summary>
        /// <see cref="RpcMethod.ID"/> this response belongs to.
        /// </summary>
        public ulong MethodID { get; set; }

        /// <summary>
        /// Reason why this call failed, otherwise null.
        /// </summary>
        public RpcFailure? Failure { get; set; } = null;

        /// <summary>
        /// Serialized response data, when this call was successfull
        /// and has no void return type, otherwise null.
        /// </summary>
        public byte[]? ReturnValue { get; set; } = null;

        /// <summary>
        /// Returns true, iff this result contains a failure which indicates
        /// that it should be retried (only if the method should be retried, of course).
        /// </summary>
        public bool IsRetryNeeded() =>
            Failure != null && Failure.Type.IsRetryable();


        public static RpcResult Timeout(ulong methodID) => new RpcResult {
            MethodID = methodID,
            Failure = new RpcFailure {
                Type = RpcFailureType.Timeout
            }
        };

        public override bool Equals(object? obj) {
            return obj is RpcResult result &&
                MethodID == result.MethodID &&
                EqualityComparer<RpcFailure?>.Default.Equals(Failure, result.Failure) &&
                ((ReturnValue == null && result.ReturnValue == null) ||
                    (ReturnValue != null && result.ReturnValue != null && ReturnValue.SequenceEqual(result.ReturnValue)));
        }

    }


}
