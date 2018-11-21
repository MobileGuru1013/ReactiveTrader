using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Server.Blotter;
using Adaptive.ReactiveTrader.Server.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ILog = Adaptive.ReactiveTrader.Shared.Logging.ILog;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    [HubName(ServiceConstants.Server.AnalyticsHub)]
    public class AnalyticsHub : Hub
    {
        private readonly IContextHolder _contextHolder;
        private readonly IAnalyticsService _analyticsService;
        private static readonly log4net.ILog Log = LogManager.GetLogger(typeof(AnalyticsHub));

        public AnalyticsHub(IContextHolder contextHolder, IAnalyticsService analyticsService)
        {
            _contextHolder = contextHolder;
            _analyticsService = analyticsService;
        }

        [HubMethodName(ServiceConstants.Server.SubscribeAnalytics)]
        public async Task SubscribeAnalytics()
        {
            _contextHolder.AnalyticsHubClients = Clients;

            var user = ContextUtil.GetUserName(Context);
            Log.InfoFormat("Received analytics subscription from use {0}", user);

            await Groups.Add(Context.ConnectionId, ServiceConstants.Server.AnalyticsGroup);
            Log.InfoFormat("Connection {0} of user {1} added to group '{2}'", Context.ConnectionId, user, ServiceConstants.Server.AnalyticsGroup);

            var analytics = _analyticsService.CurrentPositionUpdatesDto;
            await Clients.Caller.OnNewAnalytics(analytics);

            Log.InfoFormat("Snapshot published to {0}", Context.ConnectionId);
        }

        [HubMethodName(ServiceConstants.Server.UnsubscribeAnalytics)]
        public async Task UnsubscribeAnalytics()
        {
            await Groups.Remove(Context.ConnectionId, ServiceConstants.Server.AnalyticsGroup);
        }
    }
}
