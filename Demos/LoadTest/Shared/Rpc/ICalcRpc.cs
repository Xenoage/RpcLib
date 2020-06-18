using RpcLib;
using System.Threading.Tasks;

namespace Shared.Rpc {

    /// <summary>
    /// Calculation <see cref="RpcFunctions"/> for the load test.
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
