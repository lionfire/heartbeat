using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.Heartbeat.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class HeartbeatsController : ControllerBase, IAdminApi
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

        [HttpGet("ok")]
        public IEnumerable<HeartbeatStatus> IsOk() => tracker.Statuses.Where(s=> s.IsOk);

        [HttpGet("not-ok")]
        public IEnumerable<HeartbeatStatus> NotOk() => tracker.Statuses.Where(s => !s.IsOk);
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
