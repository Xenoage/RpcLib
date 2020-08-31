using DemoServer.Rpc.Stubs;
using DemoShared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System;
using System.Threading.Tasks;

namespace DemoServer {

    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // Send calculation tasks to all connected clients all 1000 milliseconds.
            // Run all tasks in parallel.
            var random = new Random();
            for (int i = 0; true; i++) {
                foreach (var clientID in RpcMain.GetClientIDs()) {
                    int a = i;
                    int b = random.Next(0, 10);
                    _ = Task.Run(async () => {
                        long startTime = CoreUtils.TimeNow();
                        try {
                            var result = await new CalcRpcStub(clientID).DivideNumbers(a, b);
                            long runTime = CoreUtils.TimeNow() - startTime;
                            Log.Write($"{clientID}: {a}/{b}={result} (runtime: {runTime} ms)");
                        }
                        catch (RpcException ex) {
                            long runTime = CoreUtils.TimeNow() - startTime;
                            Log.Write($"{clientID}: {a}/{b}=Fail! (runtime: {runTime} ms; {ex.Type}: {ex.Message}");
                        }
                    });
                }
                await Task.Delay(10_000);
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
