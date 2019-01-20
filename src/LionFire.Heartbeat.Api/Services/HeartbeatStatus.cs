using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Reactive.Subjects;

namespace LionFire.Heartbeat
{
    public class HeartbeatStatus
    {
        #region Identity

        private readonly string InstanceId;

        #endregion

        #region Parameters

        public HeartbeatInfo Info { get; private set; }
        public HeartbeatConfigFromServer ConfigFromServer { get; private set; }

        #region Derived

        public bool IsExpectingHeartbeatAtIntervals
        {
            get
            {
                return Info != null && Info.HeartbeatIntervalInSeconds > 0 && ConfigFromServer != null && ConfigFromServer.SendAtIntervals;
            }
        }
        public double HeartbeatInvervalInMilliseconds
        {
            get
            {
                if(Info == null || ConfigFromServer == null)
                {
                    return double.NaN;
                }

                var resultInSeconds = Info.HeartbeatIntervalInSeconds;

                resultInSeconds = Math.Min(resultInSeconds, ConfigFromServer.MaxIntervalInSeconds);
                resultInSeconds = Math.Max(resultInSeconds, ConfigFromServer.MinIntervalInSeconds);

                return resultInSeconds * 1000;
            }
        }

        #endregion

        #endregion

        #region Construction

        public HeartbeatStatus(string id)
        {
            this.InstanceId = id;
        }

        #endregion

        #region State

        public DateTime NextDueDate { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
        public int SeenCount { get; set; }

        public HeartbeatState HeartbeatState { get; set; }

        private object HeartbeatLock = new object();
        public BehaviorSubject<Heartbeat> Heartbeat { get; private set; }

        #endregion

        #region (Public) Methods

        public HeartbeatStatus OnHeartbeat(Heartbeat heartbeat)
        {
            lock (HeartbeatLock) {
                if (Heartbeat == null) Heartbeat = new BehaviorSubject<Heartbeat>(heartbeat);
                else Heartbeat.OnNext(heartbeat);
            }
            OnSeen();
            return this;
        }
        
        public void OnInfo(HeartbeatInfo info)
        {
            this.Info = info;
            OnSeen();
        }

        #endregion


        private void OnSeen()
        {
            SeenCount++;
            if (FirstSeen == default(DateTime)) FirstSeen = DateTime.UtcNow;

            LastSeen = DateTime.UtcNow;

            if(Info == null || !IsExpectingHeartbeatAtIntervals)
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

        public HealthStatus? HealthStatus => HealthReports?.Value?.Status;

        public void OnHealthReport(HealthReport report)
        {
            if(HealthReports == null)
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
    }
}
