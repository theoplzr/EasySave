using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a null or non-null value into a boolean suitable for visibility binding.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a given object to a boolean indicating visibility (true = visible, false = hidden).
        /// If the value is null, returns false; otherwise true.
        /// </summary>
        /// <param name="value">The input value to check for null.</param>
        /// <param name="targetType">The target conversion type (unused).</param>
        /// <param name="parameter">Additional parameter (unused).</param>
        /// <param name="culture">The current culture (unused).</param>
        /// <returns><c>true</c> if the value is non-null, otherwise <c>false</c>.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        /// <summary>
        /// This converter does not support conversion back.
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
