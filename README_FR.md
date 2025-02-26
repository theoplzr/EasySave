# 📖 EasySave 3.0 - Manuel Utilisateur

[🇬🇧 English Version](README.md)

## 📌 Introduction
EasySave 3.0 est un logiciel de sauvegarde avancé qui offre :
- **Interface Graphique (GUI)** pour une expérience utilisateur intuitive.
- **Console Déportée** pour surveiller et contrôler les sauvegardes à distance.
- **Interface en Ligne de Commande (CLI)** pour les utilisateurs avancés et l'automatisation.

---

# 🛠 Installation

## 📥 Prérequis
Avant d'installer EasySave, assurez-vous d'avoir :
- **.NET 8.0** installé.
- **Visual Studio 2022** (ou plus récent).
- **Git** (pour le suivi des versions).

## 🚀 Étapes d'Installation
1. **Télécharger le dépôt**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Aller dans le dossier du projet**
   ```sh
   cd EasySave
   ```
3. **Restaurer les dépendances**
   ```sh
   dotnet restore
   ```
4. **Compiler l'application**
   ```sh
   dotnet build
   ```
5. **Exécuter l'application**
   ```sh
   dotnet run
   ```

---

# 🖥️ EasySave.GUI - Interface Graphique

### 🏠 Ecran d'Accueil
Dès le lancement, l'interface affiche la liste des sauvegardes existantes.

### ⚙️ **Onglet Paramètres**
Situé en haut à droite, il permet de configurer :
- **Langue** : Basculer entre l'anglais et le français.
- **Format des Logs** : Choisir entre JSON ou XML.
- **Répertoire des Logs** : Définir le dossier où enregistrer les journaux.
- **Extensions prioritaires** : Définir les fichiers à sauvegarder en priorité.
- **Extensions chiffrées** : Sélectionner les fichiers à chiffrer lors de la sauvegarde.

### 🔄 **Panneau Principal (Centre)**
- Affiche tous les **travaux de sauvegarde créés**.
- Affiche le nom, le répertoire source, le répertoire cible, le type et l'état actuel.

### 📌 **Boutons à gauche**
1. **Ajouter un travail de sauvegarde** ➕
2. **Modifier un travail existant** ✏️
3. **Voir l'historique des sauvegardes (Logs)** 📜
4. **Supprimer un travail de sauvegarde** ❌
5. **Exécuter tous les travaux de sauvegarde** ▶️

### 📊 **Section Inférieure : Statut en Temps Réel**
- Affiche l'avancement des sauvegardes en cours.
- Permet de surveiller l'exécution et de détecter d'éventuels problèmes.

### ⏸️ **Boutons Pause / Arrêt / Reprise** (Au-dessus du statut en temps réel)
- **Pause** ⏸️ : Met en pause une sauvegarde en cours.
- **Arrêt** ⏹️ : Annule une sauvegarde active.
- **Reprise** ▶️ : Redémarre une sauvegarde en pause.

⚠️ **Pour utiliser ces boutons, une sauvegarde doit être sélectionnée.**

---

# 🖥️ EasySave.Client - Console Déportée
La console déportée permet de contrôler EasySave à distance via **Sockets**.

### 🔘 Boutons Disponibles
1. **Connexion** : Se connecte au serveur EasySave.
2. **Lister les sauvegardes** : Affiche tous les travaux enregistrés.
3. **Exécuter les sauvegardes** : Lance l'exécution des travaux.
4. **Mettre en pause une sauvegarde**.
5. **Reprendre une sauvegarde en pause**.

---

# 🖥️ EasySaveApp (CLI) - Interface en Ligne de Commande
L'interface CLI offre un contrôle total par terminal.

## Via Console

Naviguez vers le dossier EasySaveApp.
Exécutez la commande suivante :
```sh
dotnet run
```
Choisissez la langue (en/fr).
Naviguez dans le menu (options 1 à 6).

## Via Ligne de Commande

Exemple :
```sh
dotnet run -- '1-3' → Exécute les tâches 1, 2 et 3.
dotnet run -- '1;3' → Exécute uniquement les tâches 1 et 3.
```

## 🔧 Fonctions Principales

1. **Créer un Travail de Sauvegarde**
   - Choisissez **Option 1** (Ajouter une sauvegarde).
   - Entrez *Nom*, *Dossier Source*, *Dossier Cible*, *Type* (1 : Complète, 2 : Différentielle).

2. **Lister les Travaux**
   - Choisissez **Option 3** (Lister les sauvegardes).

3. **Exécuter les Travaux**
   - Choisissez **Option 2** (Exécuter toutes les sauvegardes).
   - Ou utilisez la **ligne de commande** (ex: `"1-3"`).

4. **Supprimer un Travail**
   - Choisissez **Option 4** (Supprimer une sauvegarde), puis entrez l'index.

5. **Modifier un Travail**
   - Choisissez **Option 5** (Modifier une sauvegarde), entrez l'index et les nouvelles valeurs (laisser vide pour conserver les anciennes).

---

# 🎯 Conclusion
EasySave 3.0 est une solution flexible, fiable et performante pour tous les besoins de sauvegarde :
- **GUI** pour une utilisation simple.
- **Console Déportée** pour le contrôle à distance.
- **CLI** pour l'automatisation et les scripts.

📌 Pour plus d'informations, consultez notre **dépôt GitHub** : [EasySave Repository](https://github.com/theoplzr/EasySave).
