using RpcLib.Peers;

namespace RpcLib {

    /// <summary>
    /// Interfaces extending this interface define all methods which can be called
    /// on the client side from RPC calls by the server.
    /// Each method must return a Task with either a single JSON-serializable class or no data,
    /// and accept any number of JSON-serializable parameters (or none).
    /// See the DemoShared project, interface IDemoRpcClient, for an example.
    /// </summary>
    public interface IRpcClient : IRpcPeer {
    }

}
