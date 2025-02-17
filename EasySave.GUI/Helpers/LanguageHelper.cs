using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EasySave.GUI.Helpers
{
    public class LanguageHelper : INotifyPropertyChanged
    {
        private static readonly Lazy<LanguageHelper> _instance = new(() => new LanguageHelper());
        public static LanguageHelper Instance => _instance.Value;

        private string _language = "en"; // Langue par défaut
        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentLanguage => _language;

        private static readonly Dictionary<string, Dictionary<string, string>> messages = new()
        {
            {
                "en", new Dictionary<string, string>
                {
                    { "MainWindowTitle", "EasySave Project, FISA A3 INFO (24/27)" },
                    { "FileMenuHeader", "Settings" },
                    { "MenuItemConfiguration", "Configuration" },
                    { "MenuItemExit", "Exit" },
                    { "ButtonAdd", "Add" },
                    { "ButtonModify", "Modify" },
                    { "ButtonList", "List All" },
                    { "ButtonDelete", "Delete" },
                    { "ButtonExecute", "Execute" },
                    { "ColumnName", "Name" },
                    { "ColumnSource", "Source" },
                    { "ColumnTarget", "Target" },
                    { "ColumnType", "Type" },
                    { "RealTimeStatusHeader", "Real-Time Status" },
                    { "JobsTabHeader", "Jobs" },
                    { "ProgressTabHeader", "Progress" },
                    { "ConfigurationWindowTitle", "Configuration" },
                    { "LogFormatLabel", "Log Format:" },
                    { "LogDirectoryLabel", "Log Directory:" },  // Nouvelle clé
                    { "ExtensionsToEncryptLabel", "Extensions to Encrypt:" },
                    { "BusinessSoftwareLabel", "Blocking business software:" },
                    { "ButtonRemove", "Remove" },
                    { "ButtonSave", "Save" },
                    { "ButtonCancel", "Cancel" },
                    { "JobFormWindowTitle", "Backup Form" },
                    { "JobNameLabel", "Job Name:" },
                    { "SourceDirectoryLabel", "Source Directory:" },
                    { "TargetDirectoryLabel", "Target Directory:" },
                    { "BackupTypeLabel", "Backup Type:" },
                    { "MenuItemLanguage", "Language" },
                    { "ChooseYourLanguage", "Choose your language:" },
                    { "English", "English" },
                    { "French", "French" },
                    { "JobsListTitle", "Here are your backup jobs created in progress!" },
                    { "ButtonListAllJobs", "List All Jobs" },
                    { "BrowseButton", "Browse" },
                    { "JobNamePlaceholder", "Enter a job name..." },
                    { "SourceDirectoryPlaceholder", "Select source directory..." },
                    { "TargetDirectoryPlaceholder", "Select target directory..." },
                    { "JobNameError", "Please enter a name for the job." },
                    { "SourceDirectoryError", "Please select a source directory." },
                    { "TargetDirectoryError", "Please select a target directory." },
                    { "DirectoryNotExist", "The selected directory does not exist." },
                    { "ErrorLoadingFiles", "Error loading files: " },
                    { "ExecutionBlocked", "🚨 Execution blocked:" },
                    { "IsRunning", "is running." },
                    { "AllJobsExecuted", "✅ All jobs executed successfully." },
                    { "JobDeleted", "🗑️ Job deleted." },
                    { "JobAdded", "✅ Job '{0}' added successfully." },
                    { "JobModified", "✏️ Job '{0}' modified." },
                    { "PleaseSelectJobForDeletion", "❌ Please select a job before deleting." },
                    { "PleaseSelectJobForModification", "❌ Please select a job before modifying." }
                }
            },
            {
                "fr", new Dictionary<string, string>
                {
                    { "MainWindowTitle", "Projet EasySave, FISA A3 INFO (24/27)" },
                    { "FileMenuHeader", "Réglages" },
                    { "MenuItemConfiguration", "Configuration" },
                    { "MenuItemExit", "Quitter" },
                    { "ButtonAdd", "Ajouter" },
                    { "ButtonModify", "Modifier" },
                    { "ButtonList", "Lister les travaux" },
                    { "ButtonDelete", "Supprimer" },
                    { "ButtonExecute", "Exécuter" },
                    { "ColumnName", "Nom" },
                    { "ColumnSource", "Source" },
                    { "ColumnTarget", "Cible" },
                    { "ColumnType", "Type" },
                    { "RealTimeStatusHeader", "État en temps réel" },
                    { "JobsTabHeader", "Travaux" },
                    { "ProgressTabHeader", "Progression" },
                    { "ConfigurationWindowTitle", "Configuration" },
                    { "LogFormatLabel", "Format du Log :" },
                    { "LogDirectoryLabel", "Répertoire des logs :" },  // Nouvelle clé
                    { "ExtensionsToEncryptLabel", "Extensions à Chiffrer :" },
                    { "BusinessSoftwareLabel", "Logiciel métier bloquant :" },
                    { "ButtonRemove", "Retirer" },
                    { "ButtonSave", "Enregistrer" },
                    { "ButtonCancel", "Annuler" },
                    { "JobFormWindowTitle", "Formulaire de sauvegarde" },
                    { "JobNameLabel", "Nom du Job :" },
                    { "SourceDirectoryLabel", "Dossier Source :" },
                    { "TargetDirectoryLabel", "Dossier Cible :" },
                    { "BackupTypeLabel", "Type de Sauvegarde :" },
                    { "MenuItemLanguage", "Langue" },
                    { "ChooseYourLanguage", "Choisissez votre langue :" },
                    { "English", "Anglais" },
                    { "French", "Français" },
                    { "JobsListTitle", "Voici vos travaux de sauvegarde créés en cours !" },
                    { "ButtonListAllJobs", "Lister tous les Travaux" },
                    { "BrowseButton", "Parcourir" },
                    { "JobNamePlaceholder", "Entrez un nom pour le travail..." },
                    { "SourceDirectoryPlaceholder", "Sélectionnez le dossier source..." },
                    { "TargetDirectoryPlaceholder", "Sélectionnez le dossier cible..." },
                    { "JobNameError", "Veuillez entrer un nom pour le travail." },
                    { "SourceDirectoryError", "Veuillez sélectionner un dossier source." },
                    { "TargetDirectoryError", "Veuillez sélectionner un dossier cible." },
                    { "DirectoryNotExist", "Le dossier sélectionné n'existe pas." },
                    { "ErrorLoadingFiles", "Erreur lors du chargement des fichiers : " },
                    { "ExecutionBlocked", "🚨 Exécution bloquée:" },
                    { "IsRunning", "est en cours d'exécution." },
                    { "AllJobsExecuted", "✅ Tous les travaux ont été exécutés avec succès." },
                    { "JobDeleted", "🗑️ Travail supprimé." },
                    { "JobAdded", "✅ Travail '{0}' ajouté avec succès." },
                    { "JobModified", "✏️ Travail '{0}' modifié." },
                    { "PleaseSelectJobForDeletion", "❌ Veuillez sélectionner un travail avant de supprimer." },
                    { "PleaseSelectJobForModification", "❌ Veuillez sélectionner un travail avant de modifier." }
                }
            }
        };

        private LanguageHelper() { }

        public void SetLanguage(string language)
        {
            _language = language;
            NotifyAllProperties();
        }

        private void NotifyAllProperties()
        {
            // Mise à jour de toutes les propriétés
            foreach (var property in typeof(LanguageHelper).GetProperties())
            {
                NotifyPropertyChanged(property.Name);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Propriétés accessibles
        public string MainWindowTitle => GetMessage("MainWindowTitle");
        public string FileMenuHeader => GetMessage("FileMenuHeader");
        public string MenuItemConfiguration => GetMessage("MenuItemConfiguration");
        public string MenuItemExit => GetMessage("MenuItemExit");
        public string ButtonAdd => GetMessage("ButtonAdd");
        public string ButtonModify => GetMessage("ButtonModify");
        public string ButtonList => GetMessage("ButtonList");
        public string ButtonDelete => GetMessage("ButtonDelete");
        public string ButtonExecute => GetMessage("ButtonExecute");
        public string ColumnName => GetMessage("ColumnName");
        public string ColumnSource => GetMessage("ColumnSource");
        public string ColumnTarget => GetMessage("ColumnTarget");
        public string ColumnType => GetMessage("ColumnType");
        public string RealTimeStatusHeader => GetMessage("RealTimeStatusHeader");
        public string JobsTabHeader => GetMessage("JobsTabHeader");
        public string ProgressTabHeader => GetMessage("ProgressTabHeader");
        public string ConfigurationWindowTitle => GetMessage("ConfigurationWindowTitle");
        public string LogFormatLabel => GetMessage("LogFormatLabel");
        public string LogDirectoryLabel => GetMessage("LogDirectoryLabel");  // Nouvelle propriété
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
        public string MenuItemLanguage => GetMessage("MenuItemLanguage");
        public string ChooseYourLanguage => GetMessage("ChooseYourLanguage");
        public string English => GetMessage("English");
        public string French => GetMessage("French");
        public string JobsListTitle => GetMessage("JobsListTitle");
        public string BrowseButton => GetMessage("BrowseButton");
        public string JobNamePlaceholder => GetMessage("JobNamePlaceholder");
        public string SourceDirectoryPlaceholder => GetMessage("SourceDirectoryPlaceholder");
        public string TargetDirectoryPlaceholder => GetMessage("TargetDirectoryPlaceholder");
        public string ErrorLoadingFiles => GetMessage("ErrorLoadingFiles");

        public string GetMessage(string key) =>
            messages[_language].TryGetValue(key, out var value)
                ? value
                : $"[MISSING: {key}]";
    }
}
