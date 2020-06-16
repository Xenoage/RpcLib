using DemoServer.Rpc;
using DemoShared.Rpc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RpcLib.Model;
using RpcLib.Server;
using System;
using System.Threading.Tasks;

namespace DemoServer {

    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // Send calculation tasks to all connected clients all few milliseconds
            var random = new Random();
            for (int i = 0; true; i++) {
                foreach (var clientID in RpcServerEngine.GetClientIDs()) {
                    int a = i;
                    int b = random.Next(0, 10);
                    try {
                        var result = await new DemoRpcClientStub(clientID).DivideNumbers(a, b);
                        Console.WriteLine($"{a}/{b}={result}");
                    }
                    catch (RpcException ex) {
                        Console.WriteLine($"{a}/{b}=? Fail: " + ex.Message);
                    }
                }
                await Task.Delay(1000 + random.Next(0, 100));
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
