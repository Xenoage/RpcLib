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

    public async Task<RpcCommandResult> Execute(RpcCommand command) {
        string? resultJson = null;
        switch (command.MethodName) {
            case "SayHelloToServer":
                await SayHelloToServer(JsonLib.FromJson<Greeting>(command.MethodParameters[0]));
                break;
            case "ProcessDataOnServer":
                resultJson = JsonLib.ToJson(await ProcessDataOnServer(JsonLib.FromJson<SampleData>(command.MethodParameters[0])));
                break;
            case "AddNumbers":
                resultJson = JsonLib.ToJson(await AddNumbers(
                    JsonLib.FromJson<int>(command.MethodParameters[0]), JsonLib.FromJson<int>(command.MethodParameters[1])));
                break;
            default:
                throw new Exception("Unknown method name: " + command.MethodName);
        }
        return RpcCommandResult.FromSuccess(command.ID, resultJson);
    }
    
}