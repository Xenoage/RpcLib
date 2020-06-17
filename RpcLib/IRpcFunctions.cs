namespace RpcLib {

    /// <summary>
    /// Interfaces extending this interface define methods which can be called by RPC commands
    /// on the client side by the server or on the server side by the client.
    /// Each method must return a Task with either a single JSON-serializable class or no data,
    /// and accept any number of JSON-serializable parameters (or none).
    /// See the DemoShared project, interface IDemoRpcClient for a client-side example
    /// and the interface IDemoRpcServer for a server-side example.
    /// </summary>
    public interface IRpcFunctions {
    }

}
