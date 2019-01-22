using LionFire.Heartbeat.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LionFire.Heartbeat
{
    //public class HeartbeatServer
    //{
    //    public HeartbeatConfigFromServer ConfigFromServer { get; set; }
    //}

    public class HeartbeatSender : IHostedService
    {
        /// <summary>
        /// In seconds
        /// </summary>
        public static double DefaultInterval { get; set; } = 60;

        #region Identity

        private string InstanceId;

        #endregion

        #region Dependencies

        private readonly IOptionsMonitor<HeartbeatOptions> options;
        private readonly ILogger logger;

        #endregion

        #region State

        private Timer timer;

        #endregion

        #region Construction

        public HeartbeatSender(IOptionsMonitor<HeartbeatOptions> options, ILogger<HeartbeatSender> logger)
        {
            InstanceId = Guid.NewGuid().ToString();

            this.logger = logger;
            this.options = options;

            options.OnChange((o, _) => timer.Interval = options.CurrentValue.Interval);
        }

        #endregion

        public double IntervalInMilliseconds => Interval * 1000;
        public double Interval
        {
            get
            {
                var configValue = options.CurrentValue.Interval;
                if (configValue == 0)
                {
                    return DefaultInterval;
                }
                else if (configValue < 0)
                {
                    return double.NaN;
                }
                else
                {
                    return configValue;
                }
            }
        }

        private void StopTimer()
        {
            var copy = timer;
            if (copy != null)
            {
                timer = null;
                copy.Enabled = false;
                copy.Dispose();
            }
        }

        private void UpdateTimer()
        {
            var interval = IntervalInMilliseconds;
            if (double.IsNaN(interval))
            {
                StopTimer();
            }
            else
            {
                timer = new Timer(interval);
                timer.Elapsed += Timer_Elapsed;
                timer.Enabled = true;
                logger.LogInformation($"Sending heartbeat every {Interval} seconds");
            }
        }

        #region Start / Stop (IHostedService)

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            UpdateTimer();

            // TODO: Send HeartbeatInfo, and receive config from server
            await SendHeartbeat(cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            StopTimer();

            await SendHeartbeat(h => h.Action = HeartbeatActions.Goodbye, cancellationToken);
        }

        #endregion

        #region Event Handler

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e) => await SendHeartbeat();

        #endregion

        #region Public Methods

        public async Task SendInfo(HeartbeatOptionsServer server, CancellationToken cancellationToken = default(CancellationToken))
        {
            var o = options.CurrentValue;

            var http = new HttpClient()
            {
                BaseAddress = new Uri(server.Url),
            };

            var heartbeatInfo = new HeartbeatInfo()
            {
                InstanceId = InstanceId,
                ProgramName = o.ProgramName ??  HeartbeatOptions.DefaultProgramName,
                HealthCheckUri = o.HealthCheckUri,
                HeartbeatInterval = Interval,
                HostName = o.HostName ?? Environment.MachineName,
                InstanceName = o.InstanceName,
                //SupportsHealthCheck // TODO
            };

            var json = SimpleJson.SimpleJson.SerializeObject(heartbeatInfo);
            var msg = new HttpRequestMessage(HttpMethod.Post, "api/heartbeat/info")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var apiKey = server.ApiKey ?? o.ApiKey;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);
            }

            HttpResponseMessage response = null;
            try
            {
                response = await http.SendAsync(msg, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send heartbeat info to " + server.Url);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var heartbeatConfig = SimpleJson.SimpleJson.DeserializeObject<HeartbeatConfigFromServer>(responseString);
            if (heartbeatConfig != null)
            {
                server.Config = heartbeatConfig;
            }

        }

        public async Task SendHeartbeat(Action<Heartbeat> heartbeatConfigurer = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var o = options.CurrentValue;

            await Task.WhenAll(o.Servers.Select(async server =>
            {
                var http = new HttpClient()
                {
                    BaseAddress = new Uri(server.Url),
                };

                var heartbeat = new Heartbeat()
                {
                    InstanceId = InstanceId,
                };

                heartbeatConfigurer?.Invoke(heartbeat);

                var json = SimpleJson.SimpleJson.SerializeObject(heartbeat);
                var msg = new HttpRequestMessage(HttpMethod.Post, "api/heartbeat")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var apiKey = server.ApiKey ?? o.ApiKey;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", apiKey);
                }

                try
                {
                    var response = await http.SendAsync(msg, cancellationToken);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var heartbeatResult = SimpleJson.SimpleJson.DeserializeObject<HeartbeatResponse>(responseString);
                    if (heartbeatResult.infoRequested)
                    {
                        await SendInfo(server, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send heartbeat to " + server);
                }
            }));
        }

        public async Task SendGoodbye(CancellationToken cancellationToken = default(CancellationToken)) => await SendHeartbeat(h => h.Action = HeartbeatActions.Goodbye);

        #endregion
    }
}
