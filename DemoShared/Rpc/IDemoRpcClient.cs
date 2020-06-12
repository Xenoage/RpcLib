using DemoShared.Model;
using RpcLib.Peers;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for a <see cref="IRpcPeer"/>.
    /// This interface defines all methods which can be called
    /// on the client side from RPC calls by the server.
    /// </summary>
    public interface IDemoRpcClient : IRpcPeer {

        /// <summary>
        /// Says "hello" to the given name on the console of the client.
        /// </summary>
        Task SayHelloToClient(Greeting greeting);

        /// <summary>
        /// Modifies the given data on the client and returns it.
        /// Strings are suffixed by "-ClientWasHere", numbers are divided by 2.
        /// </summary>
        Task<SampleData> ProcessDataOnClient(SampleData baseData);

    }

}
