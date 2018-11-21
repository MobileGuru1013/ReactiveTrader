using Adaptive.ReactiveTrader.Server;
using Adaptive.ReactiveTrader.Server.Analytics;
using Adaptive.ReactiveTrader.Server.Blotter;
using Adaptive.ReactiveTrader.Server.Control;
using Adaptive.ReactiveTrader.Server.Execution;
using Adaptive.ReactiveTrader.Server.Pricing;
using Adaptive.ReactiveTrader.Server.ReferenceData;
using Adaptive.ReactiveTrader.Server.Transport;
using Autofac;

namespace Adaptive.ReactiveTrader.Web
{
    public class Bootstrapper
    {
        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            // analytics
            builder.RegisterType<AnalyticsPublisher>().As<IAnalyticsPublisher>().SingleInstance();
            builder.RegisterType<AnalyticsService>().As<IAnalyticsService>().SingleInstance();
            builder.RegisterType<AnalyticsHub>().SingleInstance();

            // pricing
            builder.RegisterType<PricePublisher>().As<IPricePublisher>().SingleInstance();
            builder.RegisterType<PriceFeedSimulator>().As<IPriceFeed>().SingleInstance();
            builder.RegisterType<PriceLastValueCache>().As<IPriceLastValueCache>().SingleInstance();
            builder.RegisterType<PricingHub>().SingleInstance();

            // reference data
            builder.RegisterType<CurrencyPairRepository>().As<ICurrencyPairRepository>().SingleInstance();
            builder.RegisterType<CurrencyPairUpdatePublisher>().As<ICurrencyPairUpdatePublisher>().SingleInstance();
            builder.RegisterType<ReferenceDataHub>().SingleInstance();            

            // execution            
            builder.RegisterType<ExecutionService>().As<IExecutionService>().SingleInstance();
            builder.RegisterType<ExecutionHub>().SingleInstance();

            // control
            builder.RegisterType<ControlHub>().SingleInstance();

            // blotter
            builder.RegisterType<BlotterPublisher>().As<IBlotterPublisher>().SingleInstance();
            builder.RegisterType<TradeRepository>().As<ITradeRepository>().SingleInstance();
            builder.RegisterType<BlotterHub>().SingleInstance();            
            
            builder.RegisterType<ContextHolder>().As<IContextHolder>().SingleInstance();
            builder.RegisterType<SchedulerService>().As<ISchedulerService>().SingleInstance();
            builder.RegisterType<Cleaner>().SingleInstance();
            
            return builder.Build();
        }
    }
}