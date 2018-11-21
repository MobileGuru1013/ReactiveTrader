using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Microsoft.Xaml.Interactions.Core;
using Microsoft.Xaml.Interactivity;

namespace Adaptive.ReactiveTrader.Client.Behaviors
{
    public class OnLoadedRefreshDataTriggersBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += FrameworkElementLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= FrameworkElementLoaded;
        }

        private void FrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Loaded -= FrameworkElementLoaded;

            var behaviors = Interaction.GetBehaviors(frameworkElement);
            foreach (var dataTriggerBehavior in behaviors.OfType<DataTriggerBehavior>())
            {
                RefreshBinding(dataTriggerBehavior, DataTriggerBehavior.BindingProperty);
            }
        }

        private static void RefreshBinding(DependencyObject target, DependencyProperty property)
        {
            var bindingExpression = target.ReadLocalValue(property) as BindingExpression;
            if (bindingExpression != null && bindingExpression.ParentBinding != null)
            {
                target.ClearValue(property);
                BindingOperations.SetBinding(target, property, bindingExpression.ParentBinding);
            }
        }
    }
}