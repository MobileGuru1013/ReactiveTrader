using System;
using Adaptive.ReactiveTrader.Shared.DTO.Analytics;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    public interface IAnalyticsService
    {
        void Reset();
        void OnTrade(TradeDto trade);

        void OnPrice(PriceDto priceDto);
        PositionUpdatesDto CurrentPositionUpdatesDto { get; }
    }
}