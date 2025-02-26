using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a string value to a boolean, indicating whether the string is non-empty.
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Returns <c>true</c> if the string is non-empty; otherwise <c>false</c>.
        /// </summary>
        /// <param name="value">The value to evaluate, expected to be a string.</param>
        /// <param name="targetType">The target conversion type (unused).</param>
        /// <param name="parameter">Additional parameter (unused).</param>
        /// <param name="culture">The current culture (unused).</param>
        /// <returns><c>true</c> if the input string is non-empty; otherwise <c>false</c>.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        /// <summary>
        /// Conversion back is not supported for this scenario.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
