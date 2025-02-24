# 📌 Documentation du Diagramme UML

## 📖 Introduction
Ce document décrit la structure du diagramme UML représentant les différentes classes, relations et interactions du projet.

---

## 🔹 **Principales Classes**
### 🏛️ **Classe Principale**
- **Main Class** : Point d'entrée du programme, initialise les services nécessaires.

### 📂 **Gestion des Sauvegardes**
- **BackupManager** : Gère l'ajout, la suppression et l'exécution des tâches de sauvegarde.
- **BackupJob** : Représente une tâche de sauvegarde avec ses paramètres spécifiques.
- **BackupType** *(Enum)* : Définit les types de sauvegarde (Complète ou Différentielle).
- **IBackupStrategy** *(Interface)* : Définit la logique de sauvegarde utilisée.

### 🛠️ **Stratégies de Sauvegarde**
- **FullBackupStrategy** : Implémente une sauvegarde complète.
- **DifferentialBackupStrategy** : Implémente une sauvegarde différentielle.

### 👀 **Observation et État**
- **IBackupObserver** *(Interface)* : Permet aux classes d’être notifiées des changements d’état.
- **FileStateObserver** : Implémente un observateur pour suivre l’état des sauvegardes.
- **BackupState** : Stocke l’état courant des sauvegardes.

### 💾 **Gestion des Fichiers et Logs**
- **Logger** : Gère l’enregistrement des événements.
- **LogEntry** : Représente une entrée dans les logs.

### 🗄️ **Persistance des Sauvegardes**
- **IBackupJobRepository** *(Interface)* : Définit la méthode de stockage des tâches.
- **JsonBackupJobRepository** : Implémente le stockage en fichier JSON.

---

## 🔄 **Relations Entre les Classes**
### 🔗 **Héritage**
- `FullBackupStrategy` et `DifferentialBackupStrategy` **héritent** de `IBackupStrategy`.
- `FileStateObserver` **implémente** `IBackupObserver`.
- `JsonBackupJobRepository` **implémente** `IBackupJobRepository`.

### 🔗 **Associations**
- `BackupManager` contient plusieurs `BackupJob`.
- `BackupManager` utilise un `Logger` pour enregistrer les événements.
- `BackupManager` stocke l’état des tâches via `BackupState`.
- `BackupManager` notifie les observateurs `IBackupObserver` de tout changement d’état.
- `Logger` enregistre des `LogEntry` dans un fichier.

### 🔗 **Dépendances**
- `BackupJob` utilise un `BackupType` pour déterminer sa stratégie.
- `BackupManager` utilise une **stratégie de sauvegarde** (`IBackupStrategy`).
- `JsonBackupJobRepository` sauvegarde les tâches dans un fichier JSON.
- `Logger` écrit des logs avec `LogEntry`.
