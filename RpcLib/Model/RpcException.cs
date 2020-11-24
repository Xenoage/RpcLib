using System;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Exception wrapper for <see cref="RpcFailure"/>.
    /// </summary>
    public class RpcException : Exception {

        public RpcFailure Failure { get; }

        public RpcException(RpcFailure failure) {
            Failure = failure;
        }

        public RpcFailureType Type =>
            Failure.Type;

        public override string Message =>
            Failure.Message ?? "";

        /// <summary>
        /// See <see cref="RpcFailureTypeEx.IsRetryable"/>.
        /// </summary>
        public bool IsRetryable() =>
            Failure.Type.IsRetryable();


    }

}
