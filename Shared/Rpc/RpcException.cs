using System;

namespace Shared.Rpc {

    /// <summary>
    /// Exception wrapper for <see cref="RpcFailure"/>.
    /// This is needed, because .NET exceptions can not be serialized to JSON.
    /// </summary>
    public class RpcException : Exception {

        public RpcFailure Failure { get; }

        public RpcException(RpcFailure failure) {
            Failure = failure;
        }

        public RpcFailureType Type =>
            Failure.Type;

        public override string Message =>
            Failure.Message;

    }
}
