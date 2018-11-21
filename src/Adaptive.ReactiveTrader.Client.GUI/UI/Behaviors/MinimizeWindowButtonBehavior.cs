using System.Windows;

namespace Adaptive.ReactiveTrader.Client.UI.Behaviors
{
    public class MinimizeWindowButtonBehavior : WindowButtonBehaviorBase
    {
        protected override void OnButtonClicked()
        {
            AssociatedWindow.WindowState = WindowState.Minimized;
        }
    }
}
