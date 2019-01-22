namespace LionFire.Heartbeat
{
    public class HeartbeatTrackerOptions
    {
        public double MinHeartbeatInterval { get; set; } = 5;
        public double MaxHeartbeatInterval { get; set; } = 3600;
        //public double LateMultiplierThreshold { get; set; } = 1.1;
        //public TimeSpan LateThresholdMin { get; set; } = TimeSpan.FromSeconds(3);
        //public TimeSpan LateThresholdMax { get; set; } = TimeSpan.FromSeconds(90);
    }
}
