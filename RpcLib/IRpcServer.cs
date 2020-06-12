using RpcLib.Peers;

namespace RpcLib {

    /// <summary>
    /// Interfaces extending this interface define all methods which can be called
    /// on the server side from RPC calls by the client.
    /// Each method must return a Task with either a single JSON-serializable class or no data,
    /// and accept any number of JSON-serializable parameters (or none).
    /// See the DemoShared project, interface IDemoRpcServer, for an example.
    /// </summary>
    public interface IRpcServer : IRpcPeer {
    }

}
