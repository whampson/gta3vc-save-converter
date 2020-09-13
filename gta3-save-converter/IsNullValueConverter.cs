using System;
using System.Globalization;
using System.Windows.Data;

namespace SaveConverter
{
    public class IsNullValueConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
            {
                return value != null;
            }
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"Cannot convert '{value}' to type {targetType}.");
        }
    }
}
