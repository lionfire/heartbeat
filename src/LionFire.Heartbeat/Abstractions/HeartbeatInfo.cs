namespace LionFire.Heartbeat
{
    public class HeartbeatInfo
    {
        public string ProgramName { get; set; }
        public string HostName { get; set; }
        public string InstanceName { get; set; }

        /// <summary>
        /// A GUID representing the process.
        /// </summary>
        public string InstanceId { get; set; }

        public int HeartbeatIntervalInSeconds { get; set; }
        
        public string HealthCheckUri { get; set; }

        public bool SupportsHealthCheck { get; set; }
    }
}
