using Microsoft.UI.Xaml.Data;
using System;

namespace Steampunks.Views
{
    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isListed)
            {
                return isListed ? "Listed" : "Not Listed";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 