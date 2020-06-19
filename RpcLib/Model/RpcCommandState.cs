using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RpcLib.Model {

    /// <summary>
    /// The current state of an <see cref="RpcCommand"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcCommandState {
        /// <summary>
        /// The start state. It was just created.
        /// </summary>
        Created,
        /// <summary>
        /// The call is enqueued and will be sent as soon as possible.
        /// </summary>
        Enqueued,
        /// <summary>
        /// The call was sent and, as soon as it reached the remote side, is executing there.
        /// </summary>
        Sent,
        /// <summary>
        /// The response of the call was received. It was executed successfully on the remote side.
        /// The response value (if not void) can be retrieved from <see cref="RpcCommandResult.ResultJson"/>.
        /// </summary>
        Successful,
        /// <summary>
        /// The call failed, either on the local side (because of a timeout for example) or on
        /// the remote side (exception during execution).
        /// See <see cref="RpcCommandResult.Failure"/> for details.
        /// </summary>
        Failed
    }

}
