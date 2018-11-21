using System;
using Windows.UI.Xaml.Data;

namespace Adaptive.ReactiveTrader.Client.Converters
{
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var stringFormat = parameter as string;
                if (stringFormat != null)
                {
                    return string.Format(stringFormat, value);
                }
                return value.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
