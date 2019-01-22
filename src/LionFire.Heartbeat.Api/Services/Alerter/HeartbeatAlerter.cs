using LionFire.Heartbeat.Api;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace LionFire.Heartbeat
{
    public class HeartbeatAlerter : IHostedService
    {
        private readonly IOptionsMonitor<HeartbeatAlerterOptions> options;
        private HeartbeatTracker tracker;
        private HeartbeatLog heartbeatLog;
        private IEnumerable<IHeartbeatAlerter> alerters;

        public HeartbeatAlerter(IOptionsMonitor<HeartbeatAlerterOptions> options, HeartbeatTracker tracker, IEnumerable<IHeartbeatAlerter> alerters, HeartbeatLog heartbeatLog)
        {
            this.tracker = tracker;
            this.options = options;
            this.alerters = alerters;
            this.heartbeatLog = heartbeatLog;
            this.options.OnChange(OnOptionsChanged);
        }

        private void OnOptionsChanged(HeartbeatAlerterOptions _) => StartTimer();

        private void StartTimer()
        {
            bool created = false;
            if (timer == null)
            {
                created = true;
                timer = new Timer();
                timer.Elapsed += Timer_Elapsed;
            }
            timer.Interval = options.CurrentValue.RealertInterval * 1000;
            if (created)
            {
                timer.Enabled = true;
            }
        }

        private Timer timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartTimer();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            {
                var copy = timer;
                if (copy != null)
                {
                    timer = null;
                    copy.Enabled = false;
                }
            }

            return Task.CompletedTask;
        }

        private ConcurrentDictionary<string, HeartbeatState> LastStates = new ConcurrentDictionary<string, HeartbeatState>();
        private ConcurrentDictionary<string, HealthStatus?> LastHealth = new ConcurrentDictionary<string, HealthStatus?>();

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var notOk = tracker.Statuses.Where(s => !s.IsOk);

            foreach (var service in tracker.Statuses)
            {
                {
                    var lastState = LastStates.GetOrAdd(service.InstanceId, id => service.heartbeatState);
                    if (lastState != service.heartbeatState)
                    {
                        heartbeatLog.Log(new HeartbeatTrackerLogItem(LogLevel.Information, "State Changed", $"{service.Key} state {lastState} => {service.heartbeatState}"));
                    }
                    LastStates.TryUpdate(service.InstanceId, service.heartbeatState, lastState);
                }

                {
                    var lastHealth = LastHealth.GetOrAdd(service.InstanceId, id => service.HealthStatus);
                    if (lastHealth != service.HealthStatus)
                    {
                        heartbeatLog.Log(new HeartbeatTrackerLogItem(LogLevel.Information, "Health Changed", $"{service.Key} state {lastHealth} => {service.HealthStatus}"));
                    }
                    LastHealth.TryUpdate(service.InstanceId, service.HealthStatus, lastHealth);
                }
            }

            foreach (var service in notOk)
            {
                if (service.heartbeatState == HeartbeatState.Unspecified)
                {
                    continue; // New?
                }

                var problem = "";
                if (service.heartbeatState == HeartbeatState.Missing)
                {
                    problem += " " + service.heartbeatState.ToString();
                }

                if (service.HealthStatus != Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
                {
                    problem += " " + service.HealthStatus.ToString();
                }

                string title = $"{problem}: {service.Key}";

                heartbeatLog.Log(new HeartbeatTrackerLogItem(LogLevel.Warning, $"[ALERT - {problem}]", $"{title}"));
                //await Task.WhenAll(alerters.Select(async a => await a.Alert(title, problem, "", true)));
            }

        }
    }
}
