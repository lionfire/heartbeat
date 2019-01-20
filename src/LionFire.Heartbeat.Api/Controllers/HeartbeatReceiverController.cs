using LionFire.Heartbeat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;

namespace LionFire.Heartbeat
{
    [Route("api/heartbeat")]
    [ApiController]
    public class HeartbeatReceiverController : ControllerBase
    {
        private HeartbeatTracker tracker;

        public HeartbeatReceiverController(HeartbeatTracker tracker)
        {
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
                var response = new HeartbeatResponse();
                response.infoRequested = tracker.OnHeartbeat(heartbeat).Info == null;
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
