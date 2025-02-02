# Manuel d’Utilisation – EasySave 1.0

Ce document décrit **rapidement** l’installation et l’utilisation de l’application console **EasySave**.

---

## Installation

1. **Installer .NET 8** ou plus récent.
2. **Récupérer** le dossier `EasySaveApp` (ou cloner le dépôt).
3. **Placer** le dossier là où vous souhaitez lancer l’application.

## Lancement

- **Via Console** :
  1. Se rendre dans le dossier `EasySaveApp`.
  2. Exécuter : 
     ```bash
     dotnet run
     ```
  3. Choisir la langue (en/fr).
  4. Suivre le menu (1 à 6).

- **Via Ligne de Commande** :
  - Exemple : 
    - `EasySaveApp.exe "1-3"` => exécuter les jobs 1, 2, et 3.
    - `EasySaveApp.exe "1;3"` => exécuter les jobs 1 et 3.

## Fonctions Principales

1. **Créer un job** : 
   - Sélectionner **Option 1** (Add a backup job).
   - Saisir *Nom*, *Répertoire Source*, *Répertoire Cible*, *Type* (1: Complete, 2: Differential).
2. **Lister les jobs** : 
   - Sélectionner **Option 3** (List all jobs).
3. **Exécuter les jobs** : 
   - Sélectionner **Option 2** (Execute all jobs) pour tous les jobs.
   - Ou exécuter seulement certains jobs via la *ligne de commande* (ex: `"1-3"`).
4. **Supprimer un job** : 
   - Sélectionner **Option 4** (Remove a job), puis saisir l’index du job.
5. **Mettre à jour un job** : 
   - Sélectionner **Option 5** (Update a job), saisir l’index et les nouveaux champs (laisser vide pour conserver l’existant).

## Fichiers Importants

- **`backup_jobs.json`** : liste des jobs (nom, source, cible, type).
- **`Logs/<date>.json`** : fichier journalier contenant l’historique des copies (horodatage, taille, etc.).
- **`state.json`** : fichier d’état mis à jour en temps réel (progression, fichier en cours…).

## Conseils

- Vérifier les droits en lecture/écriture sur les répertoires.
- S’assurer que `backup_jobs.json` reste accessible (ne pas le supprimer pendant l’utilisation).
- Si des erreurs surviennent, consulter le log du jour dans `Logs/yyyy-MM-dd.json`.

---

**Projet FISA A3 Informatique – CESI École d’ingénieurs, bloc Génie Logiciel**  
**Réalisé par** Théo PELLIZZARI, Basile ROSIER, Axel Mourot.
