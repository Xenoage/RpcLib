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

        public async Task<RpcCommandResult> Execute(RpcCommand command) {
            string? resultJson = null;
            switch (command.MethodName) {
                case "SayHelloToClient":
                    await SayHelloToClient(JsonLib.FromJson<Greeting>(command.MethodParameters[0]));
                    break;
                case "ProcessDataOnClient":
                    resultJson = JsonLib.ToJson(await ProcessDataOnClient(JsonLib.FromJson<SampleData>(command.MethodParameters[0])));
                    break;
                default:
                    throw new Exception("Unknown method name: " + command.MethodName);
            }
            return RpcCommandResult.FromSuccess(command.ID, resultJson);
        }

    }

}