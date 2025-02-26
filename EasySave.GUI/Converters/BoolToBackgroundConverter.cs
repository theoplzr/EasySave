using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a boolean value to a different background color for a job item.
    /// </summary>
    public class BoolToJobBackgroundConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value (<c>true</c> or <c>false</c>) into a Brush color.
        /// </summary>
        /// <param name="value">The boolean value to evaluate.</param>
        /// <param name="targetType">The target conversion type (unused).</param>
        /// <param name="parameter">Additional parameter if needed (unused).</param>
        /// <param name="culture">The current culture (unused).</param>
        /// <returns>A <see cref="Brush"/> representing the job background color.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isChecked = value as bool? ?? false;
            // If checked => #FF005A9E, otherwise => #FF007ACC
            return isChecked ? Brush.Parse("#FF005A9E") : Brush.Parse("#FF007ACC");
        }

        /// <summary>
        /// Not implemented as conversion back to boolean is not needed in this scenario.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
