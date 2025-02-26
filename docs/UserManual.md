# 📖 EasySave 3.0 - User Manual

🇬🇧 **[English Version](#english-manual)**  
🇫🇷 **[Version Française](#manuel-francais)**  

---

## <a name="english-manual"></a> 📌 **EasySave 3.0 - User Manual (English)**  

## 📌 Introduction
EasySave 3.0 is a powerful backup software that provides:
- **Graphical User Interface (GUI)** for an intuitive experience.
- **Remote Console** for monitoring and controlling backups from another machine.
- **Command Line Interface (CLI)** for advanced users and automation.

➡️ [Go to French Version](#manuel-francais)  

---

# 🛠 Installation

## 📥 Prerequisites
Before installing EasySave, ensure you have:
- **.NET 8.0** installed.
- **Visual Studio 2022** (or newer).
- **Git** (for version control).

## 🚀 Installation Steps
1. **Download the repository**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Navigate to the project folder**
   ```sh
   cd EasySave
   ```
3. **Restore dependencies**
   ```sh
   dotnet restore
   ```
4. **Build the application**
   ```sh
   dotnet build
   ```
5. **Run the application**
   ```sh
   dotnet run
   ```

---

# 🖥️ EasySave.GUI - Graphical User Interface

### 🏠 Home Screen
Upon launching, the GUI displays a list of existing backup jobs.

### ⚙️ **Settings Tab**
Located in the top-right corner, the **Settings** tab allows you to configure:
- **Language**: Switch between English and French.
- **Log Format**: Choose between JSON or XML.
- **Log Directory**: Set the folder where logs will be saved.
- **Priority Extensions**: Define file extensions that should be backed up first.
- **Encrypted Extensions**: Select which file extensions should be encrypted during backup.

### 🔄 **Main Panel (Center)**
- Displays all created **Backup Jobs**.
- Shows their name, source directory, target directory, backup type, and current status.

### 📌 **Left-Side Buttons**
1. **Add a Backup Job** ➕
2. **Modify an Existing Job** ✏️
3. **View Backup History (Logs)** 📜
4. **Delete a Backup Job** ❌
5. **Execute All Backup Jobs** ▶️

### 📊 **Bottom Section: Real-Time Status**
- Displays live updates on the progress of running backup jobs.
- Useful for monitoring execution and detecting issues.

### ⏸️ **Pause / Stop / Resume Buttons** (Above Real-Time Status)
- **Pause** ⏸️: Temporarily halts an ongoing backup.
- **Stop** ⏹️: Cancels an active backup job.
- **Resume** ▶️: Restarts a paused job.

⚠️ **To use these buttons, a job must be selected from the list.**

---

# 🖥️ EasySave.Client - Remote Console
The Remote Console allows users to control EasySave from another device using **Sockets Communication**.

### 🔘 Available Buttons
1. **Connect**: Establishes a connection to the EasySave Server.
2. **List Jobs**: Displays all backup jobs stored on the server.
3. **Execute Jobs**: Starts all backup jobs.
4. **Pause Jobs**: Pauses a selected backup job.
5. **Resume Jobs**: Resumes a paused job.

---

# 🖥️ EasySaveApp (CLI) - Command Line Interface
The CLI version provides full control through terminal commands.

## Via Console

Go to the EasySaveApp folder.
Run the following command:
```sh
dotnet run
```
Choose the language (en/fr).
Navigate the menu with options 1 to 6.

## Via Command Line

Example:
```sh
dotnet run -- ‘1-3’ → Runs jobs 1, 2 and 3.
dotnet run -- ‘1;3’ → Runs jobs 1 and 3 only.
```

## 🔧 Main Functions

1. **Create a Job**
   - Select **Option 1** (Add a backup job).
   - Enter *Name*, *Source Directory*, *Target Directory*, *Type* (1: Complete, 2: Differential).

2. **List Jobs**
   - Select **Option 3** (List all jobs).

3. **Execute Jobs**
   - Select **Option 2** (Execute all jobs) to run all jobs.
   - Or execute specific jobs via the **command line** (e.g., `"1-3"`).

4. **Remove a Job**
   - Select **Option 4** (Remove a job), then enter the job index.

5. **Update a Job**
   - Select **Option 5** (Update a job), enter the index and new fields (leave empty to keep existing values).

---

# 🎯 Conclusion
EasySave 3.0 provides a flexible, reliable, and powerful backup solution for users with different needs:
- **GUI** for ease of use.
- **Remote Console** for remote management.
- **CLI** for automation and scripting.

📌 For additional support, visit our **GitHub Repository**: [EasySave Repository](https://github.com/theoplzr/EasySave).

---

## <a name="manuel-francais"></a> 📌 **EasySave 3.0 - Manuel d'Utilisateur (Français)**  

# 📖 EasySave 3.0 - Manuel d'Utilisateur

## 📌 Introduction
EasySave 3.0 est un logiciel de sauvegarde performant qui offre :
- **Une Interface Graphique (GUI)** pour une expérience intuitive.
- **Une Console à Distance** pour surveiller et contrôler les sauvegardes depuis un autre appareil.
- **Une Interface en Ligne de Commande (CLI)** pour les utilisateurs avancés et l'automatisation.

➡️ [Retour à la version anglaise](#english-manual)  

---

# 🛠 Installation

## 📥 Prérequis
Avant d’installer EasySave, assurez-vous d’avoir :
- **.NET 8.0** installé.
- **Visual Studio 2022** (ou version plus récente).
- **Git** (pour le contrôle de version).

## 🚀 Étapes d’Installation
1. **Télécharger le dépôt**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Se rendre dans le dossier du projet**
   ```sh
   cd EasySave
   ```
3. **Restaurer les dépendances**
   ```sh
   dotnet restore
   ```
4. **Compiler l’application**
   ```sh
   dotnet build
   ```
5. **Exécuter l’application**
   ```sh
   dotnet run
   ```

---

# 🖥️ EasySave.GUI - Interface Graphique

### 🏠 Écran d'Accueil
Lors du lancement, l’interface affiche la liste des travaux de sauvegarde existants.

### ⚙️ **Onglet Paramètres**
Accessible en haut à droite, l’onglet **Paramètres** permet de configurer :
- **Langue** : Basculer entre l’anglais et le français.
- **Format des logs** : Choisir entre JSON ou XML.
- **Répertoire des logs** : Définir l’emplacement des fichiers journaux.
- **Extensions prioritaires** : Définir les extensions à sauvegarder en priorité.
- **Extensions chiffrées** : Sélectionner les extensions de fichiers à chiffrer lors de la sauvegarde.

### 🔄 **Panneau Principal (Centre)**
- Affiche tous les **travaux de sauvegarde créés**.
- Montre leur nom, répertoire source, répertoire cible, type de sauvegarde et état actuel.

### 📌 **Boutons sur la gauche**
1. **Ajouter un travail de sauvegarde** ➕
2. **Modifier un travail existant** ✏️
3. **Afficher l’historique des sauvegardes (Logs)** 📜
4. **Supprimer un travail de sauvegarde** ❌
5. **Exécuter tous les travaux de sauvegarde** ▶️

### 📊 **Section Inférieure : Suivi en Temps Réel**
- Affiche en direct l’état d’avancement des sauvegardes en cours.
- Utile pour surveiller l’exécution et détecter d’éventuels problèmes.

### ⏸️ **Boutons Pause / Stop / Reprise** (au-dessus du suivi en temps réel)
- **Pause** ⏸️ : Met temporairement un travail de sauvegarde en attente.
- **Stop** ⏹️ : Annule immédiatement une sauvegarde en cours.
- **Reprise** ▶️ : Redémarre un travail mis en pause.

⚠️ **Un travail doit être sélectionné pour pouvoir utiliser ces boutons.**

---

# 🖥️ EasySave.Client - Console à Distance
La Console à Distance permet aux utilisateurs de contrôler EasySave depuis un autre appareil via **Sockets**.

### 🔘 Boutons Disponibles
1. **Connexion** : Établit une connexion avec le serveur EasySave.
2. **Lister les Travaux** : Affiche tous les travaux de sauvegarde stockés sur le serveur.
3. **Exécuter les Travaux** : Démarre l’exécution de tous les travaux de sauvegarde.
4. **Mettre en Pause un Travail** : Suspend un travail sélectionné.
5. **Reprendre un Travail** : Redémarre un travail mis en pause.

---

# 🖥️ EasySaveApp (CLI) - Interface en Ligne de Commande
La version CLI permet un contrôle total via des commandes terminal.

## Via la Console

Allez dans le dossier EasySaveApp.
Exécutez la commande suivante :
```sh
dotnet run
```
Choisissez la langue (en/fr).
Naviguez dans le menu avec les options 1 à 6.

## Via la Ligne de Commande

Exemple :
```sh
dotnet run -- ‘1-3’ → Exécute les travaux 1, 2 et 3.
dotnet run -- ‘1;3’ → Exécute uniquement les travaux 1 et 3.
```

## 🔧 Fonctions Principales

1. **Créer un Travail**
   - Sélectionnez **Option 1** (Ajouter un travail de sauvegarde).
   - Renseignez *Nom*, *Répertoire Source*, *Répertoire Cible*, *Type* (1: Complet, 2: Différentiel).

2. **Lister les Travaux**
   - Sélectionnez **Option 3** (Lister tous les travaux).

3. **Exécuter les Travaux**
   - Sélectionnez **Option 2** (Exécuter tous les travaux) pour lancer toutes les sauvegardes.
   - Ou exécutez des travaux spécifiques via la **ligne de commande** (ex : `"1-3"`).

4. **Supprimer un Travail**
   - Sélectionnez **Option 4** (Supprimer un travail), puis entrez l’index du travail à supprimer.

5. **Modifier un Travail**
   - Sélectionnez **Option 5** (Modifier un travail), entrez l’index et les nouveaux paramètres (laisser vide pour conserver les valeurs actuelles).

---

# 🎯 Conclusion
EasySave 3.0 est une solution de sauvegarde flexible, fiable et performante qui répond aux besoins variés des utilisateurs :
- **GUI** pour une utilisation simplifiée.
- **Console à Distance** pour une gestion à distance.
- **CLI** pour l’automatisation et les scripts.

📌 Pour plus d’informations, consultez notre **Dépôt GitHub** : [EasySave Repository](https://github.com/theoplzr/EasySave).
