using System;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Demo {

    /// <summary>
    /// Demo interface for some calculation <see cref="IRpcMethods"/>.
    /// </summary>
    public interface ICalcRpc : IRpcMethods {

        /// <summary>
        /// Returns the sum of the given two numbers.
        /// </summary>
        Task<int> AddNumbers(int number1, int number2);

        /// <summary>
        /// Returns the quotient of the given two numbers.
        /// Throws a <see cref="DivideByZeroException"/> when the divisor is 0.
        /// </summary>
        Task<int> DivideNumbers(int dividend, int divisor);

    }

}
