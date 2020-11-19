namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Simple serializable RPC exception data class.
    /// </summary>
    public class RpcFailure {

        public RpcFailure(RpcFailureType type, string message) {
            Type = type;
            Message = message;
        }

        public RpcFailureType Type { get; }

        public string Message { get; }

    }
    
}
