using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Demo {

    /// <summary>
    /// Implementation of the <see cref="ICalcRpc"/> interface.
    /// This is the class used on the called peer, i.e. it contains the real code.
    /// </summary>
    public class CalcRpc : RpcMethods, ICalcRpc {

        public async Task<int> AddNumbers(int number1, int number2) {
            return number1 + number2;
        }

        public async Task<int> DivideNumbers(int dividend, int divisor) {
            return dividend / divisor; // DivideByZeroException when divisor is 0, this is ok and great for testing
        }

        /// <summary>
        /// Mapping of <see cref="RpcMethod"/> to real method calls (just boilerplate code;
        /// we could auto-generate this method later in .NET 5 with source generators)
        /// </summary>
        public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {
            "AddNumbers" => AddNumbers(method.GetParam<int>(0), method.GetParam<int>(1)).Serialize(),
            "DivideNumbers" => DivideNumbers(method.GetParam<int>(0), method.GetParam<int>(1)).Serialize(),
            _ => null
        };

    }

}
