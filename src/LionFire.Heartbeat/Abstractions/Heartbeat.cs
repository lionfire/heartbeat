using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace LionFire.Heartbeat
{
    public class Heartbeat
    {        
        /// <summary>
        /// A GUID representing the process.
        /// </summary>
        public string InstanceId { get; set; }

        public HealthReport HealthReport { get; set; }
        
        //public HealthCheckResult HealthCheckResult { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public string Action { get; internal set; }
    }
}
