using Adaptive.ReactiveTrader.Shared.UI;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    public interface ICurrencyPairViewModel : IViewModel
    {
        string Symbol { get; }
        bool Available { get; set; }
        bool Stale { get; set; }
        bool CanModify { get; set; }
        string Comment { get; }
    }
}