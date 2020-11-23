namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Simple serializable RPC exception data class.
    /// </summary>
    public class RpcFailure {

        public RpcFailureType Type { get; set; } = RpcFailureType.Other;

        public string? Message { get; set; }

    }
    
}
