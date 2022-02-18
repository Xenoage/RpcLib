namespace Utils;

public class RpcException : Exception {

    public RpcFailureType Failure { get; }

    public RpcException(RpcFailureType failure, string message = "") : base(message) {
        Failure = failure;
    }

}
