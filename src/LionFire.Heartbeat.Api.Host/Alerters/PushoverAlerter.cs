// Nuget package: NPushover

using LionFire.Heartbeat.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPushover;
using NPushover.RequestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pushover = NPushover.Pushover;

namespace LionFire.Monitoring.Heartbeat.Alerters
{
    public class PushoverHeartbeatAlerter : IHeartbeatAlerter
    {
        private readonly ILogger logger;
        private readonly IOptionsMonitor<PushoverAlerterOptions> options;

        public PushoverHeartbeatAlerter(IOptionsMonitor<PushoverAlerterOptions> options, ILogger<PushoverHeartbeatAlerter> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async Task<bool> Alert(string title, string message, string detail, bool urgent) 
        {
            var po = new NPushover.Pushover(options.CurrentValue.ApiKey);

            var msg = Message.Create(urgent ? Priority.High : Priority.Normal, title, message, false, Sounds.Spacealarm);
            //msg.SupplementaryUrl = $"http://localhost:7777/api/ack/missing/{Guid}";
            var response = await po.SendMessageAsync(msg, options.CurrentValue.Recipient);

            if (response.IsOk)
            {
                logger.LogInformation($"[pushover] Sent to {options.CurrentValue.Recipient}");
                return true;
            }
            else
            {
                logger.LogInformation($"[PUSHOVER] Fail " + response.Errors?.FirstOrDefault());
                return false;
            }
        }

    }
}

