using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value == null ? false : true; // ⚡ Utilisation de booléens pour `IsVisible`
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
