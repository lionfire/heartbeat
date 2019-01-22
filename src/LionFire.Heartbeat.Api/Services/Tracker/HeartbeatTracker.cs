using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Heartbeat
{
    public class HeartbeatTracker
    {
        #region Dependencies

        private IOptionsMonitor<HeartbeatTrackerOptions> options;
        private HeartbeatLog heartbeatLog;

        #endregion

        #region Construction

        public HeartbeatTracker(IOptionsMonitor<HeartbeatTrackerOptions> options, HeartbeatLog heartbeatLog)
        {
            this.options = options;
            this.heartbeatLog = heartbeatLog;
        }

        #endregion

        #region Methods

        public bool AckMissing(string instanceId)
        {
            if (tracking.TryRemove(instanceId, out HeartbeatStatus status))
            {
                AckedMissing.Add(status);
                Log(new HeartbeatTrackerLogItem(LogLevel.Information, "Ack Missing", $"Acked missing status of {status.Key}", true, 1)
                {
                    Data = new Dictionary<string, object>
                    {
                        ["Status"] = status,
                        //["ProgramName"] = status?.Info?.ProgramName,
                    }
                });
                return true;
            }
            Log(new HeartbeatTrackerLogItem(LogLevel.Information, "Ack Missing (fail)", $"Attempted to ack missing status of {status.Key} but it is not ackable.", false));
            return false;
        }

        #endregion

        #region Log

        protected void Log(HeartbeatTrackerLogItem logItem) => heartbeatLog.Log(logItem);

        #endregion

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
        public List<HeartbeatStatus> AckedMissing { get; } = new List<HeartbeatStatus>();

        internal ConcurrentDictionary<string, HeartbeatStatus> tracking = new ConcurrentDictionary<string, HeartbeatStatus>();

        private readonly object _lock = new object();

        //private void Update(string id, HeartbeatStatus status)
        //{
        //}

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

        private (bool created, HeartbeatStatus status) Get(string InstanceId)
        {
            if (InstanceId == null)
            {
                throw new ArgumentNullException(nameof(InstanceId));
            }
            bool created = false;
            var result = tracking.GetOrAdd(InstanceId, id =>
            {
                created = true;
                return new HeartbeatStatus(id);
            });
            return (created, result);
        }

        public void OnInfo(HeartbeatInfo info)
        {
            var status = Get(info?.InstanceId).status;
            status.OnInfo(info);
            //if (!hasInfo && status.Info != null)
            {
                Log(new HeartbeatTrackerLogItem(LogLevel.Information, "New heartbeat", "New heartbeat: " + status.Key, true));
            }
        }

        public void CreateConfigFromServer(HeartbeatResponse r, HeartbeatStatus s)
        {
            s.ConfigFromServer = new HeartbeatConfigFromServer
            {
                MaxInterval = options.CurrentValue.MaxHeartbeatInterval,
                MinInterval = options.CurrentValue.MinHeartbeatInterval,
            };
            r.config = s.ConfigFromServer;
        }

        public HeartbeatResponse OnHeartbeat(Heartbeat heartbeat)
        {
            (bool created, HeartbeatStatus status) = Get(heartbeat?.InstanceId);
            bool hasInfo = status.Info != null;
            var response = status.OnHeartbeat(heartbeat);

            if (response.infoRequested)
            {
                CreateConfigFromServer(response, status);
                if (created)
                {
                    Log(new HeartbeatTrackerLogItem(LogLevel.Debug, "Incoming heartbeat", heartbeat.InstanceId, true));
                }
            }
            
            return response;
        }
    }
}
