using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.GUI.Helpers;  

namespace EasySave.GUI.Converters
{
    public class JobOrdinalConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                // Utiliser la langue définie dans LanguageHelper
                string lang = LanguageHelper.Instance.CurrentLanguage;
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
            return value ?? "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
