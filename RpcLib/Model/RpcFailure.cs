namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Simple serializable RPC exception data class.
    /// </summary>
    public class RpcFailure {

        public RpcFailureType Type { get; set; } = RpcFailureType.Other;

        public string? Message { get; set; } = null;

        public static RpcFailure Other(string message) => new RpcFailure {
            Type = RpcFailureType.Other,
            Message = message
        };

        public override bool Equals(object? obj) {
            return obj is RpcFailure failure &&
                   Type == failure.Type &&
                   Message == failure.Message;
        }
    }
    
}
