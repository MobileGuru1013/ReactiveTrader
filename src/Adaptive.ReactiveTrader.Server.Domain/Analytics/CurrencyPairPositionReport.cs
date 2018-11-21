using System;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    public class CurrencyPairPositionReport
    {
        public string Symbol { get; set; }
        public decimal BaseTradedAmount { get; set; }
        public decimal BasePnl { get; set; }
        public decimal UsdPnl { get; set; }
        public bool WasTraded { get; set; }

        public override string ToString()
        {
            return string.Format("Symbol: {0}, BaseTradedAmount: {1}, BasePnl: {2}, UsdPnl: {3}, WasTraded: {4}", Symbol, BaseTradedAmount, BasePnl, UsdPnl, WasTraded);
        }
    }
}