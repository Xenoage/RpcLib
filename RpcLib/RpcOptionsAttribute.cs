using RpcLib.Model;
using System;

namespace RpcLib {
    
    /// <summary>
    /// RPC functions (defined in your <see cref="IRpcFunctions"/> interfaces) can be annotated
    /// with this attribute to set custom features for the method calls, like timeout and retry strategy.
    /// </summary>
    public class RpcOptionsAttribute : Attribute {

        public const int useDefaultTimeout = -2;

        /// <summary>
        /// Individual timeout for this command.
        /// By default (value -2, since null not allowed in C# attribute),
        /// <see cref="defaultTimeoutMs"/> is used.
        /// -1 means no timeout (unlimited time).
        /// </summary>
        public int TimeoutMs { get; set; } = useDefaultTimeout;

        /// <summary>
        /// Use an <see cref="RpcRetryStrategy"/> enum value.
        /// Strategy used for automatic retrying of this command,
        /// when it has failed because of network problems.
        /// </summary>
        public object? RetryStrategy { get; set; } = null;

        /// <summary>
        /// Use an <see cref="RpcCompressionStrategy"/> enum value.
        /// Strategy used for compress messages to reduce traffic between the peers.
        /// </summary>
        public object? Compression { get; set; } = null;

    }

}
