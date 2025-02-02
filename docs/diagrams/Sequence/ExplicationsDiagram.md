# Explication du Diagramme de Séquence “Execute a Job (Simplifié)”

Ce document décrit en termes simples le **diagramme de séquence** qui illustre comment l’application **EasySave** exécute un job spécifique (exemple : job n° 3) lorsqu’un utilisateur le demande.

---

## Participants

1. **User** : L’acteur humain ou utilisateur final, qui lance l’application console et choisit les options dans le menu.
2. **Program** : Le point d’entrée de l’application console (classe `Program`).  
3. **EasySaveFacade** (nommé `Facade` dans le diagramme) : Classe fournissant un accès simple aux fonctionnalités d’EasySave, sans exposer la complexité interne.
4. **ExecuteJobCommand** (nommé `ExecCmd`) : Commande concrète créée par la façade, qui encapsule l’action « Exécuter un job par index ».
5. **BackupManager** (nommé `BM`) : Classe principale de la logique métier, gérant la liste des jobs et leur exécution.
6. **BackupAlgorithm** (nommé `Algo`) : Représentation simplifiée de l’algorithme de sauvegarde (full ou differential) utilisé par le `BackupManager`.

---

## Étapes du Scénario

1. **Sélection de l’option “Exécuter un job”**  
   L’**utilisateur** interagit avec l’**application console**. Il tape par exemple « 4 » si c’est l’option “Execute a job”. Ou il choisit spécifiquement « Exécuter le job n° X ».  
   \- Sur le diagramme, c’est :  
   ```plaintext
   User -> Program: Choix: "Execute job #n"
