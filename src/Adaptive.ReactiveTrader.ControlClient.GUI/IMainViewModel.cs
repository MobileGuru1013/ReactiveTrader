using System.Collections.ObjectModel;
using System.Windows.Input;
using Adaptive.ReactiveTrader.Shared.UI;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    public interface IMainViewModel : IViewModel
    {
        ICommand RefreshCommand { get; }
        string ServerStatus { get; }
        string DesiredThroughput { get; }
        ObservableCollection<ICurrencyPairViewModel> CurrencyPairs { get; }
        bool WindowAlwaysOnTop { get; set; }
        void Start();
    }
}