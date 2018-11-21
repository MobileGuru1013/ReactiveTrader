using Adaptive.ReactiveTrader.Shared;
using Microsoft.AspNet.SignalR.Hubs;

namespace Adaptive.ReactiveTrader.Server
{
    public static class ContextUtil
    {
        public static string GetUserName(HubCallerContext context)
        {
            var userName = context.Headers[ServiceConstants.Server.UsernameHeader];
            if (string.IsNullOrEmpty(userName))
            {
                return context.QueryString[ServiceConstants.Server.UsernameHeader];
            }

            return userName;
        }
    }
}