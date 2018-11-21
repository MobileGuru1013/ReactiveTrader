using System;
using System.Reactive.Disposables;
using Adaptive.ReactiveTrader.Server.ReferenceData;
using Adaptive.ReactiveTrader.Shared.Extensions;
using Adaptive.ReactiveTrader.Shared.UI;

namespace Adaptive.ReactiveTrader.Server
{
    class CurrencyPairViewModel : ViewModelBase, ICurrencyPairViewModel, IDisposable
    {
        private readonly CurrencyPairInfo _currencyPairInfo;
        private readonly ICurrencyPairUpdatePublisher _currencyPairUpdatePublisher;
        private readonly SerialDisposable _subscription = new SerialDisposable();

        public string Comment { get; private set; }
        public string Symbol { get; private set; }

        public CurrencyPairViewModel(CurrencyPairInfo currencyPairInfo, ICurrencyPairUpdatePublisher currencyPairUpdatePublisher)
        {
            _currencyPairInfo = currencyPairInfo;
            _currencyPairUpdatePublisher = currencyPairUpdatePublisher;
            Symbol = currencyPairInfo.CurrencyPair.Symbol;
            Available = currencyPairInfo.Enabled;
            Stale = currencyPairInfo.Stale;
            Comment = currencyPairInfo.Comment;

            _subscription.Disposable = this.ObserveProperty(p => p.Available)
                .Subscribe(SetAvailability);
        }

        public bool Available
        {
            get { return _currencyPairInfo.Enabled; }
            set { _currencyPairInfo.Enabled = value; }
        }

        public bool Stale
        {
            get { return _currencyPairInfo.Stale; }
            set
            {
                _currencyPairInfo.Stale = value;
            }
        }

        private void SetAvailability(bool isAvailable)
        {
            if (isAvailable)
            {
                _currencyPairUpdatePublisher.AddCurrencyPair(_currencyPairInfo.CurrencyPair);
            }
            else
            {
                _currencyPairUpdatePublisher.RemoveCurrencyPair(_currencyPairInfo.CurrencyPair);
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}