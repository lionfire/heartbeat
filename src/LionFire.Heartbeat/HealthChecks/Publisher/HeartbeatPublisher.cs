using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Heartbeat.HealthChecks.Publisher
{
    public class HeartbeatPublisher : IHealthCheckPublisher
    {
        private HeartbeatSender heartbeatService;

        public HeartbeatPublisher(HeartbeatSender heartbeatService)
        {
            this.heartbeatService = heartbeatService;
        }

        public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken) => await heartbeatService.SendHeartbeat(h => h.HealthReport = report, cancellationToken);
    }
}
