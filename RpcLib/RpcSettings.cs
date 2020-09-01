using RpcLib.Model;

namespace RpcLib {

    /// <summary>
    /// Settings for this library, including reasonable default values.
    /// </summary>
    public class RpcSettings {

        /// <summary>
        /// Maximum time in milliseconds a sent command may take to be executed and acknowledged. This
        /// includes the time where it is still in the queue.
        /// 30 seconds by default.
        /// </summary>
        public int TimeoutMs { get; set; } = 30_000;

        /// <summary>
        /// If and how to compress commands and responses to reduce traffic between the peers.
        /// <see cref="RpcCompressionStrategy.Disabled"/> by default.
        /// </summary>
        public RpcCompressionStrategy Compression { get; set; } = RpcCompressionStrategy.Disabled;

        /// <summary>
        /// Size in number of bytes of a message (JSON-encoded command or response),
        /// from when onit will be compressed. Applies only to the <see cref="Compression"/>
        /// strategy <see cref="RpcCompressionStrategy.Auto"/>. 
        /// </summary>
        public int CompressionThresholdBytes { get; set; } = 1000;

    }


}
