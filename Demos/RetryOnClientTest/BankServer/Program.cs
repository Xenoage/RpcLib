using DemoServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace BankClient {

    public class Program {

        /// <summary>
        /// In this simple demo, this server plays the role of a bank.
        /// It listens to commands sent from the customer clients.
        /// </summary>
        public static async Task Main(string[] args) {
            _ = CreateHostBuilder(args).Build().RunAsync();

            // File logging
            string filename = $"BankServer-Server.banklog";
            File.Delete(filename);

            // Run until killed
            while (true)
                await Task.Delay(100000);

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
