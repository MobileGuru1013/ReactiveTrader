using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Adaptive.ReactiveTrader.Client.Controls
{
    public class LazyContentControl : ContentControl
    {
        public static readonly DependencyProperty LazyContentTemplateProperty = DependencyProperty.Register(
            "LazyContentTemplate", typeof(DataTemplate), typeof(LazyContentControl), new PropertyMetadata(null));

        public DataTemplate LazyContentTemplate
        {
            get { return (DataTemplate) GetValue(LazyContentTemplateProperty); }
            set { SetValue(LazyContentTemplateProperty, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (newContent == null)
            {
                ClearValue(ContentTemplateProperty);
            }
            else
            {
                ContentTemplate = LazyContentTemplate;
            }
        }

        public LazyContentControl()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }
    }
}
