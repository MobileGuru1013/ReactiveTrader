using Adaptive.ReactiveTrader.Server;
using Adaptive.ReactiveTrader.Server.Pricing;
using Adaptive.ReactiveTrader.Server.Transport;
using Adaptive.ReactiveTrader.Web;
using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Adaptive.ReactiveTrader.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //log4net.Config.XmlConfigurator.Configure();

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Build();
            var priceFeed = container.Resolve<IPriceFeed>();
            priceFeed.Start();
            var cleaner = container.Resolve<Cleaner>();
            cleaner.Start();
            
            app.UseCors(CorsOptions.AllowAll);
            app.Map("/signalr", map =>
            {
                var hubConfiguration = new HubConfiguration
                {
                    // you don't want to use that in prod, just when debugging
                    EnableDetailedErrors = true,
                    EnableJSONP = true,
                    Resolver = new AutofacSignalRDependencyResolver(container)
                };


                map.UseCors(CorsOptions.AllowAll)
                    .RunSignalR(hubConfiguration);
            });
        }
    }
}