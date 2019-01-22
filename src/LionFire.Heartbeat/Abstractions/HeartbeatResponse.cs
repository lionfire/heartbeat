namespace LionFire.Heartbeat
{
    public class HeartbeatResponse
    {
        public HeartbeatConfigFromServer config;

        public bool infoRequested { get; set; } // TEMP camelCase until deserializer can be configured to deal with it
    }
}
