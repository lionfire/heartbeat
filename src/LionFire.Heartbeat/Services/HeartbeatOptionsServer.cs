namespace LionFire.Heartbeat.Options
{
    public class HeartbeatOptionsServer
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
        public HeartbeatConfigFromServer Config { get; internal set; }
    }
}
