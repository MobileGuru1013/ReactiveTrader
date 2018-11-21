using System.Collections.Generic;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    public class CurrencyPairTracker
    {
        private readonly string _currencyPair;
        private readonly string _crossedPair;
        private decimal _baseTradedAmount;
        private decimal _counterTradedAmount;
        private decimal _baseSpot;
        private int _tradeCount;
        private CurrencyPairPositionReport _currentPosition = new CurrencyPairPositionReport();

        public CurrencyPairTracker(string currencyPair)
        {
            _currencyPair = currencyPair;
            _crossedPair = currencyPair.Substring(0, 3) + "USD";
            
        }

        public string CurrencyPair
        {
            get { return _currencyPair; }
        }

        public string CrossedPair
        {
            get { return _crossedPair; }
        }

        public int TradeCount
        {
            get { return _tradeCount; }
        }

        public CurrencyPairPositionReport CurrentPosition
        {
            get { return _currentPosition; }
        }

        public void OnTrade(TradeDto trade, IDictionary<string, PriceDto> priceCache)
        {
            if (trade.Status != TradeStatusDto.Done)
                return;
            
            if (trade.Direction == DirectionDto.Buy)
            {
                _baseTradedAmount += trade.Notional;
                _counterTradedAmount += (trade.Notional*trade.SpotRate);
            }
            else
            {
                _baseTradedAmount -= trade.Notional;
                _counterTradedAmount -= (trade.Notional * trade.SpotRate);
            }
            _tradeCount++;

            OnPrice(priceCache, true);
        }

        public void OnPrice(IDictionary<string, PriceDto> priceCache, bool wasTraded)
        {
            var isLong = _baseTradedAmount >= 0;
            var isUsdBased = CurrencyPair.StartsWith("USD");

            PriceDto monitoredPrice, crossedPrice = null;
            if (!priceCache.TryGetValue(CurrencyPair, out monitoredPrice)
                || (!isUsdBased && !priceCache.TryGetValue(CrossedPair, out crossedPrice)))
            {
                return;
            }

            _baseSpot = isLong
                ? _counterTradedAmount/monitoredPrice.Bid
                : _counterTradedAmount/monitoredPrice.Ask;

            var basePnl = _baseTradedAmount - _baseSpot;

            decimal usdPnl;
            if (isUsdBased)
            {
                usdPnl = basePnl;
            }
            else
            {
                usdPnl = isLong
                    ? basePnl * crossedPrice.Bid
                    : basePnl * crossedPrice.Ask;
            }
            
            _currentPosition = new CurrencyPairPositionReport
            {
                Symbol = CurrencyPair,
                BaseTradedAmount = _baseTradedAmount,
                BasePnl = basePnl,
                UsdPnl = usdPnl,
                WasTraded = wasTraded
            };
        }
    }
}