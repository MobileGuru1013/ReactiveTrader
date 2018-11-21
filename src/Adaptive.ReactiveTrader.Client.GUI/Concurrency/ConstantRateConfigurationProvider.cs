using System;
using System.Configuration;

namespace Adaptive.ReactiveTrader.Client.Concurrency
{
    public sealed class ConstantRateConfigurationProvider : IConstantRateConfigurationProvider
    {
        public ConstantRateConfigurationProvider()
        {
            var timespan = ConfigurationManager.AppSettings["constantRateTimeSpan"];
            TimeSpan constantRate;
            if (string.IsNullOrEmpty(timespan) || !TimeSpan.TryParse(timespan, out constantRate))
            {
                throw new ConfigurationErrorsException("AppSettings 'constantRateTimeSpan' key is not defined, empty or does not parse.");
            }

            ConstantRate = constantRate;
        }

        public TimeSpan ConstantRate { get; private set; }
    }
}