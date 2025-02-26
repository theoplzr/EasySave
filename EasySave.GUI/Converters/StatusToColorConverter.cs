using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a status string into a corresponding <see cref="Brush"/> color.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        /// <summary>
        /// Maps the status string to a specific color. Known values are "success" (light green) or "failed" (red).
        /// Defaults to white if no known match is found.
        /// </summary>
        /// <param name="value">The status string to evaluate.</param>
        /// <param name="targetType">The target conversion type (unused).</param>
        /// <param name="parameter">Additional parameter (unused).</param>
        /// <param name="culture">The current culture (unused).</param>
        /// <returns>A <see cref="Brush"/> representing the color for the given status.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLowerInvariant() switch
                {
                    "success" => Brushes.LightGreen,
                    "failed" => Brushes.Red,
                    _ => Brushes.White // Default color
                };
            }
            return Brushes.White;
        }

        /// <summary>
        /// This converter does not support conversion back.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
