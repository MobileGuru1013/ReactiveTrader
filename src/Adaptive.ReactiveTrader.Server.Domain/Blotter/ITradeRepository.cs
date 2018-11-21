using System.Collections.Generic;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public interface ITradeRepository
    {
        void Reset();
        void StoreTrade(TradeDto trade);
        IList<TradeDto> GetAllTrades();

    }
}