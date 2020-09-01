using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RpcLib.Model {

    /// <summary>
    /// If and how to compress commands and responses
    /// to reduce traffic between the peers.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcCompressionStrategy {

        /// <summary>
        /// Compress the messages, if their size (in JSON-encoded format)
        /// is equal or greater than <see cref="RpcSettings.CompressionThresholdBytes"/>.
        /// </summary>
        Auto,

        /// <summary>
        /// Always use gzip compression. For small messages, this may even
        /// increase the size of the packets, so be careful to use it only
        /// when you are sure of what you are doing.
        /// </summary>
        Enabled,

        /// <summary>
        /// Never use gzip compression. Messages will always be transferred
        /// in plaintext JSON format.
        /// </summary>
        Disabled

    }

}
