using System.Collections.Generic;
using System.Linq;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;

namespace Adaptive.ReactiveTrader.Server.Blotter
{
    public class TradeRepository : ITradeRepository
    {
        private readonly Queue<TradeDto> _trades = new Queue<TradeDto>();

        public void Reset()
        {
            lock (_trades)
            {
                _trades.Clear();
            }
        }

        public void StoreTrade(TradeDto trade)
        {
            lock (_trades)
            {
                _trades.Enqueue(trade);
            }
        }

        public IList<TradeDto> GetAllTrades()
        {
            IList<TradeDto> trades;

            lock (_trades)
            {
                trades = _trades.ToList();
            }

            return trades;
        } 
    }
}