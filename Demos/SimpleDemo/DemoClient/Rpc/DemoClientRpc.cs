using System.Threading.Tasks;
using System;
using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using System.Linq;
using RpcLib;
using RpcLib.Utils;

namespace DemoServer.Rpc {

    /// <summary>
    /// Implementation of the demo RPC client.
    /// </summary>
    public class DemoClientRpc : RpcFunctions, IDemoClientRpc {

        public async Task SayHelloToClient(Greeting greeting) {
            Console.WriteLine("Hello " + greeting.Name + "!");
            if (greeting.MoreData is SampleData moreData)
                Console.WriteLine("More information for you: " + RpcMain.JsonLib.ToJson(moreData));
        }

        public async Task<SampleData> ProcessDataOnClient(SampleData baseData) {
            return new SampleData {
                Text = baseData.Text + "-ClientWasHere",
                Number = baseData.Number / 2,
                List = baseData.List.Select(it => it + "-ClientWasHere").ToList()
            };
        }

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public override Task<string?>? Execute(RpcCommand command) => command.MethodName switch {
            "SayHelloToClient" => SayHelloToClient(command.GetParam<Greeting>(0)).ToJson(),
            "ProcessDataOnClient" => ProcessDataOnClient(command.GetParam<SampleData>(0)).ToJson(),
            _ => null
        };

    }

}