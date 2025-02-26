using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EasySave.GUI.ViewModels;
using System;

namespace EasySave.GUI
{
    /// <summary>
    /// Locates and instantiates the appropriate view for a given view model.
    /// </summary>
    public class ViewLocator : IDataTemplate
    {
        /// <summary>
        /// Builds the control associated with the specified view model instance.
        /// </summary>
        /// <param name="param">The view model instance.</param>
        /// <returns>A <see cref="Control"/> corresponding to the discovered view, or a fallback message if not found.</returns>
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            // Replace "ViewModel" with "View" in the full type name
            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        /// <summary>
        /// Determines whether this template can match the provided data object.
        /// </summary>
        /// <param name="data">The object to check for a match.</param>
        /// <returns><c>true</c> if the data is a <see cref="ViewModelBase"/>, otherwise <c>false</c>.</returns>
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
