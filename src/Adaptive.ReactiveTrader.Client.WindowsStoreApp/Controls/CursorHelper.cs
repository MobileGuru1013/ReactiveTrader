using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Adaptive.ReactiveTrader.Client.Controls
{
    public class CursorHelper
    {
        public static readonly DependencyProperty CursorProperty = DependencyProperty.RegisterAttached(
            "Cursor", typeof (CoreCursorType), typeof (CursorHelper), new PropertyMetadata(default(CoreCursorType), OnCursorChanged));

        public static void SetCursor(DependencyObject element, CoreCursorType value)
        {
            element.SetValue(CursorProperty, value);
        }

        public static CoreCursorType GetCursor(DependencyObject element)
        {
            return (CoreCursorType) element.GetValue(CursorProperty);
        }

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElementExtensions.SetCursor(d, new CoreCursor((CoreCursorType)e.NewValue, 1));
        }
    }
}
