using System.Configuration;
using Adaptive.ReactiveTrader.Shared;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Adaptive.ReactiveTrader.Server.Control
{
    public class ControlAuth : AuthorizeAttribute
    {
        private static readonly string AuthToken = ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey];

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                return false;

            return hubIncomingInvokerContext.Hub.Context.Headers[AuthTokenProvider.AuthTokenKey] == AuthToken;
        }
    }
}