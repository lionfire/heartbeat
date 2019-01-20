using System.Collections.Generic;
using LionFire.Heartbeat.Samples.AspNetCore.Models;

namespace LionFire.Heartbeat.Samples.AspNetCore.Providers
{
    public interface IWeatherProvider
    {
        List<WeatherForecast> GetForecasts();
    }
}
