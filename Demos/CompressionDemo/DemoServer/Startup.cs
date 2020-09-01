using DemoServer.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DemoShared.Rpc;
using RpcLib;
using System.Collections.Generic;
using System;

namespace DemoServer {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            var mvc = services.AddControllers();

            // RPC initialization
            services.InitRpcServer(mvc, typeof(DemoRpcAuth), new List<Type> {
                typeof(TextRpc)
            }, new RpcSettings {
                CompressionThresholdBytes = 1500 // Auto-compress messages >= 1.5 kB in this test
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
