using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a status string into a corresponding color.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "success" => Brushes.LightGreen,
                    "failed" => Brushes.Red,
                    _ => Brushes.White // Default color
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
