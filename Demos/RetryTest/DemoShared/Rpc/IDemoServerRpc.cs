using DemoShared.Model;
using RpcLib;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for server-side <see cref="IRpcFunctions"/>.
    /// This interface defines some methods which can be called
    /// on the server by RPC calls from the client.
    /// </summary>
    public interface IDemoServerRpc : IRpcFunctions {

        /// <summary>
        /// Says "hello" to the given name on the console of the server.
        /// </summary>
        Task SayHelloToServer(Greeting greeting);

        /// <summary>
        /// Modifies the given data on the server and returns it.
        /// Strings are suffixed by "-ServerWasHere", numbers are multipied by 2.
        /// </summary>
        Task<SampleData> ProcessDataOnServer(SampleData baseData);

    }

}
