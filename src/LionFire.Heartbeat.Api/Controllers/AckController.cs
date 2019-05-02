using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.Heartbeat.Api.Controllers
{
    [ApiController]
    [Route("{controller}")]
    public class AckController : ControllerBase, IPublicApi
    {
        HeartbeatTracker tracker;
        public AckController(HeartbeatTracker tracker)
        {
            this.tracker = tracker;
        }

        [HttpGet]
        [Route("missing/{instanceId}")]
        public IActionResult AckMissing(string instanceId) => tracker.AckMissing(instanceId) ? (IActionResult) Ok() : NotFound($"InstanceId {instanceId} is unknown or already acked");
    }
}
