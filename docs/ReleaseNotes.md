# EasySave – Version 1.0 Release Notes

**Date** : * 5 Février 2025  
**Version** : 1.0*  

## Nouvelles Fonctionnalités

1. **Création et configuration de jobs** de sauvegarde (jusqu’à 5) :
   - Nom, répertoire source, répertoire cible, type (complète ou différentielle).
2. **Sauvegarde complète** ou **différentielle** :
   - Complète : copie inconditionnelle de tous les fichiers.
   - Différentielle : copie uniquement les fichiers modifiés (basée sur la date).
3. **Journalisation** en temps réel au format JSON journalier :
   - Horodatage, taille des fichiers, temps de transfert, statut d’exécution, etc.
4. **Suivi de l’état** dans `state.json` :
   - Progression, nombre de fichiers restants, fichier en cours, etc.
5. **Internationalisation** (Français / Anglais) :
   - Sélection au lancement de l’application ou via un fichier de configuration.

## Compatibilité et Pré-requis

- **.NET 8.0** ou plus récent.
- Systèmes Windows (10+) ou tout OS supportant .NET 8.
- Accès en lecture/écriture aux répertoires sources et cibles (locaux ou réseaux).

## Problèmes / Bugs Connus

- Copie de très gros volumes : les logs JSON peuvent ralentir la lecture.
- Erreurs d’accès sur certains fichiers simplement loguées (pas de gestion avancée).

## Futur / Roadmap

- **Version 2.0** : Interface graphique (WPF) avec architecture MVVM.
- **Version 3.0** : Possibilité de planifier les sauvegardes, exécution asynchrone.

---

**Projet FISA A3 Informatique – CESI École d’ingénieurs, bloc Génie Logiciel**  
**Réalisé par** Théo PELLIZZARI, Basile ROSIER, Axel Mourot.
