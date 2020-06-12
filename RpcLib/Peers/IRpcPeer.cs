using RpcLib.Model;
using System;
using System.Threading.Tasks;

namespace RpcLib.Peers {

    /// <summary>
    /// Base interface for <see cref="IRpcClient"/> and <see cref="IRpcServer"/>.
    /// </summary>
    public interface IRpcPeer {

        /// <summary>
        /// Implement this method to map the encoded RPC commands to
        /// the real methods in this class. Thrown exceptions are catched
        /// and the calling peer gets notified about a
        /// <see cref="RpcFailureType.RemoteException"> failure.
        /// </summary>
        Task<RpcCommandResult> Execute(RpcCommand command) {
            throw new NotImplementedException("Could not execute the command, since the " +
                nameof(IRpcPeer) + " interface is not implemented."); // TIDY
        }

    }

}
