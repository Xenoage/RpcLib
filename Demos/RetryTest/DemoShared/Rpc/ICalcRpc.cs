using RpcLib;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Demo interface for some additional calculation <see cref="RpcFunctions"/>, both client-side and server-side.
    /// This demonstrates, that
    /// * multiple interfaces can be registered to distribute the registered
    ///   functions over several interfaces/classes
    /// * the same interfaces/classes can be used both on the client side and the server side
    /// </summary>
    public interface ICalcRpc : IRpcFunctions {

        /// <summary>
        /// Returns the sum of the given two numbers.
        /// </summary>
        Task<int> AddNumbers(int number1, int number2);

        /// <summary>
        /// Returns the quotient of the given two numbers.
        /// </summary>
        Task<int> DivideNumbers(int dividend, int divisor);

    }

}
