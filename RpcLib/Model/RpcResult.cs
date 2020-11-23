using System.Collections.Generic;

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


        public override bool Equals(object? obj) {
            return obj is RpcResult result &&
                   MethodID == result.MethodID &&
                   EqualityComparer<RpcFailure?>.Default.Equals(Failure, result.Failure) &&
                   EqualityComparer<byte[]?>.Default.Equals(ReturnValue, result.ReturnValue);
        }

    }


}
