using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EasySave.GUI.Helpers
{
    public class LanguageHelper : INotifyPropertyChanged
    {
        private static readonly Lazy<LanguageHelper> _instance = new(() => new LanguageHelper());
        public static LanguageHelper Instance => _instance.Value;

        private string _language = "en";

        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> messages = new()
        {
            { "en", new Dictionary<string, string>
                {
                    { "MainWindowTitle", "EasySave" },
                    { "FileMenuHeader", "File" },
                    { "MenuItemConfiguration", "Configuration" },
                    { "MenuItemExit", "Exit" },
                    { "ButtonAdd", "Add" },
                    { "ButtonModify", "Modify" },
                    { "ButtonDelete", "Delete" },
                    { "ButtonExecute", "Execute" },
                    { "ColumnName", "Name" },
                    { "ColumnSource", "Source" },
                    { "ColumnTarget", "Target" },
                    { "ColumnType", "Type" },
                    { "RealTimeStatusHeader", "Real-Time Status" }
                }
            },
            { "fr", new Dictionary<string, string>
                {
                    { "MainWindowTitle", "EasySave" },
                    { "FileMenuHeader", "Fichier" },
                    { "MenuItemConfiguration", "Configuration" },
                    { "MenuItemExit", "Quitter" },
                    { "ButtonAdd", "Ajouter" },
                    { "ButtonModify", "Modifier" },
                    { "ButtonDelete", "Supprimer" },
                    { "ButtonExecute", "Exécuter" },
                    { "ColumnName", "Nom" },
                    { "ColumnSource", "Source" },
                    { "ColumnTarget", "Cible" },
                    { "ColumnType", "Type" },
                    { "RealTimeStatusHeader", "État en temps réel" }
                }
            }
        };

        private LanguageHelper() { }

        public void SetLanguage(string language)
        {
            _language = language;
            NotifyPropertyChanged(nameof(MainWindowTitle));
            NotifyPropertyChanged(nameof(FileMenuHeader));
            NotifyPropertyChanged(nameof(MenuItemConfiguration));
            NotifyPropertyChanged(nameof(MenuItemExit));
            NotifyPropertyChanged(nameof(ButtonAdd));
            NotifyPropertyChanged(nameof(ButtonModify));
            NotifyPropertyChanged(nameof(ButtonDelete));
            NotifyPropertyChanged(nameof(ButtonExecute));
            NotifyPropertyChanged(nameof(ColumnName));
            NotifyPropertyChanged(nameof(ColumnSource));
            NotifyPropertyChanged(nameof(ColumnTarget));
            NotifyPropertyChanged(nameof(ColumnType));
            NotifyPropertyChanged(nameof(RealTimeStatusHeader));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string MainWindowTitle => GetMessage("MainWindowTitle");
        public string FileMenuHeader => GetMessage("FileMenuHeader");
        public string MenuItemConfiguration => GetMessage("MenuItemConfiguration");
        public string MenuItemExit => GetMessage("MenuItemExit");
        public string ButtonAdd => GetMessage("ButtonAdd");
        public string ButtonModify => GetMessage("ButtonModify");
        public string ButtonDelete => GetMessage("ButtonDelete");
        public string ButtonExecute => GetMessage("ButtonExecute");
        public string ColumnName => GetMessage("ColumnName");
        public string ColumnSource => GetMessage("ColumnSource");
        public string ColumnTarget => GetMessage("ColumnTarget");
        public string ColumnType => GetMessage("ColumnType");
        public string RealTimeStatusHeader => GetMessage("RealTimeStatusHeader");

        public string GetMessage(string key)
        {
            return messages.ContainsKey(_language) && messages[_language].ContainsKey(key)
                ? messages[_language][key]
                : "Message not found";
        }
    }
}
