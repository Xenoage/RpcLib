using DemoShared.Model;
using RpcLib;
using RpcLib.Server;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for client-side <see cref="RpcFunctions"/>.
    /// This interface defines some methods which can be called
    /// on the client by RPC calls from the server.
    /// </summary>
    public interface IDemoClientRpc : IRpcFunctions {

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
