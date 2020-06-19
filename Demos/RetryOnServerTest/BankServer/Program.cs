using BankClient.Rpc.Stubs;
using BankShared;
using DemoServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RpcLib.Model;
using RpcLib.Utils;
using System.IO;
using System.Threading.Tasks;

namespace BankClient {

    /// <summary>
    /// In this simple demo, this server plays the role of a bank customer.
    /// It periodically calls methods on the bank client and prints the results on the console.
    /// Yes, this is kind of weird setup, but it's just for testing the reverse direction
    /// compared to the RetryOnClientTest.
    /// </summary>
    public class Program {

        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // File logging
            string filename = $"BankServer-Server.banklog";
            File.Delete(filename);

            // See the RetryOnServerTest test project to understand what we are doing now.

            int accountNumber = 0;
            var bankClient = new BankClientRpcStub("BankClient-0"); // Bank runs at client 0

            // Repeatedly, get the current account balance and send an increasing amount (1 ct, 2ct, 3ct, ...)
            // to the bank, which is still offline at the beginning. This is done for 20 seconds.
            // Each 5 seconds, change the owner name.
            // TODO bankClient.OnAddMoneyRetryFinished = (command) =>
            //    Log.WriteToFile(filename, $"{command.GetParam<int>(1)} | {command.GetResult().ResultJson} | retried");
            for (int i = 1; i <= 20; i++) {

                // Get current balance (no retry!)
                long startTime = CoreUtils.TimeNow();
                try {
                    int newCents = await bankClient.GetBalance(accountNumber);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Now | {newCents} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Now | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                }

                // Add money (retry for each command)
                startTime = CoreUtils.TimeNow(); ;
                try {
                    int newCents = await bankClient.AddMoney(accountNumber, i);
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Add | {i} | {newCents} | {runTime}ms");
                }
                catch (RpcException ex) {
                    long runTime = CoreUtils.TimeNow() - startTime;
                    Log.WriteToFile(filename, $"Add | {i} | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                }

                // Change owner name (retry for newest call of command)
                if (i % 5 == 0) {
                    startTime = CoreUtils.TimeNow();
                    string newName = "MyName-" + (i / 5);
                    try {
                        await bankClient.ChangeOwnerName(accountNumber, newName);
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.WriteToFile(filename, $"Name | {newName} | {runTime}ms");
                    }
                    catch (RpcException ex) {
                        long runTime = CoreUtils.TimeNow() - startTime;
                        Log.WriteToFile(filename, $"Name | {newName} | Fail: {ex.Type}: {ex.Message} | {runTime}ms");
                    }
                }

                await Task.Delay(1000);
            }

            // Finished. Close server.

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
