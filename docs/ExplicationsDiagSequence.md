@startuml
skinparam participantPadding 15
title Sequence Diagram – Execute a Job (Simplifié)

actor User
participant Program
participant "EasySaveFacade" as Facade
participant "ExecuteJobCommand" as ExecCmd
participant "BackupManager" as BM
participant "BackupAlgorithm" as Algo

' 1) L'utilisateur choisit d'exécuter un job
User -> Program: Choix: "Execute job #n"

' 2) Program appelle la Façade
Program -> Facade: ExecuteJobByIndex(n)

' 3) La Façade crée la commande et l'exécute
Facade -> ExecCmd: new ExecuteJobCommand(BM, n)
Facade -> ExecCmd: Execute()

' 4) La Commande appelle BackupManager
ExecCmd -> BM: ExecuteBackupByIndex(n)

' 5) Le BM récupère le job et lance ExecuteBackup
BM -> BM: job = backupJobs[n]
BM -> BM: ExecuteBackup(job)

' 6) Le BM instancie l'algorithme (full ou diff)
BM -> Algo: new FullBackupAlgorithm / DiffAlgorithm
BM -> Algo: Execute(job)

' 7) L'algorithme parcourt les fichiers et copie
note right
  - Préparation
  - Parcours des fichiers
  - Copie si nécessaire
  - Finalisation
end note

' 8) Retour en cascade
Algo --> BM: Fin de la sauvegarde
BM --> ExecCmd: (ok)
ExecCmd --> Facade: (term)
Facade --> Program: (fin)
@enduml
