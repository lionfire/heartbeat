using System;

namespace LionFire.Heartbeat
{
    public class UndoAckMissing : LionFireCommand
    {
        HeartbeatTracker tracker;
        public UndoAckMissing(HeartbeatTracker tracker)
        {
            this.tracker = tracker;
        }

        public override bool CanExecute(object target, object context)
        {
            if (!(target is HeartbeatTrackerLogItem item) || item.TypeId != 1) return false;

            var status = (HeartbeatStatus)item.Data["Status"];
            tracker.tracking.TryAdd(status.InstanceId, status);
            tracker.AckedMissing.Remove(status);

            return true;
        }
        public override void Execute(object target, object context)
        {
            if (!CanExecute(target, context)) throw new InvalidOperationException();

            var item = (HeartbeatTrackerLogItem) target;

            var status = (HeartbeatStatus)item.Data["Status"];
            tracker.tracking.TryAdd(status.InstanceId, status);
            tracker.AckedMissing.Remove(status);
        }
    }
}
