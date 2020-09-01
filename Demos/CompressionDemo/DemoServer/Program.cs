using DemoServer.Rpc.Stubs;
using DemoShared;
using DemoShared.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoServer {

    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // Let the client send its commands first, to group them together in a
            // network analysis software like Fiddler.
            // Wait for 25 seconds, then send the server commands.
            await Task.Delay(25_000);

            // Compression strategies
            var client = new TextRpcStub("DemoClient");
            var strategies = new List<(string, Func<string, Task<string>>)> {
                ("Auto", client.CapitalizeText),
                ("Compressed", client.CapitalizeText_Compressed),
                ("Uncompressed", client.CapitalizeText_Uncompressed)
            };
            // Message in different sizes
            var texts = TestText.GetDemoTexts();

            // Call client
            foreach (var strategy in strategies) {
                Log.Write("Using strategy: " + strategy.Item1);
                foreach (var text in texts) {
                    Log.Write($"  Sending text with {text.Length} chars");
                    var result = await strategy.Item2.Invoke(text);
                    // Check result
                    if (result.Length != text.Length)
                        Log.Write("WARNING: Unexpected result: different length");
                    else if (result != text.ToUpper())
                        Log.Write("WARNING: Unexpected result: wrong content");
                    // Wait a second before the next call
                    await Task.Delay(1000);
                }
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
