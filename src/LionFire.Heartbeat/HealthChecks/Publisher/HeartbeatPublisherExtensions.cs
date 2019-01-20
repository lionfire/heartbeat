using LionFire.Heartbeat;
using LionFire.Heartbeat.HealthChecks.Publisher;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HeartbeatPublisherExtensions
    {
        public static IHealthChecksBuilder AddHeartbeatPublisher(this IHealthChecksBuilder builder)
        {
            builder.Services
              .AddSingleton<IHealthCheckPublisher>(sp => new HeartbeatPublisher(sp.GetService<HeartbeatSender>()));

            return builder;
        }
    }
}
