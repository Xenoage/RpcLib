using RpcLib.Model;
using System.Threading.Tasks;

namespace RpcLib {

    /// <summary>
    /// Interfaces extending this "marker interface" define all methods which can be called
    /// on the server side from RPC calls by the client.
    /// Each method must return a Task with either a single JSON-serializable class or no data,
    /// and accept any number of JSON-serializable parameters (or none).
    /// See the DemoShared project, interface IDemoRpcServer, for an example.
    /// </summary>
    public interface IRpcServer {

        /// <summary>
        /// Implement this method to map the encoded RPC commands to
        /// the real methods in this class. Hhrown exceptions are catched
        /// and the calling peer gets notified about a
        /// <see cref="RpcFailureType.RemoteException"> failure.
        /// </summary>
        Task<RpcCommandResult> Execute(RpcCommand command);

    }

}
