using System;
using Xenoage.RpcLib.Methods;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// RPC methods (defined in your <see cref="IRpcMethods"/> interfaces) can be annotated
    /// with this attribute to set custom options for the method calls, like timeout and retry strategy.
    /// </summary>
    public class RpcOptionsAttribute : Attribute {

        /// <summary>
        /// Flag indicating to use no timeout.
        /// </summary>
        public const int useInfiniteTimeout = -1;

        /// <summary>
        /// Flag indicating to use the default timeout instead
        /// (value -2, since null not allowed in C# attribute).
        /// </summary>
        public const int useDefaultTimeout = -2;

        /// <summary>
        /// Individual timeout for this method call in milliseconds.
        /// By default <see cref="useDefaultTimeout"/>.
        /// Also <see cref="useInfiniteTimeout"/> may be used.
        /// </summary>
        public int TimeoutMs { get; set; } = useDefaultTimeout;

        /// <summary>
        /// Use an <see cref="RpcRetryStrategy"/> enum value.
        /// Strategy used for automatic retrying of this command,
        /// when it has failed because of network problems.
        /// By default null.
        /// </summary>
        public object? RetryStrategy { get; set; } = null; // Must be an object for C# attribute

    }

}
