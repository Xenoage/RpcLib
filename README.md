# RpcLib

_Early alpha version - more documentation will follow._

Simple .NET Core RPC library for bidirectional communication based on an ASP.NET Core server and clients behind firewall

## Usage

### 1) Define RPC methods

1. In a shared class library project (or in any source folder both the server and the client can access)
   define the interfaces containing the methods which are available on the server
   (see [example](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/IDemoRpcServer.cs)) and on the client
   (see [example](https://github.com/Xenoage/RpcLib/blob/master/DemoShared/Rpc/IDemoRpcClient.cs)).
   
### 2) Implement the server side

1. _TODO_


### 3) Implement the client side

1. On the client side, a so called "stub" for the server interface is needed. Within this stub class, the method
   calls are simply encoded and forwarded to the RPC engine, which runs the commands on the server and returns
   the results to the caller. See this
   [example](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Rpc/DemoRpcServerStub.cs)
   how to implement a stub (this is just boilerplate code; we could auto-generate
   it in a later version of this library from the server interface!).
2. Implement the client interface with the "real" logic. See this
   [example](https://github.com/Xenoage/RpcLib/blob/master/DemoClient/Rpc/DemoRpcClient.cs).
3. Initialize the RPC library on the client side: _TODO_
