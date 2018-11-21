using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Control;
using Adaptive.ReactiveTrader.Shared.Extensions;
using Adaptive.ReactiveTrader.Shared.UI;
using log4net;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    internal class MainViewModel : ViewModelBase, IMainViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MainWindow));

        private readonly IReactiveTrader _reactiveTrader;
        private readonly Func<CurrencyPairStateDto, ICurrencyPairViewModel> _ccyViewModelFactory;
        private readonly IUserProvider _userProvider;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IConcurrencyService _concurrencyService;

        public ICommand RefreshCommand { get; private set; }
        public string ServerStatus { get; private set; }
        public string DesiredThroughput { get; set; }
        public bool WindowAlwaysOnTop { get; set; }

        public ObservableCollection<ICurrencyPairViewModel> CurrencyPairs { get; private set; } 

        public MainViewModel(IReactiveTrader reactiveTrader, 
            Func<CurrencyPairStateDto, ICurrencyPairViewModel> ccyViewModelFactory,
            IUserProvider userProvider,
            IConfigurationProvider configurationProvider,
            IConcurrencyService concurrencyService)
        {
            _reactiveTrader = reactiveTrader;
            _ccyViewModelFactory = ccyViewModelFactory;
            _userProvider = userProvider;
            _configurationProvider = configurationProvider;
            _concurrencyService = concurrencyService;

            WindowAlwaysOnTop = false;

            RefreshCommand = new DelegateCommand(RefreshData);
            CurrencyPairs = new ObservableCollection<ICurrencyPairViewModel>();

            ObserveThroughputs();
        }

        public void Start()
        {
            ServerStatus = "Connecting...";

            _reactiveTrader.Initialize(_userProvider.Username, _configurationProvider.Servers, null, ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey]);


            _reactiveTrader.ConnectionStatusStream
                .SubscribeOn(_concurrencyService.TaskPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Subscribe(ci =>
            {
                ServerStatus = ci.ConnectionStatus.ToString();
            });

            _reactiveTrader.ConnectionStatusStream
                .SubscribeOn(_concurrencyService.TaskPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Where(ci => ci.ConnectionStatus == ConnectionStatus.Connected)
                .Subscribe(_ => RetrieveState());
        }

        private void RetrieveState()
        {
            _reactiveTrader.Control.GetCurrencyPairStates()
                .SubscribeOn(_concurrencyService.TaskPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Subscribe(states =>
            {
                foreach (var cpi in states)
                {
                    CurrencyPairs.Add(_ccyViewModelFactory(cpi));
                }
            });

            _reactiveTrader.Control.GetPriceFeedThroughput()
                .SubscribeOn(_concurrencyService.TaskPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Subscribe(throughput =>
                {
                    DesiredThroughput = throughput.ToString();
                });
        }


        private void RefreshData()
        {
            CurrencyPairs.Clear();
            RetrieveState();
        }

        private void ObserveThroughputs()
        {
            this.ObserveProperty(p => p.DesiredThroughput)
                .Subscribe(desiredThroughput =>
                {
                    int value;
                    if (int.TryParse(desiredThroughput, out value))
                    {
                        _reactiveTrader.Control.SetPriceFeedThroughput(value)
                            .SubscribeOn(_concurrencyService.TaskPool)
                            .ObserveOn(_concurrencyService.Dispatcher)
                            .Subscribe(_ =>
                            {
                                
                            },
                            ex => { },
                            () => {});

                    }
                });

        }

    }
}