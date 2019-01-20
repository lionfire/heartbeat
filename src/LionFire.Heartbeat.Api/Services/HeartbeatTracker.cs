using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Heartbeat
{
    public class HeartbeatTracker
    {
        //public IEnumerable<HeartbeatStatus> Active { get; set; }

        //public IEnumerable<HeartbeatStatus> Degraded => active.Values.Where(s => s.Status == HealthStatus.Degraded);
        //public IEnumerable<HeartbeatStatus> Unhealthy => active.Values.Where(s => s.Status == HealthStatus.Unhealthy);


        //public IEnumerable<HeartbeatStatus> Late => active.Values.Where(s => s.HeartbeatState == HeartbeatState.Late);
        //public IEnumerable<HeartbeatStatus> Missing => active.Values.Where(s => s.HeartbeatState == HeartbeatState.Missing);

        //public IEnumerable<HeartbeatStatus> Failed { get; set; }

        //public IEnumerable<HeartbeatStatus> Archived { get; set; }

        //ConcurrentDictionary<string, HeartbeatStatus> active = new ConcurrentDictionary<string, HeartbeatStatus>();
        //ConcurrentDictionary<string, HeartbeatStatus> failed = new ConcurrentDictionary<string, HeartbeatStatus>();
        //ConcurrentDictionary<string, HeartbeatStatus> archived = new ConcurrentDictionary<string, HeartbeatStatus>();

        public IEnumerable<HeartbeatStatus> Statuses => tracking.Values;
        ConcurrentDictionary<string, HeartbeatStatus> tracking = new ConcurrentDictionary<string, HeartbeatStatus>();

        private object _lock = new object();

        private void Update(string id, HeartbeatStatus status)
        {

        }

        public void HealthCheck(HealthCheckResult healthCheck)
        {
            void IdMissing()
            {
                throw new ArgumentException("Data[\"Id\"] must be specified");
            }
            if (healthCheck.Data == null || !healthCheck.Data.ContainsKey("Id"))
            {
                IdMissing();
            }
        }

        private HeartbeatStatus Get(string InstanceId)
        {
            if (InstanceId == null) throw new ArgumentNullException(nameof(InstanceId));
            return tracking.GetOrAdd(InstanceId, id => new HeartbeatStatus(id));
        }

        public void OnInfo(HeartbeatInfo info) => Get(info?.InstanceId).OnInfo(info);

        public HeartbeatStatus OnHeartbeat(Heartbeat heartbeat) => Get(heartbeat?.InstanceId).OnHeartbeat(heartbeat);
    }
}
