using DemoShared.Model;
using RpcLib;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for a <see cref="IRpcClient"/>.
    /// This interface defines all methods which can be called
    /// on the client side from RPC calls by the server.
    /// </summary>
    public interface IDemoRpcClient : IRpcClient {

        /// <summary>
        /// Says "hello" to the given name on the console of the client.
        /// </summary>
        Task SayHello(Greeting greeting);

        /// <summary>
        /// Modifies the given data on the client and returns it.
        /// Strings are suffixed by "-ClientWasHere", numbers are divided by 2.
        /// </summary>
        Task<SampleData> ProcessData(SampleData baseData);

    }

}
