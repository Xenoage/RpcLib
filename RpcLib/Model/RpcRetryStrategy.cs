using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RpcLib.Model {

    /// <summary>
    /// If and how to retry commands which could not be finished because of an
    /// <see cref="RpcException.IsRpcProblem"/> failure.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcRetryStrategy {

        /// <summary>
        /// When the timeout is hit, the RPC command returns a timeout failure
        /// and is not tried automatically again. This is the default behavior.
        /// </summary>
        None,

        /// <summary>
        /// Runs the command as soon as the other peer is online again.
        /// When 10 commands with this flag are called, all 10 commands will be
        /// executed later in exactly this order.
        /// Example use case: A method to add or remove credit (when adding 5, 10
        /// and 30 ct, at the very end the other peer should have received all 45 ct).
        /// </summary>
        RetryWhenOnline,

        /// <summary>
        /// Runs the command as soon as the other peer is online again,
        /// but only the newest call with the same method name is retried.
        /// Example use case: A method to set a configuration file (when first setting
        /// the name to "MyFirstName", then to "MySecondName" and finally to "MyThirdName",
        /// only "MyThirdName" will be sent to the other peer as soon it is online again).
        /// </summary>
        RetryNewestWhenOnline
    }

}
