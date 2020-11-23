using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Serialization;

/// <summary>
/// 
/// List of TODOs
/// *************
/// 
/// - Prevent CSWSH (Cross-Site WebSocket Hijacking) by using a whitelist of the allowed origins.
///   See: https://www.codetinkerer.com/2018/06/05/aspnet-core-websockets.html
/// 
/// - Ignore null values when serializing JSON. .NET 5 is required, see <see cref="JsonSerializer"/>.
/// 
/// - Implement <see cref="RpcCall.SerializerID"/>
/// 
/// - Rename "target" peer ID to "remote" peer ID
/// 
/// </summary>