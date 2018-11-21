using System;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Server.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Analytics;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    public class AnalyticsPublisher : IAnalyticsPublisher
    {
        private readonly IContextHolder _contextHolder;

        public AnalyticsPublisher(IContextHolder contextHolder)
        {
            _contextHolder = contextHolder;
        }

        public async Task Publish(PositionUpdatesDto positionUpdatesDto)
        {
            var context = _contextHolder.AnalyticsHubClients;
            if (context == null)
                return;

            try
            {
                await context.Group(ServiceConstants.Server.AnalyticsGroup).OnNewAnalytics(positionUpdatesDto);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}