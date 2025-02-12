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

        // Ajoutez la propriété CurrentLanguage
        public string CurrentLanguage => _language;

        private static readonly Dictionary<string, Dictionary<string, string>> messages = new()
        {
            {
                "en", new Dictionary<string, string>
                {
                    // Clés déjà existantes
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
                    { "RealTimeStatusHeader", "Real-Time Status" },
                    { "JobsTabHeader", "Jobs" },
                    // --- Nouveaux ajout ---
                    { "ConfigurationWindowTitle", "Configuration" },
                    { "LogFormatLabel", "Log Format:" },
                    { "ExtensionsToEncryptLabel", "Extensions to Encrypt:" },
                    { "BusinessSoftwareLabel", "Business Software Name:" },
                    { "ButtonRemove", "Remove" },
                    { "ButtonSave", "Save" },
                    { "ButtonCancel", "Cancel" },
                    { "JobFormWindowTitle", "Job Form" },
                    { "JobNameLabel", "Job Name:" },
                    { "SourceDirectoryLabel", "Source Directory:" },
                    { "TargetDirectoryLabel", "Target Directory:" },
                    { "BackupTypeLabel", "Backup Type:" },
                    { "LanguageSelectionWindowTitle", "Select Language" },
                    { "ChooseYourLanguage", "Choose your language:" },
                    { "English", "English" },
                    { "French", "French" },
                }
            },
            {
                "fr", new Dictionary<string, string>
                {
                    // Clés déjà existantes
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
                    { "RealTimeStatusHeader", "État en temps réel" },
                    { "JobsTabHeader", "Travaux" },
                    // --- Nouveaux ajout ---
                    { "ConfigurationWindowTitle", "Configuration" },
                    { "LogFormatLabel", "Format du Log :" },
                    { "ExtensionsToEncryptLabel", "Extensions à Chiffrer :" },
                    { "BusinessSoftwareLabel", "Logiciel métier :" },
                    { "ButtonRemove", "Retirer" },
                    { "ButtonSave", "Enregistrer" },
                    { "ButtonCancel", "Annuler" },
                    { "JobFormWindowTitle", "Formulaire de Job" },
                    { "JobNameLabel", "Nom du Job :" },
                    { "SourceDirectoryLabel", "Dossier Source :" },
                    { "TargetDirectoryLabel", "Dossier Cible :" },
                    { "BackupTypeLabel", "Type de Sauvegarde :" },
                    { "LanguageSelectionWindowTitle", "Sélection de la Langue" },
                    { "ChooseYourLanguage", "Choisissez votre langue :" },
                    { "English", "Anglais" },
                    { "French", "Français" },
                }
            }
        };

        private LanguageHelper() { }

        public void SetLanguage(string language)
        {
            _language = language;

            // Propriétés déjà existantes
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
            NotifyPropertyChanged(nameof(JobsTabHeader));
            NotifyPropertyChanged(nameof(ProgressTabHeader));

            // Nouveaux
            NotifyPropertyChanged(nameof(ConfigurationWindowTitle));
            NotifyPropertyChanged(nameof(LogFormatLabel));
            NotifyPropertyChanged(nameof(ExtensionsToEncryptLabel));
            NotifyPropertyChanged(nameof(BusinessSoftwareLabel));
            NotifyPropertyChanged(nameof(ButtonRemove));
            NotifyPropertyChanged(nameof(ButtonSave));
            NotifyPropertyChanged(nameof(ButtonCancel));
            NotifyPropertyChanged(nameof(JobFormWindowTitle));
            NotifyPropertyChanged(nameof(JobNameLabel));
            NotifyPropertyChanged(nameof(SourceDirectoryLabel));
            NotifyPropertyChanged(nameof(TargetDirectoryLabel));
            NotifyPropertyChanged(nameof(BackupTypeLabel));
            NotifyPropertyChanged(nameof(LanguageSelectionWindowTitle));
            NotifyPropertyChanged(nameof(ChooseYourLanguage));
            NotifyPropertyChanged(nameof(English));
            NotifyPropertyChanged(nameof(French));
        }

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propriétés existantes
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
        public string JobsTabHeader => GetMessage("JobsTabHeader");
        public string ProgressTabHeader => GetMessage("ProgressTabHeader");

        // Nouveaux
        public string ConfigurationWindowTitle => GetMessage("ConfigurationWindowTitle");
        public string LogFormatLabel => GetMessage("LogFormatLabel");
        public string ExtensionsToEncryptLabel => GetMessage("ExtensionsToEncryptLabel");
        public string BusinessSoftwareLabel => GetMessage("BusinessSoftwareLabel");
        public string ButtonRemove => GetMessage("ButtonRemove");
        public string ButtonSave => GetMessage("ButtonSave");
        public string ButtonCancel => GetMessage("ButtonCancel");
        public string JobFormWindowTitle => GetMessage("JobFormWindowTitle");
        public string JobNameLabel => GetMessage("JobNameLabel");
        public string SourceDirectoryLabel => GetMessage("SourceDirectoryLabel");
        public string TargetDirectoryLabel => GetMessage("TargetDirectoryLabel");
        public string BackupTypeLabel => GetMessage("BackupTypeLabel");
        public string LanguageSelectionWindowTitle => GetMessage("LanguageSelectionWindowTitle");
        public string ChooseYourLanguage => GetMessage("ChooseYourLanguage");
        public string English => GetMessage("English");
        public string French => GetMessage("French");

        private string GetMessage(string key)
        {
            if (messages.ContainsKey(_language) && messages[_language].ContainsKey(key))
            {
                return messages[_language][key];
            }
            return $"[MISSING: {key}]";
        }
    }
}
