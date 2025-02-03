# Scénarios de Test EasySave – Version 1.0

Ce document recense plusieurs **scénarios de test** que vous pouvez exécuter **en direct** pour démontrer que l’application **EasySave** répond à toutes les exigences du **Livrable 1**. Ils couvrent la **création** de jobs, leur **exécution** (complète/différentielle), la **journalisation** (logs), l’**état** en temps réel (`state.json`), et la **multi-langue** (fr/en).

---

## **Pré-requis**

1. Avoir **.NET 8** (ou supérieur) installé.  
2. S’assurer d’avoir la **solution** (dossier `EasySaveApp` + `EasySaveLogs`) correctement compilée (`dotnet build` ou équivalent).  
3. Disposer du fichier **`backup_jobs.json`** (peut être vide ou non), du dossier **`Logs/`** (même vide), et d’un **`state.json`** initial (peut être vide).  
4. (Optionnel) Éditer **`appsettings.json`** si vous souhaitez tester la langue par défaut ou le répertoire de logs.

---

## **Scénario 1 : Démarrer l’application et choisir la langue**

**Objectif** : Vérifier la **multi-langue** (fr/en) et la bonne initialisation de l’appli.

1. **Lancer** l’application via la console :  
   ```bash
   dotnet run
   ```
2. L’application **demande** : « Choose language (en/fr) ».  
3. **Taper** `fr`.  
   - **Attendu** : Le menu doit s’afficher en **français** avec les options (1) Ajouter un travail, (2) Exécuter tous les travaux, etc.  
4. **Quitter** l’application pour relancer. Taper `en` cette fois.  
   - **Attendu** : Le menu doit être en **anglais** (1) Add a backup job, etc.

**Validation** : L’application propose bien 2 langues, et le menu s’adapte.  

---

## **Scénario 2 : Créer un job et vérifier `backup_jobs.json`**

**Objectif** : Prouver qu’on peut créer un travail de sauvegarde (jusqu’à 5 max) et le stocker dans `backup_jobs.json`.

1. **Lancer** l’application (choix de langue indifférent).  
2. Sélectionner l’**Option 1** : « Add a backup job ».  
3. **Saisir** :  
   - Nom : `Backup1`  
   - Source directory : `(chemin source existant)`  
   - Target directory : `(chemin cible existant)`  
   - Type : `1` (Complete)  
4. **Attendu** : L’appli affiche « Backup job 'Backup1' added. » (ou la traduction fr).  
5. **Quitter** l’application et **ouvrir** le fichier `backup_jobs.json`.  
   - **Attendu** : Y voir un objet JSON représentant `Backup1`, avec `SourceDirectory`, `TargetDirectory`, `BackupType` (Complete), etc.

**Validation** : Le job est bien créé, persistant dans le JSON.

---

## **Scénario 3 : Lister et Mettre à jour un job**

**Objectif** : Prouver qu’on peut afficher les jobs existants et en modifier certains champs.

1. **Relancer** l’application.  
2. Sélectionner l’**Option 3** : « List all jobs ».  
   - **Attendu** : Voir `[1] Name: Backup1, Source: …, Target: …, Type: Complete`.  
3. Sélectionner l’**Option 5** : « Update a job ».  
   - **Indice** demandé ? Taper `1` (moins 1 si l’index est zero-based selon ton code).  
   - **Nom** : saisir un nouveau nom, ex. `BackupRenamed`.  
   - **Source dir** : laisser vide (juste `Entrée`) pour garder l’existant.  
   - **Target dir** : laisser vide aussi.  
   - **BackupType** : taper `2` pour différentiel.  
4. **Attendu** : Message « Job ‘BackupRenamed’ updated successfully. »  
5. **Lister** à nouveau (Option 3).  
   - **Attendu** : `[1] Name: BackupRenamed, Source: (ancien chemin), Target: (ancien chemin), Type: Differential`.

**Validation** : La mise à jour d’un job et l’énumération fonctionne.

---

## **Scénario 4 : Exécuter un job (Sauvegarde Complète)**

**Objectif** : Vérifier la copie des fichiers, la journalisation (`Logs/*.json`), et l’état (`state.json`) en mode complet.

1. **Créer** (ou avoir déjà) un job de type `Complete` (Scénario 2).  
2. **Option 2** : « Execute all jobs » **ou** Option 2 (in English).  
   - Si tu n’as qu’un job, ça lancera `BackupRenamed`.  
3. **Observation** :  
   - L’appli peut afficher « Copied: … -> … » pour chaque fichier, ou mentionner un log.  
   - À la fin : « Backup 'BackupRenamed' completed. » (ou version FR).  
4. **Aller** dans le dossier `Logs/`.  
   - **Attendu** : Un fichier nommé `yyyy-MM-dd.json` (date du jour).  
   - L’ouvrir : on doit y voir **plusieurs entrées** (`LogEntry`), chaque action de copie avec `Timestamp`, `BackupName`, `SourceFilePath`, `TargetFilePath`, `FileSize`, `TransferTimeMs`, `Status`.  
5. **Vérifier** `state.json`.  
   - Il devrait indiquer un état final (Nombre de fichiers restants = 0, etc.).

**Validation** : Copie inconditionnelle de tous les fichiers, logs journaliers, `state.json` mis à jour.

---

## **Scénario 5 : Exécuter un job Différentiel**

**Objectif** : Vérifier que seules les modifications sont recopiées, journalisation OK.

1. **Créer** ou **mettre à jour** un job en `Differential`.  
2. **Modifier** un seul fichier dans le dossier source pour changer sa date.  
3. Sélectionner l’**Option 2** : « Execute all jobs » (ou exécuter par index).  
4. **Regarder** la console :  
   - Si c’est la première exécution du job, tout sera copié.  
   - Si c’est la **deuxième** exécution, seuls les fichiers modifiés sont recopiés (ex. 1 fichier).  
5. **Aller** dans `Logs/…` :  
   - **Attendu** : Les nouvelles entrées ne concernent que les fichiers modifiés.

**Validation** : La logique différentielle agit sur la date de modification, on voit moins de fichiers recopiés lors de la deuxième exécution.

---

## **Scénario 6 : Supprimer un job et vérifier**

1. **Lister** d’abord les jobs (Option 3).  
2. **Option 4** : « Remove a job ». Taper l’index correspondant (ex. `1`).  
3. **Attendu** : « Backup job 'X' removed. »  
4. **Lister** à nouveau :  
   - Le job ne doit plus apparaître.  
5. **Ouvrir** `backup_jobs.json` : le job supprimé doit avoir disparu.

---

## **Scénario 7 : Lancer l’application avec arguments** (ex. `"1;3"`)

**Objectif** : Vérifier la ligne de commande (ex. `dotner run -- "1;3"`).

1. **Terminal** :  
   ```bash
   dotnet run -- "1;3"
   ```
2. **Attendu** :  
   - L’appli parse `"1;3"`, exécute les jobs #1 et #3 l’un après l’autre, puis se ferme.  
   - Vérifier le log `Logs/` et `state.json`.  
   - Pas d’interaction console (sauf affichage du résultat).

**Validation** : Permet d’automatiser les sauvegardes sans passer par le menu interactif.

---

## **Scénario 8 : (Optionnel) Vérifier la limite de 5 jobs**

1. **Tenter** de créer 6 jobs (Option 1 → Add job).  
2. Au 6ᵉ job, l’application doit afficher une **erreur** ou lever une **exception** :  
   - "Maximum of 5 backup jobs allowed."  
3. **Validation** : La contrainte du cahier des charges est respectée.

---

## **Conclusion**

En exécutant ces **huit scénarios** de démonstration, on montre :

- **Création**, **mise à jour**, **suppression**, **exécution** de jobs (complète / différentielle).  
- La **journalisation** dans `Logs/`.  
- La **mise à jour de l’état** dans `state.json`.  
- La prise en charge **multi-langues**.  
- L’**utilisation** en **ligne de commande** (arguments).  
- La **gestion** d’une limite à 5 jobs.
