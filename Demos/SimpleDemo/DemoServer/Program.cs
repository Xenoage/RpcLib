using DemoServer.Rpc.Stubs;
using DemoShared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RpcLib.Model;
using RpcLib.Peers.Server;
using RpcLib.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoServer {

    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // File logging
            string filename = $"DemoServer.calclog";
            File.Delete(filename);

            // Send calculation tasks to all connected clients all 500-1000 milliseconds.
            // Run all tasks in parallel.
            var random = new Random();
            for (int i = 0; true; i++) {
                foreach (var clientID in RpcServerEngine.Instance.GetClientIDs()) { // TIDY
                    int a = i;
                    int b = random.Next(0, 10);
                    _ = Task.Run(async () => {
                        long startTime = CoreUtils.TimeNow();
                        try {
                            var result = await new CalcRpcStub(clientID).DivideNumbers(a, b);
                            long rpcTime = CoreUtils.TimeNow() - startTime;
                            var log = $"{clientID} | {a}/{b}={result} | {rpcTime} ms";
                            Log.Write(log);
                            Log.WriteToFile(filename, log);
                        }
                        catch (RpcException ex) {
                            long rpcTime = CoreUtils.TimeNow() - startTime;
                            var log = $"{clientID} | {a}/{b}=? | {rpcTime} ms | Fail: {ex.Type}: {ex.Message}";
                            Log.Write(log);
                            Log.WriteToFile(filename, log);
                        }
                    });
                }
                await Task.Delay(random.Next(500, 1000));
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
