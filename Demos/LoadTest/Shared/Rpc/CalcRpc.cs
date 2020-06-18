using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Threading.Tasks;

namespace Shared.Rpc {

    /// <summary>
    /// Implementation of the <see cref="ICalcRpc"/> interface.
    /// </summary>
    public class CalcRpc : RpcFunctions, ICalcRpc {

        public async Task<int> AddNumbers(int number1, int number2) {
            return number1 + number2;
        }

        public async Task<int> DivideNumbers(int dividend, int divisor) {
            return dividend / divisor; // DivideByZeroException when divisor is 0, this is ok and great for testing
        }

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public override Task<string?>? Execute(RpcCommand command) => command.MethodName switch
        {
            "AddNumbers" => AddNumbers(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
            "DivideNumbers" => DivideNumbers(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
            _ => null
        };

    }
}
