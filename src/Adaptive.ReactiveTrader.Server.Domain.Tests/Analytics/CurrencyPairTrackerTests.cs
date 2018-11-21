using System.Collections.Generic;
using Adaptive.ReactiveTrader.Server.Analytics;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;
using NUnit.Framework;

namespace Adaptive.ReactiveTrader.Server.Domain.Tests.Analytics
{
    [TestFixture]
    public class CurrencyPairTrackerTests
    {
        private CurrencyPairTracker _target;

        [Test]
        public void Doesnt_check_for_cross_when_monitoring_usd_base()
        {
            // arrange
            _target = new CurrencyPairTracker("USDZZZ");

            var prices = new Dictionary<string, PriceDto>
            {
                {"USDZZZ", new PriceDto() {Symbol = "USDZZZ", Bid = 1, Ask = 1}}
            };
            
            // act
            _target.OnPrice(prices, true);
            
            // assert
            
        }
        [Test]
        public void Does_check_for_cross_when_monitoring_non_usd_base()
        {
            // arrange
            _target = new CurrencyPairTracker("EURZZZ");

            var prices = new Dictionary<string, PriceDto>
            {
                {"EURZZZ", new PriceDto() {Symbol = "EURZZZ", Bid = 1, Ask = 1}},
                {"EURUSD", new PriceDto() {Symbol = "EURUSD", Bid = 1, Ask = 1}}

            };
            
            // act
            _target.OnPrice(prices, true);
            
            // assert
            
        }
    }
}