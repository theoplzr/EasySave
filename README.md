# EasySave 3.0 - README

## 📖 Introduction
EasySave 3.0 est une application de sauvegarde avancée qui offre des fonctionnalités modernes telles que les sauvegardes en parallèle, la gestion des fichiers prioritaires et une interface distante permettant un suivi en temps réel des opérations de sauvegarde.

---

## 🛠️ Fonctionnalités Principales
### ✅ Sauvegarde en Parallèle
- Exécute plusieurs tâches de sauvegarde simultanément.
- Optimisation de l'utilisation des ressources système.

### 🔐 Gestion des Fichiers Prioritaires
- Assure que les fichiers jugés prioritaires par l'utilisateur sont traités en premier.
- Empêche la sauvegarde des fichiers non prioritaires tant que des fichiers prioritaires sont en attente.

### 📏 Limitation des Fichiers Volumineux
- Bloque la sauvegarde simultanée de fichiers volumineux pour éviter de surcharger la bande passante.
- Taille limite configurable par l'utilisateur.

### 🖥️ Interface Graphique & Console Déportée
- Interface utilisateur intuitive développée avec **Avalonia**.
- Suivi et gestion des sauvegardes à distance via une console client utilisant **Sockets**.

### 📊 Gestion Dynamique des Ressources *(Optionnel)*
- Réduction automatique des flux de sauvegarde en cas de surcharge réseau détectée.

---

## 🚀 Installation
### 📌 Prérequis
- **.NET 8.0**
- **Visual Studio 2022**
- **Git** pour la gestion des versions

### 🔧 Étapes d’installation
1. **Cloner le projet**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Accéder au répertoire du projet**
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
5. **Exécuter EasySave**
   ```sh
   dotnet run
   ```

---

## 📂 Structure du Projet
| 📄 Dossier/Fichier | 📌 Description |
|-----------------|--------------|
| `EasySave.GUI/` | Interface graphique développée avec Avalonia |
| `EasySave.Server/` | Serveur de gestion des sauvegardes |
| `EasySave.Client/` | Client distant pour la supervision et le contrôle |
| `appsettings.json` | Fichier de configuration de l’application |
| `Logs/` | Dossier contenant les logs des sauvegardes |

---

## 📜 Manuel d’Utilisation
### 🎬 Lancer une sauvegarde
1. **Depuis l’interface graphique :**
   - Sélectionner une tâche et cliquer sur **"Exécuter"**.
2. **Depuis la console distante :**
   - Se connecter au serveur et envoyer la commande : `execute`

### ⏸️ Mettre en pause une sauvegarde
- `pause-[ID_TACHE]` → Suspend la tâche après la copie du fichier en cours.

### ⏯️ Reprendre une sauvegarde en pause
- `resume-[ID_TACHE]`

### ❌ Arrêter une sauvegarde immédiatement
- `stop-[ID_TACHE]`

---

## 🛠️ Support Technique
### 📍 Emplacement des fichiers
- **Configuration :** `appsettings.json`
- **Logs :** `Logs/`
- **Sauvegardes :** définies par l’utilisateur.

### 💻 Configuration Minimale Requise
| Composant | Recommandation |
|-----------|---------------|
| OS | Windows 10 / macOS / Linux |

---

## 🚀 Prochaines Évolutions
- 📊 Optimisation du cryptage des fichiers.
- 🌐 Intégration d'une API REST pour gestion à distance.
- ⚡ Amélioration du multi-threading pour les transferts.

---

## 📞 Contact
- 📘 GitHub : [EasySave Repository](https://github.com/theoplzr/EasySave)

---

🛠️ **Développé par Théo, Basile et Axel EasySave - 2024** 🚀

