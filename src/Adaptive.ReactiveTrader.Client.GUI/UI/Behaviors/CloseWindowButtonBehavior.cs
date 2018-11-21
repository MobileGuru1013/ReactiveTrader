namespace Adaptive.ReactiveTrader.Client.UI.Behaviors
{
    public class CloseWindowButtonBehavior : WindowButtonBehaviorBase
    {
        protected override void OnButtonClicked()
        {
            AssociatedWindow.Close();
        }
    }
}
