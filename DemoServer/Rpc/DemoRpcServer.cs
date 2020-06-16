using System.Threading.Tasks;
using System;
using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Rpc.Utils;
using System.Linq;

/// <summary>
/// Implementation of the demo RPC server.
/// </summary>
public class DemoRpcServer : IDemoRpcServer {

    public async Task SayHelloToServer(Greeting greeting) {
        Console.WriteLine("Hello " + greeting.Name + "!");
        if (greeting.MoreData is SampleData moreData)
            Console.WriteLine("More information for you: " + JsonLib.ToJson(moreData));
    }

    public async Task<SampleData> ProcessDataOnServer(SampleData baseData) {
        return new SampleData {
            Text = baseData.Text + "-ServerWasHere",
            Number = baseData.Number * 2,
            List = baseData.List.Select(it => it + "-ServerWasHere").ToList()
        };
    }

    public async Task<int> AddNumbers(int number1, int number2) {
        return number1 + number2;
    }

    // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
    public async Task<RpcCommandResult> Execute(RpcCommand command) =>
        RpcCommandResult.FromSuccess(command.ID, await (command.MethodName switch
        {
            "SayHelloToServer" => SayHelloToServer(command.GetParam<Greeting>(0)).ToJson(),
            "ProcessDataOnServer" => ProcessDataOnServer(command.GetParam<SampleData>(0)).ToJson(),
            "AddNumbers" => AddNumbers(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
            _ => throw new Exception("Unknown method name: " + command.MethodName)
        }));
    
}