namespace LionFire.Heartbeat
{
    public class HeartbeatConfigFromServer
    {
        /// <summary>
        /// If false, don't send Heartbeats to the server.  If true, send Heartbeats to the server at regular intervals
        /// </summary>
        public bool SendAtIntervals { get; set; }

        /// <summary>
        /// If true, send a Heartbeat as soon as a health status changes
        /// </summary>
        public bool SendChanges { get; set; }

        /// <summary>
        /// If regularly sending heartbeats, do not send any less frequently than this number of seconds. (Does not affect change detection.)
        /// </summary>
        public double MaxInterval { get; set; }

        /// <summary>
        /// If regularly sending heartbeats, do not send any more frequently than this number of seconds. (Does not affect change detection.)
        /// </summary>
        public double MinInterval { get; set; }

    }
}
