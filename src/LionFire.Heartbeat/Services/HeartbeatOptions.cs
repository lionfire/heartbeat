using System.Collections.Generic;

namespace LionFire.Heartbeat.Options
{

    public class HeartbeatOptions
    {
        /// <summary>
        /// (Optional) Name of service.  If not provided, will use the FullName of the entry assembly.
        /// </summary>
        public string ProgramName { get; set; }

        public List<HeartbeatOptionsServer> Servers { get; set; }

        public string ApiKey { get; set; }

        /// <summary>
        /// Interval in seconds
        /// </summary>
        public double Interval { get; set; } = 15.0;

        #region (Static)

        public static string DefaultProgramName
        {
            get
            {
                if (defaultProgramName == null)
                {
                    var n = System.Reflection.Assembly.GetEntryAssembly().FullName;
                    n = n.Split(new char[] { ',' }, 2)[0];

                    defaultProgramName = n;
                }
                return defaultProgramName;
            }
        }

        public string HealthCheckUri { get; internal set; }
        public int HeartbeatIntervalInSeconds { get; internal set; }
        public string HostName { get; internal set; }
        public string InstanceName { get; internal set; }

        private static string defaultProgramName;

        #endregion
    }
}
