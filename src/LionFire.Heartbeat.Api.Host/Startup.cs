#define DotNetify
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if DotNetify
using DotNetify;
using LionFire.AspNetCore.Mvc.Controllers;
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
using WebApiContrib.Core;

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
            //services.AddMvc()
            //           .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            //           ;
            services.AddSingleton<HeartbeatLog>();
            services.AddSingleton<HeartbeatTracker>();

        }

        public void Configure(IApplicationBuilder a, IHostingEnvironment env)
        {
            a.UseBranchWithServices("/api", services =>
            {
                services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .ConfigureApplicationPartManager(manager =>
                    {
                        manager.FeatureProviders.Clear();
                        manager.FeatureProviders.Add(new TypedControllerFeatureProvider<IPublicApi>());
                    })
                    .AddApplicationPart(typeof(HeartbeatReceiverController).Assembly)
                    ;

                services.AddSingleton(a.ApplicationServices.GetService<HeartbeatLog>());
                services.AddSingleton(a.ApplicationServices.GetService<HeartbeatTracker>());
                services.Configure<HeartbeatTrackerOptions>(Configuration.GetSection("HeartbeatTracker"));

                services.TryAddEnumerable(new ServiceDescriptor(typeof(IHeartbeatAlerter), typeof(PushoverHeartbeatAlerter), ServiceLifetime.Singleton));
                services.Configure<PushoverAlerterOptions>(Configuration.GetSection("Pushover"));

                services.Configure<HeartbeatAlerterOptions>(Configuration.GetSection("HeartbeatAlerter"));
                services.AddHostedService<HeartbeatAlerter>();

            }, app =>
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.Use(async (context, next) =>
                {
                    var port = context.Request.HttpContext.Request.Host.Port;
                    if (!port.HasValue || port.Value != 7777)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Wrong port: " + port);
                    }
                    else
                    {
                        await next();
                    }
                });

                app.UseMvc();
                //app.UseExceptionHandler()
            });

            a.UseBranchWithServices("/admin", services =>
            {
#if DotNetify
                services.AddMemoryCache();
                services.AddSignalR();
                services.AddDotNetify();
#endif

                services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .ConfigureApplicationPartManager(manager =>
                    {
                        manager.FeatureProviders.Clear();
                        manager.FeatureProviders.Add(new TypedControllerFeatureProvider<IAdminApi>());
                    })
                    .AddApplicationPart(typeof(LionFire.Heartbeat.Api.Controllers.HeartbeatsController).Assembly)
                    ;

                services.AddSingleton(a.ApplicationServices.GetService<HeartbeatLog>());
                services.AddSingleton(a.ApplicationServices.GetService<HeartbeatTracker>());
                services.Configure<HeartbeatTrackerOptions>(Configuration.GetSection("HeartbeatTracker"));

            }, app =>
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.Use(async (context, next) =>
                {
                    var port = context.Request.HttpContext.Request.Host.Port;
                    if (!port.HasValue || port.Value != 7780)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Wrong port: " + port);
                    }
                    else
                    {
                        await next();
                    }
                });

#if DotNetify
                app.UseWebSockets();
                app.UseSignalR(routes => routes.MapDotNetifyHub());
                app.UseDotNetify();
#endif

                app.UseStaticFiles();
                app.UseMvc();
                app.Run(async (context) =>
                {
                    using (var reader = new StreamReader(File.OpenRead("wwwroot/index.html")))
                        await context.Response.WriteAsync(reader.ReadToEnd());
                });
            });

            a.Run(async c =>
            {
                await c.Response.WriteAsync("Nothing here!");
            });

        }
    }
}
