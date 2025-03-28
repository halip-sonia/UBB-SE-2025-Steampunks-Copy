using System;
using Microsoft.UI.Xaml.Data;

namespace Steampunks.Converters
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float price)
            {
                return $"Price: ${price:F2}";
            }
            return "Price: $0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 