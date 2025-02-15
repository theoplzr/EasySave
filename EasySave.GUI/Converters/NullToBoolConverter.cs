using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null; // Si null -> false, sinon -> true
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
