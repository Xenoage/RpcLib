namespace RpcLib.Model {

    /// <summary>
    /// This is the data model which is really sent over the wire.
    /// It consists of, optionally, the next <see cref="RpcCommand"/> to execute, and, also optionally,
    /// the <see cref="RpcCommandResult"/> of the last (successful or failed) execution.
    /// </summary>
    public class RpcMessage {

        public RpcCommand? NextCommand { get; }
        public RpcCommandResult? LastResult { get; }

        public RpcMessage(RpcCommand? nextCall, RpcCommandResult? lastResult) {
            NextCommand = nextCall;
            LastResult = lastResult;
        }

    }

}
