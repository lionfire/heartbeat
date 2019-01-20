using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Heartbeat.Api.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class HeartbeatsController : ControllerBase
    {
        private readonly HeartbeatTracker tracker;

        public HeartbeatsController(HeartbeatTracker tracker)
        {
            this.tracker = tracker;
        }

        //[HttpGet()]
        //public HeartbeatTrackerSummary

        [HttpGet]
        public IEnumerable<HeartbeatStatus> Heartbeats() => tracker.Statuses;
    }

    //public class HeartbeatTrackerSummary
    //{
    //}
    //public class HeartbeatSummary
    //{
    //    public string InstanceId { get; set; }
    //    public string ProgramName { get; set; }
    //}
}
