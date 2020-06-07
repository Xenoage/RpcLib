using Shared.Model;
using System.Threading.Tasks;

namespace Shared.Rpc {

    /// <summary>
    /// This interface defines all methods which can be called
    /// on the client side from RPC calls by the server.
    /// Each method must return a Task with either a single JSON-serializable class or no data,
    /// and accept either a single JSON-serializable parameter or none.
    /// </summary>
    public interface IRpcClient {

        // Demo content:

        /// <summary>
        /// Says "hello" to the given name on the console.
        /// </summary>
        Task SayHello(Greeting greeting);

        /// <summary>
        /// Modifies the given data on the client and returns it.
        /// Strings are suffixed by "-ClientWasHere", numbers are divided by 2.
        /// </summary>
        Task<SampleData> ProcessData(SampleData baseData);

    }

}
