using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null; // Retourne true si la valeur n'est pas null, sinon false
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}