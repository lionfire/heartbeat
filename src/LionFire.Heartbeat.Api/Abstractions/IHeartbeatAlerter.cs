using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Heartbeat.Api
{
    public interface IHeartbeatAlerter
    {
        // TEMP interface just to get started:
        Task<bool> Alert(string title, string message, string detail, bool urgent);
    }
}
