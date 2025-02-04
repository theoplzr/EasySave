# **README: EasySave Application (Version 1.0)**

> **FISA A3 Computer Science Project – CESI Engineering School, Software Engineering module**.
> Developed by **Théo PELLIZZARI**, **Basile ROSIER**, and **Axel Mourot**.

Welcome to **version 1.0** of **EasySave**, a C# console application for managing **backup tasks** (full or differential). Below is an overview of the architecture, functionalities, and **design patterns** used.

---

## **Table of Contents**

1. **General Overview**  
2. **Key Features**  
3. **Project and File Organization**  
4. **Architecture and Design Patterns**  
   I. **Repository Pattern**  
   II. **Command Pattern**  
   III. **Observer Pattern**  
   IV. **Template Method Pattern**  
   V. **Strategy Pattern**  
   VI. **Singleton Pattern (Logger)**  
   VII. **Facade Pattern**  
5. **Important Files**  
   I. `backup_jobs.json` (Job list)  
   II. `state.json` (Real-time status)  
   III. `Logs/*.json` (Daily logs)  
6. **Application Startup**  
   I. **Running via Console**  
   II. **Running via Command Line Arguments**  
7. **Internationalization (fr/en)**  
8. **Future Features**  

---

## 1. **General Overview**

**EasySave** is a .NET (C#) console application that allows you to:

- **Create up to 5 backup jobs**.
- **Execute** these jobs (full or differential backup).
- **Log** all actions in a JSON log file.
- **Update** a `state.json` file that indicates backup progress (percentage, remaining files, etc.).
- **Persist** job lists in `backup_jobs.json` for reuse between sessions.
- **Display messages** in French or English.

This version 1.0 is based on a **modular architecture** and applies multiple design patterns to facilitate maintenance and future development (notably version 2 with a graphical interface).

---

## 2. **Key Features**

1. **Creation of backup jobs** (name, source directory, target directory, backup type).
2. **Listing and updating** jobs (change name, directories, or backup type).
3. **Full execution** (unconditional copy) or **differential execution** (copy only modified files).
4. **Real-time logging** into a JSON file (one per day, format `yyyy-MM-dd.json`).
5. **Continuous update** of `state.json`, showing real-time backup progress.
6. **Loading/saving** job lists in `backup_jobs.json`.
7. **Internationalization**: The application requests or reads the desired language (fr/en) for all displayed messages.

---

## 3. **Project and File Organization**

Our solution is divided into **three independent projects**:

1. **EasySaveApp** 🖥  
   - Main console application (user interaction).  
   - Uses `EasySave.Core` for backup management.  

2. **EasySave.Core** 🏗  
   - Contains business logic (`BackupManager`, backup execution).  
   - Implements multiple **design patterns** (Command, Observer, Repository, Template Method).  

3. **EasySaveLogs** 📜  
   - Dedicated logging project (`Logger`), writing logs into JSON files.  
   - Implemented as a **Singleton** to ensure a single instance.  

### Main project folders/files:

- **`Commands`**: Contains command classes (AddJobCommand, ExecuteJobCommand, etc.) – **Command Pattern**.
- **`Models`**: Contains entities and enumerations (BackupJob, BackupType, BackupState, etc.).  
  - **`Models.BackupStrategies`**: Interface `IBackupStrategy` + implementations (Full, Differential).  
- **`Observers`**: Interface `IBackupObserver`, class `FileStateObserver` to update `state.json`.
- **`Repositories`**: `IBackupJobRepository` + `JsonBackupJobRepository` – **Repository Pattern** (for `backup_jobs.json`).
- **`Template`**: `AbstractBackupAlgorithm` and its derived classes (FullBackupAlgorithm, DifferentialBackupAlgorithm) – **Template Method Pattern**.
- **`Utils`**: `LanguageHelper` (fr/en internationalization).
- **`Facade`**: `EasySaveFacade` simplifies `BackupManager` usage.
- **`BackupManager.cs`**: Central job execution management (uses Template Method and Strategy).
- **`Program.cs`**: Console entry point (menu, config reading, etc.).

---

## 4. **Architecture and Design Patterns**

### 4.1 **Repository Pattern**

- **Goal**: Separate persistence logic (`backup_jobs.json`) from business logic.
- **Key Class**: `JsonBackupJobRepository` implements `IBackupJobRepository` for loading/saving `BackupJob`.
- **Usage**: `BackupManager` receives `IBackupJobRepository` and calls `Load()` / `Save()` instead of directly accessing the JSON file.

### 4.2 **Command Pattern**

- **Goal**: Encapsulate each operation (Add, Remove, Execute, etc.) in a "Command" object.
- **Key Classes**: `AddJobCommand`, `RemoveJobCommand`, `ExecuteAllJobsCommand`, `ExecuteJobCommand`, etc.
- **Usage**: `EasySaveFacade` or `BackupManager` instantiate a command and call `Execute()`.

### 4.3 **Observer Pattern**

- **Goal**: Notify an external object in real-time when a backup state changes (current file, progress, etc.).
- **Key Class**: `FileStateObserver` implements `IBackupObserver`, receives updates (`Update(BackupState state)`) and writes to `state.json`.

### 4.4 **Template Method Pattern**

- **Goal**: Define the **execution skeleton** of a backup while allowing certain steps to be implemented differently (full vs differential).
- **Key Classes**: `AbstractBackupAlgorithm`, `FullBackupAlgorithm`, `DifferentialBackupAlgorithm`.

### 4.5 **Strategy Pattern**

- **Goal**: Decide per file whether it should be copied (differential backup) or not.
- **Key Classes**: `IBackupStrategy`, `FullBackupStrategy`, `DifferentialBackupStrategy`.

### 4.6 **Singleton Pattern (Logger)**

- **Goal**: Ensure a **single instance** of the logger.
- **Key Class**: `Logger` (in **EasySaveLogs** project), using a **static method** `GetInstance(...)` and a **private constructor**.

### 4.7 **Facade Pattern**

- **Goal**: Provide a **simplified interface** for interacting with the application.
- **Key Class**: `EasySaveFacade` offering methods like `AddJob()`, `RemoveJob()`, `ExecuteAllJobs()`, etc.

---

## 5. **Important Files**

1. **`backup_jobs.json`**: Stores backup jobs.
2. **`Logs/xxxx-xx-xx.json`**: Daily logs.
3. **`state.json`**: Real-time backup state.
4. **`appsettings.json`** (optional): Stores settings like language and log directory.

---

## 6. **Application Startup**

### 6.1 **Running via Console**

- Navigate to the /EasySaveApp folder
- **Build** the project: `dotnet build`.
- **Run**: `dotnet run`.

### 6.2 **Running via Command Line Arguments**

- Example: `dotnet run -- "1"` → Execute job #1.
- Example: `dotnet run -- "1-3"` → Execute jobs #1, #2, and #3.

---

## 7. **Internationalization (fr/en)**

- Application prompts or reads `language` from `appsettings.json` (fr/en).

---

## 8. **Future Features**

- **Version 2.0**: GUI (WPF) with **MVVM**.
- **Version 3.0**: Async execution, scheduler, etc.

---

Project completed for **FISA A3 Computer Science – CESI Engineering School** by  
**Théo PELLIZZARI**, **Basile ROSIER**, and **Axel Mourot**.

