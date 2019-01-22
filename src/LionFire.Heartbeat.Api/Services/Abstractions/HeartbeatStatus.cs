using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Reactive.Subjects;

namespace LionFire.Heartbeat
{
    public class HeartbeatStatus
    {
        #region Identity

        public string InstanceId { get; }

        #endregion

        #region Parameters

        public HeartbeatInfo Info { get; private set; }
        public HeartbeatConfigFromServer ConfigFromServer { get; internal set; }

        #region Derived

        public bool IsExpectingHeartbeatAtIntervals => Info != null && Info.HeartbeatInterval > 0 && ConfigFromServer != null && ConfigFromServer.SendAtIntervals;
        public double HeartbeatInvervalInMilliseconds
        {
            get
            {
                if (Info == null || ConfigFromServer == null)
                {
                    return double.NaN;
                }

                var resultInSeconds = Info.HeartbeatInterval;

                resultInSeconds = Math.Min(resultInSeconds, ConfigFromServer.MaxInterval);
                resultInSeconds = Math.Max(resultInSeconds, ConfigFromServer.MinInterval);

                return resultInSeconds * 1000;
            }
        }

        #endregion

        #endregion

        #region Construction

        public HeartbeatStatus(string id)
        {
            InstanceId = id;
        }

        #endregion

        #region State

        public DateTime NextDueDate { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
        public int SeenCount { get; set; }

        public string CurrentHeartbeatState => heartbeatState.ToString();
        internal HeartbeatState heartbeatState
        {
            get
            {
                if (LastSeen == default(DateTime) || double.IsNaN(HeartbeatInvervalInMilliseconds))
                {
                    return HeartbeatState.Unspecified;
                }

                if (DateTime.UtcNow > LastSeen + (TimeSpan.FromMilliseconds(3 * HeartbeatInvervalInMilliseconds)))
                {
                    return HeartbeatState.Missing;
                }

                if (DateTime.UtcNow > LastSeen + TimeSpan.FromMilliseconds(HeartbeatInvervalInMilliseconds))
                {
                    return HeartbeatState.Late;
                }

                return HeartbeatState.OnTime;
            }
        }

        private readonly object HeartbeatLock = new object();
        public BehaviorSubject<Heartbeat> Heartbeat { get; private set; }

        #endregion

        #region (Public) Methods

        public HeartbeatResponse OnHeartbeat(Heartbeat heartbeat)
        {
            lock (HeartbeatLock)
            {
                if (Heartbeat == null)
                {
                    Heartbeat = new BehaviorSubject<Heartbeat>(heartbeat);
                }
                else
                {
                    Heartbeat.OnNext(heartbeat);
                }
            }
            OnSeen();
            return new HeartbeatResponse
            {
                infoRequested = Info == null
            };
        }

        public void OnInfo(HeartbeatInfo info)
        {
            Info = info;
            OnSeen();
        }

        #endregion


        private void OnSeen()
        {
            SeenCount++;
            if (FirstSeen == default(DateTime))
            {
                FirstSeen = DateTime.UtcNow;
            }

            LastSeen = DateTime.UtcNow;

            if (Info == null || !IsExpectingHeartbeatAtIntervals)
            {
                NextDueDate = default(DateTime);
            }
            else
            {
                NextDueDate = LastSeen + TimeSpan.FromSeconds(HeartbeatInvervalInMilliseconds);
            }

        }

        #region HealthChecks

        public BehaviorSubject<HealthReport> HealthReports { get; private set; }

        public HealthStatus HealthStatus => HealthReports?.Value?.Status ?? Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy;

        public bool IsOk => heartbeatState == HeartbeatState.OnTime && (HealthStatus == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);

        public void OnHealthReport(HealthReport report)
        {
            if (HealthReports == null)
            {
                HealthReports = new BehaviorSubject<HealthReport>(report);
            }
            else
            {
                HealthReports.OnNext(report);
            }
            OnSeen();
        }

        #endregion

        public string Key
        {
            get
            {
                if (Info == null)
                {
                    return $"{{{InstanceId}}}";
                }
                var key = $"{Info.HostName}";
                if(Info.ProgramName != null)
                {
                    key = Info.ProgramName + "@" + key;
                }
                if(Info.InstanceName != null)
                {
                    key += "#" + Info.InstanceName;
                }
                return key;
            }
        }
    }
}
