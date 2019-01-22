using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LionFire.Heartbeat
{
    public class HeartbeatTrackerLogItem
    {
        public DateTime Date { get; set; }
        public EventId EventId { get; set; }
        public int TypeId => EventId.Id;
        public string Type => EventId.Name;
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }

        public HeartbeatTrackerLogItem() { Date = DateTime.UtcNow; }
        public HeartbeatTrackerLogItem(LogLevel logLevel, string type, string message, bool? success = null, int eventId = 0) :this()
        {
            this.LogLevel = logLevel;
            this.EventId = new EventId(eventId, type);
            this.Message = message;
            this.Success = success;
        }

        public Dictionary<string, object> Data { get; set; }

        public bool DidUndo { get; set; }
        public bool? Success { get; set; }
    }

    public class HeartbeatLog
    {
        #region State

        public List<HeartbeatTrackerLogItem> LogItems { get; } = new List<HeartbeatTrackerLogItem>();

        #endregion

        public void Log(HeartbeatTrackerLogItem logItem)
        {
            LogItems.Add(logItem);
        }
    }
}
