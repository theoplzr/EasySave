using Avalonia.Data.Converters;
using EasySave.GUI.Helpers;
using System;
using System.Globalization;

namespace EasySave.GUI.Converters
{
    /// <summary>
    /// Converts a zero-based job index to a localized ordinal string (e.g., "Job n°1" or "Travail n°1").
    /// </summary>
    public class JobOrdinalConverter : IValueConverter
    {
        /// <summary>
        /// Converts an integer index to a localized ordinal string based on the current language.
        /// </summary>
        /// <param name="value">The job index (zero-based).</param>
        /// <param name="targetType">The target conversion type (unused).</param>
        /// <param name="parameter">Additional parameter (unused).</param>
        /// <param name="culture">The current culture (unused).</param>
        /// <returns>A localized ordinal string indicating the job number.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                // Retrieve the current language from the LanguageHelper
                string lang = LanguageHelper.Instance.CurrentLanguage;
                
                // Build the ordinal string based on the current language
                if (lang == "fr")
                {
                    return index switch
                    {
                        0 => "Travail n°1",
                        1 => "Travail n°2",
                        2 => "Travail n°3",
                        3 => "Travail n°4",
                        4 => "Travail n°5",
                        _ => $"Travail n° {index + 1}"
                    };
                }
                else
                {
                    return index switch
                    {
                        0 => "Job n°1",
                        1 => "Job n°2",
                        2 => "Job n°3",
                        3 => "Job n°4",
                        4 => "Job n°5",
                        _ => $"Job n° {index + 1}"
                    };
                }
            }
            // If the value is not an integer, return the original value or an empty string
            return value ?? "";
        }

        /// <summary>
        /// Conversion back is not supported in this scenario.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
