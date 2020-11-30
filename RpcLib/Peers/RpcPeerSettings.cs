using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Queue;
using Xenoage.RpcLib.Serialization;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Settings for a local <see cref="RpcServer"/> or <see cref="RpcClient"/>.
    /// </summary>
    public class RpcPeerSettings {

        /// <summary>
        /// Default options for local method execution (calls of the remote peer
        /// which are invocated on this local side).
        /// </summary>
        public RpcOptions DefaultOptions { get; set; } = new RpcOptions();

        /// <summary>
        /// Persistent storage for retryable calls, or null, when
        /// failed retryable calls should not be retried after program restarts.
        /// </summary>
        public IRpcBacklog? Backlog { get; set; } = null;

        /// <summary>
        /// After the connection gets lost, automatically tries to reestablish the
        /// connection after this amount of time in milliseconds. Only used on the
        /// client side, since the server can not open a connection to a client.
        /// </summary>
        public int ReconnectTimeMs { get; set; } = 30_000;

        /// <summary>
        /// Custom (de)serializer for the method call payloads.
        /// By default, the included <see cref="JsonSerializer"/> is used.
        /// </summary>
        public ISerializer Serializer { get; set; } = new JsonSerializer();

        /// <summary>
        /// Custom logger for library internal logging.
        /// By default, the included <see cref="ConsoleLogger"/>
        /// with <see cref="LogLevel.Info"/> is used.
        /// </summary>
        public ILogger Logger { get; set; } = new ConsoleLogger(LogLevel.Info);

    }

}
