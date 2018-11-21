using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace Adaptive.ReactiveTrader.Client.Behaviors
{
    /// <summary>
    /// Behavior to only trigger the visual states for PointerOver if the pointer is a mouse. Otherwise it can get stuck "on" for touch. 
    /// Button manages this better but I cannot figure out exactly how yet.
    /// </summary>
    public class MouseOverVisualStateBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PointerEntered += AssociatedObjectPointerEntered;
            AssociatedObject.PointerExited += AssociatedObjectPointerExited;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PointerEntered -= AssociatedObjectPointerEntered;
            AssociatedObject.PointerExited -= AssociatedObjectPointerExited;
        }


        private void AssociatedObjectPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                GotoState("PointerOver");
            }
        }

        private void AssociatedObjectPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                GotoState("Normal");
            }
        }

        private void GotoState(string stateName)
        {
            Control control = VisualStateUtilities.FindNearestStatefulControl(AssociatedObject);
            if (control != null)
            {
                VisualStateManager.GoToState(control, stateName, true);
            }
        }
    }
}
