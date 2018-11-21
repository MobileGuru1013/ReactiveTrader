﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Adaptive.ReactiveTrader.Shared.UI;

namespace Adaptive.ReactiveTrader.Server
{
    public interface IMainViewModel : IViewModel
    {
        ICommand StartStopCommand { get; }
        string ServerStatus { get; }
        string StartStopCommandText { get; }
        string Throughput { get; }
        string DesiredThroughput { get; }
        int UpdateFrequency { get; set; }
        ObservableCollection<ICurrencyPairViewModel> CurrencyPairs { get; }
        bool WindowAlwaysOnTop { get; set; }
        void Start();
    }
}