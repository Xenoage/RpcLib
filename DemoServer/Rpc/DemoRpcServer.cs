using System.Threading.Tasks;
using System;
using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using RpcLib.Rpc.Utils;

public class DemoRpcServer : IDemoRpcServer
{
    
    public async Task SayHello(Greeting greeting)
    {
        Console.WriteLine("TODO"); // TODO
    }

    public async Task<SampleData> ProcessData(SampleData baseData)
    {
        return baseData; // TODO
    }

    public async Task<RpcCommandResult> Execute(RpcCommand command)
    {
        string? resultJson = null;
        switch (command.MethodName) {
            case "SayHello":
                await SayHello(JsonLib.FromJson<Greeting>(command.MethodParameters[0]));
                break;
            case "ProcessData":
                resultJson = JsonLib.ToJson(await ProcessData(JsonLib.FromJson<SampleData>(command.MethodParameters[0])));
                break;
            default:
                throw new Exception("Unknown method name: " + command.MethodName);
        }
        return RpcCommandResult.FromSuccess(command.ID, resultJson);
    }

}