using System;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Shared.DTO.Control;
using Adaptive.ReactiveTrader.Shared.UI;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    class CurrencyPairViewModel : ViewModelBase, ICurrencyPairViewModel
    {
        private readonly CurrencyPairStateDto _currencyPairStateDto;
        private readonly IReactiveTrader _reactiveTrader;
        private readonly IConcurrencyService _concurrencyService;
        public string Comment { get; private set; }
        public string Symbol { get; private set; }
        public bool CanModify { get; set; }

        public CurrencyPairViewModel(CurrencyPairStateDto currencyPairStateDto, IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService)
        {
            _currencyPairStateDto = currencyPairStateDto;
            _reactiveTrader = reactiveTrader;
            _concurrencyService = concurrencyService;
            Symbol = currencyPairStateDto.Symbol;
            Available = currencyPairStateDto.Enabled;
            Stale = currencyPairStateDto.Stale;
            CanModify = true;
        }

        public bool Available
        {
            get { return _currencyPairStateDto.Enabled; }
            set
            {
                CanModify = false;
                _currencyPairStateDto.Enabled = value;

                _reactiveTrader.Control.SetCurrencyPairState(_currencyPairStateDto.Symbol, value, Stale)
                    .SubscribeOn(_concurrencyService.TaskPool)
                    .ObserveOn(_concurrencyService.Dispatcher)
                    .Subscribe(
                        _ =>
                        {
                            CanModify = true;
                        },
                        ex =>
                        {
                            CanModify = true;
                        },
                    () => { });
            }
        }

        public bool Stale
        {
            get { return _currencyPairStateDto.Stale; }
            set
            {
                _currencyPairStateDto.Stale = value;
                CanModify = false;

                _reactiveTrader.Control.SetCurrencyPairState(_currencyPairStateDto.Symbol, Available, value)
                    .SubscribeOn(_concurrencyService.TaskPool)
                    .ObserveOn(_concurrencyService.Dispatcher)
                    .Subscribe(
                        _ => CanModify = true,
                        ex => { CanModify = true; },
                        () => { });
            }
        }
    }
}