using Microsoft.UI.Xaml.Data;
using System;

namespace Steampunks.Views.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float price)
            {
                return $"${price:F2}";
            }
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 