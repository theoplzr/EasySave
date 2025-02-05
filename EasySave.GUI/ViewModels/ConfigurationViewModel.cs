using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using EasySave.Core;            // Vous aurez accès aux propriétés statiques de ConfigurationProvider
using EasySave.Core.Models;
using EasySave.Core.Facade;

namespace EasySave.GUI.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private string _logFormat;
        public string LogFormat
        {
            get => _logFormat;
            set => this.RaiseAndSetIfChanged(ref _logFormat, value);
        }

        public ObservableCollection<string> LogFormatOptions { get; } = new ObservableCollection<string> { "XML", "JSON" };

        private string _extensions;
        public string Extensions
        {
            get => _extensions;
            set => this.RaiseAndSetIfChanged(ref _extensions, value);
        }

        private string _businessSoftware;
        public string BusinessSoftware
        {
            get => _businessSoftware;
            set => this.RaiseAndSetIfChanged(ref _businessSoftware, value);
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public ConfigurationViewModel()
        {
            // Charger les valeurs initiales depuis la configuration via ConfigurationProvider (accès direct aux propriétés statiques)
            LogFormat = ConfigurationProvider.LogFormat;
            Extensions = string.Join(",", ConfigurationProvider.EncryptionExtensions);
            BusinessSoftware = ConfigurationProvider.BusinessSoftware;

            SaveCommand = ReactiveCommand.Create(Save);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Save()
        {
            Console.WriteLine("Configuration saved:");
            Console.WriteLine($"Log Format: {LogFormat}");
            Console.WriteLine($"Extensions: {Extensions}");
            Console.WriteLine($"Business Software: {BusinessSoftware}");
            // Vous pouvez ajouter ici la logique pour mettre à jour appsettings.json si nécessaire.
        }

        private void Cancel()
        {
            Console.WriteLine("Configuration cancelled.");
        }
    }
}
