# **README : EasySave Application (Version 1.0)**

> **Projet FISA A3 Informatique – CESI École d’ingénieurs, bloc Génie Logiciel**.
> Réalisé par **Théo PELLIZZARI**, **Basile ROSIER** et **Axel Mourot**.

Bienvenue dans la **version 1.0** d’**EasySave**, une application console en C# pour gérer des **travaux de sauvegarde** (complets ou différentiels). Voici un aperçu global de l’architecture, des fonctionnalités, et des **design patterns** utilisés.

---

## **Sommaire**

1. **Présentation Générale**  
2. **Fonctionnalités Clés**  
3. **Organisation des Projets et Fichiers**  
4. **Architecture et Design Patterns**  
   I. **Repository Pattern**  
   II. **Command Pattern**  
   III. **Observer Pattern**  
   IV. **Template Method Pattern**  
   V. **Strategy Pattern**  
   VI. **Singleton Pattern (Logger)**  
   VII. **Facade Pattern**  
5. **Fichiers Importants**  
   I. `backup_jobs.json` (Liste des jobs)  
   II. `state.json` (État en temps réel)  
   III. `Logs/*.json` (Logs journaliers)  
6. **Démarrage de l’Application**  
   I. **Exécution via Console**  
   II. **Exécution via Arguments (ligne de commande)**  
7. **Internationalisation (fr/en)**  
8. **Fonctionnalités Futures**  

---

## 1. **Présentation Générale**

**EasySave** est une application console .NET (C#) qui permet de :

- **Créer jusqu’à 5 travaux de sauvegarde** (jobs).  
- **Exécuter** ces travaux (sauvegarde complète ou différentielle).  
- **Consigner** toutes les actions dans un fichier log (au format JSON).  
- **Mettre à jour** un fichier `state.json` indiquant l’état des sauvegardes (progression, nombre de fichiers restants, etc.).  
- **Persister** la liste des jobs dans `backup_jobs.json` afin de les recharger entre deux utilisations.  
- **Afficher** les messages en français ou en anglais.

Cette version 1.0 se base sur une **architecture modulaire** et applique plusieurs design patterns pour faciliter la maintenance et l’évolution future (notamment la V2 avec interface graphique).

---

## 2. **Fonctionnalités Clés**

1. **Création de jobs** de sauvegarde (choix du nom, répertoire source, répertoire cible, type de sauvegarde).  
2. **Listing** et **mise à jour** de ces jobs (changer le nom, les répertoires, ou le type de sauvegarde).  
3. **Exécution complète** (copie inconditionnelle) ou **différentielle** (copie conditionnée par la date de modification).  
4. **Journalisation** en temps réel dans un fichier JSON journalier (un fichier par jour, format `yyyy-MM-dd.json`).  
5. **Mise à jour** en continu du fichier `state.json`, qui indique l’avancement de chaque job actif.  
6. **Chargement/sauvegarde** de la liste de jobs dans `backup_jobs.json`.  
7. **Internationalisation** : L’application demande ou lit la langue souhaitée (fr/en), pour tous les messages affichés.

---

## 3. **Organisation des Projets et Fichiers**

Notre solution est divisée en **trois projets indépendants** :

1. **EasySaveApp** 🖥  
   - Application console principale (interaction avec l’utilisateur).  
   - Utilise `EasySave.Core` pour la gestion des sauvegardes.  

2. **EasySave.Core** 🏗  
   - Contient la logique métier (`BackupManager`, exécution des sauvegardes).  
   - Implémente plusieurs **design patterns** (Command, Observer, Repository, Template Method).  

3. **EasySaveLogs** 📜  
   - Projet dédié à la journalisation (`Logger`), écrivant les logs dans des fichiers JSON.  
   - Implémenté en **Singleton** pour assurer une instance unique. 

### Principaux dossiers/fichiers du projet :

- **`Commands`** : Contient les commandes (AddJobCommand, ExecuteJobCommand, etc.) – **Command Pattern**.  
- **`Models`** : Contient les entités et énumérations (BackupJob, BackupType, BackupState, etc.).  
  - **`Models.BackupStrategies`** : Interface `IBackupStrategy` + implémentations (Full, Differential).  
- **`Observers`** : Interface `IBackupObserver`, classe `FileStateObserver` pour mettre à jour `state.json`.  
- **`Repositories`** : `IBackupJobRepository` + `JsonBackupJobRepository` – **Repository Pattern** (pour `backup_jobs.json`).  
- **`Template`** : `AbstractBackupAlgorithm` et ses dérivés (FullBackupAlgorithm, DifferentialBackupAlgorithm) – **Template Method Pattern**.  
- **`Utils`** : `LanguageHelper` (internationalisation fr/en).  
- **`Facade`** : `EasySaveFacade` masque la complexité de `BackupManager` pour simplifier l’usage.  
- **`BackupManager.cs`** : Point central pour la gestion des jobs et l’exécution (utilise Template Method et Strategy).  
- **`Program.cs`** : Point d’entrée console (menu, lecture de la config, etc.).

---

## 4. **Architecture et Design Patterns**

### 4.1 **Repository Pattern**

- **But** : séparer la logique de persistance (`backup_jobs.json`) de la logique métier.  
- **Classe clé** : `JsonBackupJobRepository` implémente `IBackupJobRepository` pour charger/sauvegarder les `BackupJob`.  
- **Usage** : `BackupManager` reçoit `IBackupJobRepository` en paramètre et appelle `Load()` / `Save()` au lieu d’accéder directement au fichier JSON.

### 4.2 **Command Pattern**

- **But** : encapsuler chaque opération (Ajout, Suppression, Exécution, etc.) dans un objet « Commande ».  
- **Classes clés** : `AddJobCommand`, `RemoveJobCommand`, `ExecuteAllJobsCommand`, `ExecuteJobCommand`, etc. – tous dérivent d’une classe abstraite `BackupCommand` et implémentent `ICommand`.  
- **Usage** : `EasySaveFacade` ou `BackupManager` instancient une commande et appellent `Execute()`.

### 4.3 **Observer Pattern**

- **But** : notifier en temps réel un objet externe lorsque l’état d’une sauvegarde change (fichier en cours, progression, etc.).  
- **Classe clé** : `FileStateObserver` implémente `IBackupObserver`, reçoit les updates (`Update(BackupState state)`) et écrit dans `state.json`.  
- **Usage** : `BackupManager` maintient une liste d’observers (`AddObserver/RemoveObserver`) et les notifie via `NotifyObservers(state)`.

### 4.4 **Template Method Pattern**

- **But** : définir le **squelette** de l’exécution d’une sauvegarde (préparation → récupération des fichiers → copie conditionnelle → finalisation) tout en laissant certaines étapes s’implémenter différemment (complet vs différentiel).  
- **Classes clés** : 
  - `AbstractBackupAlgorithm` (classe abstraite)  
  - `FullBackupAlgorithm` et `DifferentialBackupAlgorithm` (sous-classes concrètes).  
- **Usage** : Dans `BackupManager.ExecuteBackup(BackupJob)`, on choisit l’algorithme (Full vs Differential) et on appelle `algorithm.Execute(job)`.

### 4.5 **Strategy Pattern**

- **But** : décider, fichier par fichier, s’il doit être recopié (sauvegarde différentielle) ou non.  
- **Classes clés** : `IBackupStrategy`, `FullBackupStrategy`, `DifferentialBackupStrategy`.  
- **Usage** : Dans la classe `BackupJob`, on stocke un `_backupStrategy` (complète ou différentielle). Pour un **job différentiel**, on compare les dates de modification pour copier seulement les fichiers modifiés.

### 4.6 **Singleton Pattern (Logger)**

- **But** : s’assurer qu’il n’y ait qu’**une seule instance** du logger dans toute l’application.  
- **Classe clé** : `Logger` (dans le projet **EasySaveLogs**), avec une **méthode statique** `GetInstance(...)` et un **constructeur privé**.  
- **Usage** : `BackupManager` fait `_logger = Logger.GetInstance(logDirectory);`. Tous les logs sont centralisés dans un unique endroit.

### 4.7 **Facade Pattern**

- **But** : fournir une **interface simplifiée** pour manipuler l’application, sans exposer les détails internes (`BackupManager`, `Commands`, etc.).  
- **Classe clé** : `EasySaveFacade` qui propose : `AddJob()`, `RemoveJob()`, `ExecuteAllJobs()`, etc.  
- **Usage** : Dans `Program.cs`, on crée la façade et on l’appelle directement (`facade.AddJob(...)`, `facade.ExecuteJobByIndex(...)`, etc.), au lieu de manipuler le `BackupManager` et les commandes individuellement.

---

## 5. **Fichiers Importants**

1. **`backup_jobs.json`**  
   - Stocke la liste des jobs (`BackupJob`).  
   - Chargé au lancement, sauvegardé après modifications (ajout/suppression/mise à jour).  
   - Géré par le **Repository** (`JsonBackupJobRepository`).

2. **`Logs/xxxx-xx-xx.json`**  
   - Fichiers journaliers (un par date).  
   - Contiennent l’historique des actions de copie (horodatage, chemin source, chemin cible, taille, etc.).  
   - Gérés par la classe `Logger` (Singleton) dans **EasySaveLogs**.

3. **`state.json`**  
   - Fichier d’état mis à jour **en temps réel** par `FileStateObserver`, contenant la progression pour chaque job (fichiers restants, taille restante, fichier en cours, etc.).

4. **`appsettings.json`** (optionnel)  
   - Contient la configuration de base, comme la langue et le répertoire de logs (`Language`, `Logging:LogDirectory`).

---

## 6. **Démarrage de l’Application**

### 6.1 **Exécution via Console**

- se placer dans le dossier /EasySaveApp
- **Compiler** le projet : `dotnet build`.  
- **Lancer** : `dotnet run` (dans le dossier du projet **EasySaveApp**).  
- L’application lit la configuration (`appsettings.json`), initialise la façade (`EasySaveFacade`), puis présente un **menu** :  
  1) **Add a backup job**  
  2) **Execute all jobs**  
  3) **List all jobs**  
  4) **Remove a job**  
  5) **Update a job**  
  6) **Exit**  

### 6.2 **Exécution via Arguments (ligne de commande)**

- On peut lancer l’application avec des **arguments** pour exécuter certains jobs.  
  - Ex : `dotnet run -- "1"` => exécuter le job numéro 1.  
  - Ex : `dotnet run -- "1-3"` => exécuter les jobs 1, 2 et 3.  
  - Ex : `dotnet run -- "1;3"` => exécuter les jobs 1 et 3.  
- Le code parse la chaîne et appelle `facade.ExecuteJobByIndex(...)` ou `facade.ExecuteAllJobs()` selon le format.

---

## 7. **Internationalisation (fr/en)**

- À l’exécution, l’application lit `language` depuis `appsettings.json` ou demande à l’utilisateur de taper **“fr”** ou **“en”**.  
- **`LanguageHelper`** contient deux dictionnaires (frDictionary/enDictionary) et fournit des messages localisés (ex: “Goodbye!” vs “Au revoir !”).  
- Tous les menus et messages peuvent ainsi être affichés en anglais ou en français selon le choix.

---

## 8. **Fonctionnalités Futures**

- **Version 2.0** : Implémentation d’une **interface graphique** (WPF) avec **MVVM**, réutilisant la logique déjà codée (BackupManager, Repositories, Observers…).  
- **Version 3.0** : Possibilité de gestion **asynchrone** des copies, gestion de planification (scheduler), etc.  
- **Optimisations** : Ajout de **tests unitaires**...

---

# **Conclusion**

Notre **version 1.0** d’**EasySave** offre actuellement une **architecture robuste** et modulaire, répondant aux **exigences** suivantes :

- **Application console** en C# (.NET 8)  
- **Gestion** de 1 à 5 jobs  
- **Exécution** Complète ou Différentielle  
- **Logs** JSON et **fichier d’état**  
- **Internationalisation** en français/anglais  
- **Pérennité** et **évolutivité** grâce à plusieurs **Design Patterns** (Command, Repository, Observer, Template Method, Strategy, Singleton, Facade)

Les prochaines versions ajouteront des **interfaces graphiques** et d’autres fonctionnalités avancées.

---

Projet réalisé dans le cadre du **FISA A3 Informatique – CESI École d’ingénieurs, bloc Génie Logiciel**, par  
**Théo PELLIZZARI**, **Basile ROSIER** et **Axel Mourot**.
