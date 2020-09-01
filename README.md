# Xenoage.RpcLib

*A simple .NET Core RPC library for bidirectional, reliable and typesafe communication between an ASP.NET Core server with HTTP/S Web API endpoints and clients behind a firewall.*

[![NuGet version (Xenoage.RpcLib)](https://img.shields.io/nuget/v/Xenoage.RpcLib.svg?style=flat-square)](https://www.nuget.org/packages/Xenoage.RpcLib/)

_Early alpha version - more documentation will follow._

## Overview

![Overview](https://raw.githubusercontent.com/wiki/Xenoage/RpcLib/Drawings/RpcLib-Overview.png)

Illustration of the infrastructure: 1 server with HTTP(S) endpoints, N clients, direct client-to-server calls, indirect server-to-client calls (long polling)

## Usage

After integrating the library in your project as described below, it is very easy to call methods on the remote peer. It feels very natural, just like calling them on the local side! If you are calling from the client to the server or the other way, the code looks the same. A shortened example just to get a feeling:

```c#
// Method declarations (excerpt)
interface IRemoteCalculator : IRpcFunctions {
   Task<int> AddNumbers(int n1, int n2);
}

// Caller-side code (excerpt)
IRemoteCalculator calc = ...;
try {
   int sum = await calc.AddNumbers(5, 10); // This method will be executed on the remote peer
   Console.WriteLine("Result should be 15: " + sum);
}
catch (RpcException ex) {
   if (ex.IsRpcProblem)
      Console.WriteLine("Could not reach the remote peer. Try again later.");
   else
      Console.WriteLine("The remote peer threw an exception: " + ex.Message);
}

// Callee-side code (excerpt)
public async Task<int> AddNumbers(int n1, int n2) {
   return n1 + n2;
}
```

## More information

For more information how to integrate the library into your project, use special features like configuring [timeouts](https://github.com/Xenoage/RpcLib/wiki/Individual-timeouts), [automatic retries](https://github.com/Xenoage/RpcLib/wiki/Automatic-retry), [compression](https://github.com/Xenoage/RpcLib/wiki/Compression) and get explanations on the included demo projects, please have a look in our [wiki](https://github.com/Xenoage/RpcLib/wiki).

This library is very new and changes will happen frequently. Contributions are welcome.
