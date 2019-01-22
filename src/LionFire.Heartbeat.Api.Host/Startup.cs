#define DotNetify
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if DotNetify
using DotNetify;
#endif
using LionFire.Heartbeat;
using LionFire.Heartbeat.Api;
using LionFire.Monitoring.Heartbeat.Alerters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Monitoring.Heartbeat.Api.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
#if DotNetify
            services.AddMemoryCache();
            services.AddSignalR();
            services.AddDotNetify();
#endif

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(typeof(HeartbeatReceiverController).Assembly)
                ;

            services.AddSingleton<HeartbeatLog>();
            services.AddSingleton<HeartbeatTracker>();
            services.Configure<HeartbeatTrackerOptions>(Configuration.GetSection("HeartbeatTracker"));

            services.TryAddEnumerable(new ServiceDescriptor(typeof(IHeartbeatAlerter), typeof(PushoverHeartbeatAlerter), ServiceLifetime.Singleton));
            services.Configure<PushoverAlerterOptions>(Configuration.GetSection("Pushover"));

            services.Configure<HeartbeatAlerterOptions>(Configuration.GetSection("HeartbeatAlerter"));
            services.AddHostedService<HeartbeatAlerter>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseWebSockets();
            app.UseSignalR(routes => routes.MapDotNetifyHub());
            app.UseDotNetify();

            app.UseStaticFiles();
            app.UseMvc();
            app.Run(async (context) =>
            {
                using (var reader = new StreamReader(File.OpenRead("wwwroot/index.html")))
                    await context.Response.WriteAsync(reader.ReadToEnd());
            });
        }
    }
}
