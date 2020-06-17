﻿using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Server.Client;
using System.Threading.Tasks;

namespace DemoClient.Rpc {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="IDemoServerRpc"/> functions.
    /// 
    /// The returned tasks are completed when the response of the server was received.
    /// When there was any problem (server-side exception, network problem, ...) an <see cref="RpcException"/> is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="IDemoServerRpc"/> interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class DemoServerRpcStub : IDemoServerRpc {

        public async Task SayHelloToServer(Greeting greeting) =>
            await RpcClientEngine.ExecuteOnServer(new RpcCommand("SayHelloToServer", greeting));

        public async Task<SampleData> ProcessDataOnServer(SampleData baseData) =>
            await RpcClientEngine.ExecuteOnServer<SampleData>(new RpcCommand("ProcessDataOnServer", baseData));
    }

}