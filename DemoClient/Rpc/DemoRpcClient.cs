using System.Threading.Tasks;
using System;
using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Rpc.Utils;
using System.Linq;

namespace DemoClient.Rpc {

    /// <summary>
    /// Implementation of the demo RPC client.
    /// </summary>
    public class DemoRpcClient : IDemoRpcClient {

        public async Task SayHelloToClient(Greeting greeting) {
            Console.WriteLine("Hello " + greeting.Name + "!");
            if (greeting.MoreData is SampleData moreData)
                Console.WriteLine("More information for you: " + JsonLib.ToJson(moreData));
        }

        public async Task<SampleData> ProcessDataOnClient(SampleData baseData) {
            return new SampleData {
                Text = baseData.Text + "-ClientWasHere",
                Number = baseData.Number / 2,
                List = baseData.List.Select(it => it + "-ClientWasHere").ToList()
            };
        }

        public async Task<int> DivideNumbers(int dividend, int divisor) {
            return dividend / divisor; // DivideByZeroException when divisor is 0, this is ok and great for testing
        }

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public async Task<RpcCommandResult> Execute(RpcCommand command) =>
            RpcCommandResult.FromSuccess(command.ID, await (command.MethodName switch {
                "SayHelloToClient" => SayHelloToClient(command.GetParam<Greeting>(0)).ToJson(),
                "ProcessDataOnClient" => ProcessDataOnClient(command.GetParam<SampleData>(0)).ToJson(),
                "DivideNumbers" => DivideNumbers(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
                _ => throw new Exception("Unknown method name: " + command.MethodName)
            }));

    }

}