using RpcLib.Logging;
using RpcLib.Model;
using RpcLib.Utils;

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
        /// Long polling time in milliseconds. After this time, the server returns null when there is
        /// no command in the queue. Make sure this value is identical on the server and on the
        /// clients, otherwise the client may think there is a network problem if the server
        /// does not respond early enough.
        /// </summary>
        public int LongPollingMs = 90_000;

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

        /// <summary>
        /// Logging interface used for RpcLib log messages.
        /// The interface is kept very simple, so it is easy to adapt your favorite
        /// logging framework to it.
        /// By default, logging is disabled.
        /// </summary>
        public ILogger Logger { get; set; } = new NoLogger();

        /// <summary>
        /// Serialization into JSON and deserialization from JSON.
        /// The interface is kept very simple, so that any JSON library with its
        /// custom settings can easily be adapted to be used within this library.
        /// A default implementation based on Newtonsoft.JSON is provided.
        /// </summary>
        public IJsonLib JsonLib { get; set; } = new JsonLib();

    }


}
