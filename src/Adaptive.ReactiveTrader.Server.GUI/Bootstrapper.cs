﻿using Adaptive.ReactiveTrader.Server.Analytics;
using Adaptive.ReactiveTrader.Server.Blotter;
using Adaptive.ReactiveTrader.Server.Control;
using Adaptive.ReactiveTrader.Server.Execution;
using Adaptive.ReactiveTrader.Server.Pricing;
using Adaptive.ReactiveTrader.Server.ReferenceData;
using Adaptive.ReactiveTrader.Server.Transport;
using Autofac;

namespace Adaptive.ReactiveTrader.Server
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
            
            // blotter
            builder.RegisterType<BlotterPublisher>().As<IBlotterPublisher>().SingleInstance();
            builder.RegisterType<TradeRepository>().As<ITradeRepository>().SingleInstance();
            builder.RegisterType<BlotterHub>().SingleInstance();            
            

            // control
            builder.RegisterType<ControlHub>().SingleInstance();

            builder.RegisterType<ContextHolder>().As<IContextHolder>().SingleInstance();

            // UI
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<MainViewModel>().As<IMainViewModel>().SingleInstance();
            builder.RegisterType<CurrencyPairViewModel>().As<ICurrencyPairViewModel>();

            return builder.Build();
        }
    }
}