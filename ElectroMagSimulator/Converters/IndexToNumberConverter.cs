using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ElectroMagSimulator.Converters
{
    public class IndexToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
                return (index + 1).ToString();
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
