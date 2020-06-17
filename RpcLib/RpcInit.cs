using Microsoft.Extensions.DependencyInjection;
using RpcLib.Peers;
using RpcLib.Peers.Server;
using RpcLib.Server.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace RpcLib {

    /// <summary>
    /// Initialization of the RPC engine for the server and the client.
    /// </summary>
    public static class RpcInit {

        /// <summary>
        /// Initialize the RPC server with the given ASP.NET Core MVC builder, authentication method
        /// and server-side RPC functions.
        /// Call this method during ASP.NET Core startup in the ConfigureServices method. 
        /// </summary>
        public static void InitRpcServer(this IServiceCollection services, IMvcBuilder mvc, Type auth, List<Type> rpcFunctions) {
            // Register this assembly for the MVC module, so that ASP.NET Core can find the RpcApi controller
            mvc.AddApplicationPart(Assembly.Load(new AssemblyName("RpcLib")));
            // Register command runner
            services.AddScoped<RpcCommandRunner>();
            // Register authentication method
            services.AddSingleton(typeof(IRpcAuth), auth);
            // Register given RPC functions
            foreach (var rpcFunction in rpcFunctions)
                services.AddScoped(typeof(RpcFunctions), rpcFunction);
        }

        /// <summary>
        /// Initialize the RPC client with the given configuration, authentication method
        /// and server-side RPC functions.
        /// </summary>
        public static void InitRpcClient(RpcClientConfig config, Action<HttpClient> auth, Func<List<RpcFunctions>> rpcFunctions) {
            RpcClientEngine.Start(rpcFunctions, config, auth);
        }

    }
}
