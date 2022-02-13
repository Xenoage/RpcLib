using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;

namespace Xenoage.RpcLib.Methods {

    /// <summary>
    /// Base class for the <see cref="IRpcMethods"/> implementations on the caller side,
    /// i.e. containing code to call the remote side.
    /// </summary>
    public abstract class RpcMethodsStub : IRpcMethods {

        /// <summary>
        /// The local RPC instance, either a <see cref="RpcClient"/> or a <see cref="RpcServer"/>.
        /// </summary>
        public IRpcPeer LocalPeer { get; }

        /// <summary>
        /// The ID of the client on which to run the commands, or null for the server.
        /// </summary>
        public string? RemotePeerID { get; }

        /// <summary>
        /// Creates a new callee-side stub, when the local side is a <see cref="RpcClient"/>
        /// which wants to talk to the remote server.
        /// </summary>
        public RpcMethodsStub(RpcClient localClient) {
            LocalPeer = localClient;
            RemotePeerID = null;

            // GOON!!
            localClient.RegisteredMethodStubs.Add(this);
        }

        /// <summary>
        /// Creates a new callee-side stub, when the local side is a <see cref="RpcServer"/>
        /// which wants to talk to the given client ID.
        /// </summary>
        public RpcMethodsStub(RpcServer localServer, string remoteClientID) {
            LocalPeer = localServer;
            RemotePeerID = remoteClientID;
        }

        /// <summary>
        /// Runs the given RPC method on the remote peer as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        protected Task<T> ExecuteOnRemotePeer<T>(string methodName, params object[] methodParameters) =>
            LocalPeer.ExecuteOnRemotePeer<T>(RemotePeerID, methodName, methodParameters);

        /// <summary>
        /// Like <see cref="ExecuteOnRemotePeer{T}"/> but without return value.
        /// </summary>
        protected Task ExecuteOnRemotePeer(string methodName, params object[] methodParameters) =>
            ExecuteOnRemotePeer<object>(methodName, methodParameters);

        /// <summary>
        /// Register for all events with the given names on the remote peer.
        /// </summary>
        protected void RegisterEventsOnRemotePeer() {
            var eventNames = GetRegisteredEventNames().ToArray();
            Log.Debug($"Registering events on remote peer {RemotePeerID}: " + string.Join(", ", eventNames));
            _ = ExecuteOnRemotePeer<object>("!RegisterEvents", eventNames);
        }

        protected virtual IEnumerable<string> GetRegisteredEventNames() =>
            ImmutableList<string>.Empty;

        /// <summary>
        /// Raises the given event, serialized in the given <see cref="RpcMethod"/>.
        /// When there are no events in the implementing class, this method does not
        /// have to be overridden.
        /// </summary>
        public virtual void ExecuteEventOnLocalPeer(RpcMethod evt) { }

    }

}
