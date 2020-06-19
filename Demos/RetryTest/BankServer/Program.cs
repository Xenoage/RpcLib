using BankShared;
using DemoServer;
using DemoServer.Rpc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RpcLib.Model;
using RpcLib.Peers.Server;
using RpcLib.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BankClient {

    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // File logging
            string filename = $"BankServer.calclog";
            File.Delete(filename);

            // To understand the actions in the following lines, read the testing
            // strategy described in the RetryTest class in the RetryTest project.



        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
