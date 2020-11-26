using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;
using Xenoage.RpcLib.Serialization;

/// <summary>
/// 
/// List of TODOs
/// *************
/// 
/// - Prevent CSWSH (Cross-Site WebSocket Hijacking) by using a whitelist of the allowed origins.
///   See: https://www.codetinkerer.com/2018/06/05/aspnet-core-websockets.html
/// 
/// - When switching to .NET 5:
///   - Ignore null values when serializing JSON, see <see cref="JsonSerializer"/>
///   - Use <see cref="TaskCompletionSource"/> without generic, see <see cref="RpcCallExecution"/> and <see cref="RpcChannel"/>
///   - Autogenerate implementations of <see cref="RpcMethods.Execute"/> and the stubs using C# source generator
/// 
/// - Implement <see cref="RpcCall.SerializerID"/>
/// 
/// - Rename "target" peer ID to "remote" peer ID
/// 
/// - Make all classes internal, that are not required outside the library
/// 
/// - Optimise <see cref="RpcPeer.ApplyRpcOptionsFromCallStack"/> by caching the results
/// 
/// </summary>