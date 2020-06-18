# Xenoage.RpcLib

*A simple .NET Core RPC library for bidirectional communication between an ASP.NET Core server with HTTP(S) Web API endpoints and clients behind a firewall.*

[![NuGet version (Xenoage.RpcLib)](https://img.shields.io/nuget/v/Xenoage.RpcLib.svg?style=flat-square)](https://www.nuget.org/packages/Xenoage.RpcLib/)

_Early alpha version - more documentation will follow._

## Overview

_TODO: Illustration of the infrastructure: 1 server with HTTP(S) endpoints, N clients, direct client-to-server calls, indirect server-to-client calls (long polling)_

## Usage

After integrating the library in your project as described below, it is very easy to call methods on the remote peer. It feels very natural, just like calling them on the local side! If you are calling from the client to the server or the other way, the code looks the same. A shortened example just to get a feeling:

```
interface IRemoteCalculator : IRpcFunctions {
   Task<int> AddNumbers(int n1, int n2);
}

IRemoteCalculator calc = ...;
try {
   int sum = await calc.AddNumbers(5, 10);
   Console.WriteLine("Result should be 15: " + sum);
}
catch (RpcException ex) {
   if (ex.IsRpcProblem)
      Console.WriteLine("Could not reach the remote peer. Try again later.");
   else
      Console.WriteLine("The remote peer threw an exception: " + ex.Message);
}
```

## Integration into in your project

Before adding the library to your own project, we recommend having a look at the provided [example](https://github.com/Xenoage/RpcLib) code and following these steps:

### 1) Project setup

1. You need at least two projects: the server and the client. In our [example](https://github.com/Xenoage/RpcLib) code, these projects are named `DemoServer` and `DemoClient`.
   * The server project must be an ASP.NET Core project, since the RPC handler on the server side is based on an ASP.NET Core Web API controller.
   * The client project can be of any kind, e.g. a simple console application.
2. An additional class library project is recommended, which is referenced by both the client and the server. This project is called `DemoShared` in the example code.
3. Add a reference to this library to your projects, e.g. by using the NuGet GUI or by calling `dotnet add package Xenoage.RpcLib`

### 2) Define RPC methods

1. In the shared class library project (or in any source folder both the server and the client can access) define the interfaces containing the methods which are available on the server (see example [`IDemoServerRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/IDemoServerRpc.cs)) and on the client (see example [`IDemoClientRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/IDemoClientRpc.cs)). It is also possible to distribute the functions over several interfaces/classes and use the implementations both server-side and client-side, as demonstrated with the example [`ICalcRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/ICalcRpc.cs).
   * All methods must return a `Task` or a `Task<T>`, because they are called asynchronously.
   * Both the return type (if any) and the parameters (if any) must be (de)serializable to/from JSON. Internally, we use the Newtonsoft.JSON library for (de)serialization. 
   
### 3) Implement the client side

1. On the client side, a so called "stub" for each server interface is needed. Within this stub class, the method calls are simply encoded and forwarded to the RPC engine, which runs the commands on the server and returns the results to the caller. See the examples [`DemoServerRpcStub`](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Rpc/DemoServerRpcStub.cs) and [`CalcRpcStub`](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Rpc/CalcRpcStub.cs) how to implement a stub (this is just boilerplate code; we could auto-generate it in a later version of this library from the server interface!).
2. Implement the client interfaces with the "real" logic. See the examples [`DemoClientRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Rpc/DemoClientRpc.cs) and [`CalcRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/CalcRpc.cs).
3. Initialize the RPC library on the client side (see example [`Program`](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Program.cs), the lines after `// RPC initialization` ):
   * Create an instance of your server stub class and use it wherever you want to call the server.
   * Call the `RpcInit.InitRpcClient` method for instantiating the RPC engine, using your client configuration (including the unique client ID and the URL of the server endpoint), an authentication method (e.g. HTTP Basic Auth) and a factory method which returns the concrete
   implementations of your client-side methods.

### 4) Implement the server side

1. Like on the client side, we also need "stubs" on the server side. See the examples [`DemoClientRpcStub`](https://github.com/Xenoage/RpcLib/blob/master/DemoServer/Rpc/DemoClientRpcStub.cs) and [`CalcRpcStub`](https://github.com/Xenoage/RpcLib/blob/master/DemoServer/Rpc/CalcRpcStub.cs).
2. Implement the server interfaces with the "real" logic. See the example [`DemoServerRpc`](https://github.com/Xenoage/RpcLib/blob/master/DemoServer/Rpc/DemoServerRpc.cs). Within the method calls, you can use the property `Context` to query the calling client's ID and a service factory to use ASP.NET Core dependency injection (see the method `SayHelloToServer` for an example).
3. Initialize the RPC library on the server side (see example [`Startup`](https://github.com/Xenoage/RpcLib/blob/master/DemoServer/Startup.cs), the lines after `// RPC initialization` ):
   * Call the `RpcInit.InitRpcServer` method for instantiating the RPC engine, using the ASP.NET Core services, an authentication method (e.g. HTTP Basic Auth) and a list of the types of the
   implementations of your server-side methods.
4. To call the clients, create instances of your client stubs and call their methods. To get a list of the currently connected client IDs, call `RpcServerEngine.GetClientIDs()`

## Special features

### Run remote functions as soon as the other peer is online

Normally, when an RPC command is called, it has a defined time period to execute. During this time, the call is retried several times automatically. But when the timeout is hit (by default 30 seconds), the RPC command returns a timeout failure and is not tried automatically again.

This can be changed for important commands, which should reach the other side as soon as it is online again, maybe even after restarting the computer. For this, when executing an `RpcCommand`, an additional [`RpcRetryStrategy`](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Model/RpcRetryStrategy.cs) parameter can be given, which is `None` by default. There are two other strategies:

* `RetryWhenOnline`: Runs the command as soon as the other peer is online again. When 10 commands with this flag are called, all 10 commands will be executed later in exactly this order. Example use case: A method to add or remove credit (when adding 5, 10 and 30 ct, at the very end the other peer should have received all 45 ct).
  
* `RetryNewestWhenOnline`: Runs the command as soon as the other peer is online again, but only the newest call with the same method name is retried. Example use case: A method to set a configuration file (when first setting the name to "MyFirstName", then to "MySecondName" and finally to "MyThirdName", only "MyThirdName" will be sent to the other peer as soon it is online again).

When the other peer is online again, first the commands in this queue will be sent, and after that the recently called "normal" commands will be sent (if their timeout was not already hit).

The RPC engine needs some way to persist the remembered commands, but does not include an own database for this. So, when using one of the above strategies, an implementation of the `IRpcCommandBacklog` interface has to be provided when initializing the RPC peer. See `DemoRpcCommandBacklog` for a very simple implementation using a JSON file for persisting the queue.