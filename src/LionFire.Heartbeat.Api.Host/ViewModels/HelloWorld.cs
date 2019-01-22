using System;
using DotNetify;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace LionFire.Heartbeat
{
    public class HelloWorld : BaseVM
    {
        #region Dependencies

        HeartbeatTracker tracker;
        HeartbeatLog heartbeatLog;

        #region Convenience


        #endregion

        #endregion


        #region View Model

        public string Greetings => "Hello World!";
        public DateTime ServerTime => DateTime.Now;

        public int HealthyHeartbeatCount => tracker.Statuses.Where(s => s.IsOk).Count();
        public IEnumerable<HeartbeatTrackerLogItem> LogItems => heartbeatLog.LogItems;

        #endregion

        #region State

        private Timer _timer;

        #endregion

        public HelloWorld(HeartbeatTracker tracker, HeartbeatLog heartbeatLog)
        {
            this.tracker = tracker;
            this.heartbeatLog = heartbeatLog;

            _timer = new Timer(state =>
            {
                Changed(nameof(ServerTime));
                Changed(nameof(HealthyHeartbeatCount));
                Changed(nameof(LogItems));
                PushUpdates();
            }, null, 0, 1000);
        }

        public override void Dispose() => _timer.Dispose();
    }
}