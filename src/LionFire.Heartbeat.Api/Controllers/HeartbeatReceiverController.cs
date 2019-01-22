using LionFire.Heartbeat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace LionFire.Heartbeat
{
    [Route("api/heartbeat")]
    [ApiController]
    public class HeartbeatReceiverController : ControllerBase
    {
        private HeartbeatTracker tracker;
        private IOptionsMonitor<HeartbeatTrackerOptions> trackerOptions;

        public HeartbeatReceiverController(IOptionsMonitor<HeartbeatTrackerOptions> trackerOptions, HeartbeatTracker tracker)
        {
            this.trackerOptions = trackerOptions;
            this.tracker = tracker;
        }

        [HttpPost("Info")]
        public HeartbeatConfigFromServer Info(HeartbeatInfo info)
        {
            tracker.OnInfo(info);
            return new HeartbeatConfigFromServer
            {
                SendAtIntervals = true,
                SendChanges = true,
            };
        }

        [HttpPost]
        [Route("")]
        public IActionResult Heartbeat(Heartbeat heartbeat)
        {
            try
            {
                
                var response = tracker.OnHeartbeat(heartbeat);
                if (response.infoRequested)
                {
                    response.config = new HeartbeatConfigFromServer
                    {
                        SendAtIntervals = true,
                        MinInterval = trackerOptions.CurrentValue.MinHeartbeatInterval,
                    };
                }
                return Ok(response);
            }
            catch(ArgumentNullException)
            {
                return BadRequest();
            }
        }

        //[HttpPost]
        //public void HealthCheck(HealthCheckResult healthCheck) => tracker.HealthCheck(healthCheck);
    }
}
