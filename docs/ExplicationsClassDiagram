# Explication du Diagramme de Classes EasySave

Ce document décrit les **principales classes** et **interfaces** représentées dans le diagramme de classes du projet EasySave. Chaque paquet (package) correspond à un dossier de l'application, et chaque classe décrit son rôle, ses attributs essentiels et ses méthodes principales. L'objectif est de donner une **vue d'ensemble** de l'architecture pour qu'on puisse comprendre qui fait quoi et comment les classes interagissent.

---

## 1. Programme et Façade

### `Program`  
- **Rôle** : Point d’entrée de l’application console.  
- **Principales méthodes** :  
  - `Main(args)`: Lance le programme, affiche le menu, gère les arguments en ligne de commande.  
  - `ExecuteBackupFromArgs(...)`: Parse les arguments (`1-3`, `1;3`, etc.) pour exécuter les backups.  
  - `CreateJobFromConsole(...)`: Demande à l’utilisateur les informations nécessaires pour créer un `BackupJob`.

### `EasySaveFacade`  
- **Rôle** : Fournir une interface simplifiée (« façade ») aux fonctionnalités d’EasySave, sans exposer la complexité interne.  
- **Attribut clé** : `_backupManager` (référence vers `BackupManager`).  
- **Principales méthodes** :  
  - `AddJob(...)`, `RemoveJob(...)`, `UpdateJob(...)`: Opérations sur les travaux de sauvegarde.  
  - `ExecuteAllJobs()`, `ExecuteJobByIndex(...)`: Lance l’exécution des sauvegardes.  
  - `ListJobs()`, `GetJobCount()`: Liste et compte les jobs.  

Cette façade est utilisée par `Program` pour manipuler les fonctionnalités de sauvegarde en toute simplicité.

---

## 2. BackupManager et BackupJob

### `BackupManager`  
- **Rôle** : Classe centrale de la logique métier. Gère la liste des `BackupJob`, leur exécution, et le logging.  
- **Attributs clés** :  
  - `_jobRepository`: Récupère/stocke les `BackupJob` via un repository.  
  - `_backupJobs`: La liste en mémoire des jobs.  
  - `_logger`: Le logger (singleton) pour journaliser les actions.  
  - `_observers`: Liste d’observateurs (pattern Observer) pour mettre à jour l’état (`state.json`).  
- **Méthodes principales** :  
  - `AddBackupJob(...)`, `RemoveBackupJob(...)`, `UpdateBackupJob(...)`, `ListBackupJobs()`: Gestion des jobs.  
  - `ExecuteAllJobs()`, `ExecuteBackupByIndex(...)`: Exécute les sauvegardes (complètes ou différentielles).  
  - `ExecuteBackup(...)`: Méthode interne qui instancie l’algorithme (Template Method) et parcourt les fichiers.

### `BackupJob`  
- **Rôle** : Représente un **travail de sauvegarde**. Contient son nom, ses répertoires source/cible, le type de sauvegarde (`BackupType`), et une stratégie (`IBackupStrategy`).  
- **Attributs clés** :  
  - `Name`, `SourceDirectory`, `TargetDirectory`, `BackupType` (complet ou différentiel).  
  - `_backupStrategy` (pattern Strategy) : peut être `FullBackupStrategy` ou `DifferentialBackupStrategy`.  
- **Méthode** : `ToString()` pour décrire brièvement le job (utile en logging ou affichage).

---

## 3. Models : Stratégies de Sauvegarde et État

### `BackupType` (enum)  
- **Valeurs** : `Complete` (sauvegarde complète) ou `Differential` (sauvegarde différentielle).

### `IBackupStrategy`  
- **Rôle** : Interface pour la logique de décision « Faut-il copier ce fichier ? ».  
- **Méthode** :  
  - `ShouldCopyFile(sourceFilePath, targetFilePath)`: Retourne `true` ou `false`.

### `FullBackupStrategy` et `DifferentialBackupStrategy`  
- **Rôle** : Implémentent `IBackupStrategy`.  
  - `FullBackupStrategy` copie systématiquement tous les fichiers.  
  - `DifferentialBackupStrategy` compare les dates de modification pour ne copier que les fichiers modifiés.

### `BackupState`  
- **Rôle** : Représente l’état d’avancement d’un job (nom, nombre total/restant de fichiers, taille, fichier en cours, etc.).  
- **Utilisation** : Stocké dans `state.json`, mis à jour en temps réel par un observateur (e.g. `FileStateObserver`).

---

## 4. Observers

### `IBackupObserver`  
- **Rôle** : Interface pour l’observation de l’avancement des sauvegardes.  
- **Méthode** : `Update(state: BackupState)`.

### `FileStateObserver`  
- **Rôle** : Implémentation de `IBackupObserver`. Écrit l’état courant dans un fichier unique (`state.json`).  
- **Attributs** :  
  - `_stateFilePath`: Chemin du fichier `state.json`.  
  - `_states`: Tableau/dictionnaire des états par job.  
- **Méthode** :  
  - `Update(state)`: Mets à jour `_states[state.JobId]` et réécrit tout le JSON.

---

## 5. Repositories

### `IBackupJobRepository`  
- **Rôle** : Interface pour la persistance des `BackupJob`.  
- **Méthodes** :  
  - `Load()`: Récupère la liste de jobs.  
  - `Save(...)`: Enregistre la liste de jobs.

### `JsonBackupJobRepository`  
- **Rôle** : Implémentation concrète stockant les jobs dans un fichier JSON (`backup_jobs.json`).  
- **Attribut** : `_filePath` (chemin du fichier).  
- **Méthodes** :  
  - `Load()`: Lit et désérialise le JSON en liste de `BackupJob`.  
  - `Save(jobs)`: Sérialise et écrit la liste dans le fichier JSON.

---

## 6. Template

### `AbstractBackupAlgorithm`  
- **Rôle** : Classe abstraite définissant le **squelette** (template) de l’exécution d’un backup (préparer, parcourir les fichiers, copier, finaliser).  
- **Attributs** :  
  - `_logger`: Pour enregistrer les actions.  
  - `_notifyObserver`: Action déléguée pour notifier l’état.  
  - `_saveChanges`: Action déléguée pour persister les modifications (e.g. `SaveChanges()` du `BackupManager`).  
- **Méthodes** :  
  - `Execute(job)`: Appelle successivement `Prepare`, `GatherFiles`, `ShouldCopyFile`, `CopyFile`, `FinalizeBackup`.  
  - `ShouldCopyFile(...)`: **abstraite** – redéfinie dans `FullBackupAlgorithm` ou `DifferentialBackupAlgorithm`.  
  - `CopyFile(...)`: **abstraite** – idem.

### `FullBackupAlgorithm` et `DifferentialBackupAlgorithm`  
- **Rôle** : Sous-classes concrètes du Template Method.  
  - `FullBackupAlgorithm` : copie systématiquement tout.  
  - `DifferentialBackupAlgorithm` : fait usage de la logique différentielle (dates).

---

## 7. EasySaveLogs (Logger)

### `Logger`  
- **Rôle** : Enregistrer chaque action de copie (ou erreur) dans un fichier JSON journalier (`yyyy-MM-dd.json`).  
- **Pattern** : **Singleton** (une seule instance dans toute l’application).  
- **Méthodes** :  
  - `GetInstance(...)`: Retourne l’unique `Logger`.  
  - `LogAction(entry)`: Ajoute l’entrée dans le fichier du jour.

### `LogEntry`  
- **Rôle** : Contient les infos d’une action : timestamp, nom du backup, chemins source/cible, taille, temps de copie, statut.

---

## 8. Commands

### `ICommand`  
- **Rôle** : Interface pour toutes les **commandes** (AddJob, RemoveJob, etc.).  
- **Méthode** : `Execute()`.  

### `BackupCommand`  
- **Rôle** : Classe abstraite implémentant `ICommand` et stockant le `BackupManager`.  
- **Sous-classes** :  
  - `AddJobCommand`, `RemoveJobCommand`, `ExecuteAllJobsCommand`, `ExecuteJobCommand`, `UpdateJobCommand`, `ListJobsCommand`.  
- **But** : Chacune encapsule une opération précise.  

Exemples :  
- `AddJobCommand` : `Execute()` => appelle `BackupManager.AddBackupJob(...)`.  
- `RemoveJobCommand` : `Execute()` => appelle `BackupManager.RemoveBackupJob(...)`.  
- etc.

---

## 9. Liens entre les classes

- **`Program`** utilise la **façade** (`EasySaveFacade`) pour créer/mettre à jour des jobs, les exécuter, etc.  
- La **façade** délègue la logique au **`BackupManager`**, qui manipule directement les jobs et appelle :  
  - Le **`Logger`** pour journaliser.  
  - Le **`IBackupJobRepository`** pour charger/sauver les jobs.  
  - Les **`observers`** (dont `FileStateObserver`) pour mettre à jour l’état.  
- **`BackupJob`** stocke un **`IBackupStrategy`** (full ou differential) pour décider si un fichier doit être recopié.  
- **`AbstractBackupAlgorithm`** (Template Method) est utilisé par le `BackupManager` lors de l’exécution, pour parcourir les fichiers et effectuer la copie selon l’algorithme complet/différentiel.  

---

## Conclusion

Ce diagramme de classe montre comment **chaque composant** d’EasySave interagit :

1. **Program** et **EasySaveFacade** : interaction avec l’utilisateur (menu console).  
2. **BackupManager** : cœur de la logique métier (création, exécution, gestion des jobs).  
3. **Commands** : pattern Command (Add, Remove, Execute, etc.).  
4. **Repository** : persistance des `BackupJob` dans un fichier JSON.  
5. **Observer** : mise à jour du `state.json`.  
6. **Strategy** : décision de copie fichier par fichier.  
7. **Template Method** : algorithme global d’exécution (Full ou Diff).  
8. **Logger (Singleton)** : journalisation des actions.

Ainsi, **toutes les briques** s’articulent de manière claire et modulaire, facilitant la maintenabilité et l’évolution du logiciel.
