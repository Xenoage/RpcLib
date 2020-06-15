using DemoShared.Model;
using RpcLib;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for a <see cref="IRpcServer"/>.
    /// This interface defines all methods which can be called
    /// on the server by RPC calls from the client.
    /// </summary>
    public interface IDemoRpcServer : IRpcServer {

        /// <summary>
        /// Says "hello" to the given name on the console of the server.
        /// </summary>
        Task SayHelloToServer(Greeting greeting);

        /// <summary>
        /// Modifies the given data on the server and returns it.
        /// Strings are suffixed by "-ServerWasHere", numbers are multipied by 2.
        /// </summary>
        Task<SampleData> ProcessDataOnServer(SampleData baseData);

        /// <summary>
        /// Returns the sum of the given two numbers.
        /// </summary>
        Task<int> AddNumbers(int number1, int number2);

    }

}
