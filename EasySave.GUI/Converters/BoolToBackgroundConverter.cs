using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace EasySave.GUI.Converters
{
    public class BoolToJobBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isChecked = value as bool? ?? false;
            return isChecked ? Brush.Parse("#FF005A9E") : Brush.Parse("#FF007ACC");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
