# **README: EasySave Application (Versions 1.1 & 2.0)**

> **FISA A3 Computer Science Project – CESI Engineering School, Software Engineering module**  
> Developed by **Théo PELLIZZARI**, **Basile ROSIER**, and **Axel Mourot**

This document describes the evolution of **EasySave** from its original **version 1.0** to the enhanced **versions 1.1 and 2.0**. While version 1.0 laid the foundation as a C# console application for managing backup tasks, versions 1.1 and 2.0 incorporate significant improvements based on client feedback and evolving requirements.

---

## **Table of Contents**

1. **General Overview**  
2. **Key Features (by Version)**  
3. **Project and File Organization**  
4. **Architecture and Design Patterns**  
   - Repository Pattern  
   - Command Pattern  
   - Observer Pattern  
   - Template Method Pattern  
   - Strategy Pattern  
   - Singleton Pattern (Logger)  
   - Facade Pattern  
5. **Important Files**  
6. **Application Startup**  
7. **Internationalization (fr/en)**  
8. **Version Comparison & Future Roadmap**  

---

## 1. **General Overview**

**EasySave** is a .NET (C#) application designed to manage backup tasks with multiple execution modes (full or differential). Over time, it has evolved in response to client needs:

- **Version 1.0**: A console application limited to 5 backup jobs, with basic logging in JSON.
- **Version 1.1**: Introduces the ability to select the log file format (JSON or XML) to satisfy key client requirements.
- **Version 2.0**: Transitions to a graphical interface (using WPF, Avalonia, or another framework), allows an unlimited number of backup jobs, integrates file encryption via CryptoSoft, and adds business software detection to manage conflicting applications.

---

## 2. **Key Features (by Version)**

### **Version 1.0**
- **Backup Jobs**: Create up to 5 backup jobs (full or differential).
- **Execution**: Execute backup jobs with real-time progress updates.
- **Logging**: Daily log files in JSON format.
- **State File**: Real-time status updates in `state.json`.
- **Internationalization**: Supports French and English.
- **Design Patterns**: Implements Repository, Command, Observer, Template Method, Strategy, Singleton (Logger), and Facade patterns.

### **Version 1.1 Enhancements**
- **Log File Format Selection**: Users can choose between JSON and XML for the daily log file.  
  - **Log File Additional Info**: The log now includes the encryption time for each file backup:
    - `0`: No encryption was applied.
    - `>0`: The time taken (in ms) for encryption.
    - `<0`: An error code indicating encryption failure.
- **Compatibility**: These enhancements ensure compatibility for clients requiring XML logs while maintaining the original JSON format option.

### **Version 2.0 Enhancements**
- **Graphical Interface**: Abandoning the console mode, the application now features a full graphical interface (built with WPF or an equivalent framework) to improve user interaction.
- **Unlimited Backup Jobs**: There is no longer a cap on the number of backup jobs.
- **File Encryption Integration**:  
  - **CryptoSoft Integration**: Incorporates external encryption using CryptoSoft.  
  - **Selective Encryption**: Only files with user-defined extensions (configured in general settings) are encrypted.
- **Business Software Detection**:  
  - If a specified business software is running, new backup jobs are prevented from starting.  
  - For sequential jobs, the currently running file transfer is completed before halting further backups.
  - Any stop due to business software detection is logged in the daily log file.
- **Note on Job Controls**: Although a future interface for job control (Play, Pause, Stop) is envisioned, this functionality is deferred to version 3.0.

---

## 3. **Project and File Organization**

The solution is divided into **three independent projects**:

1. **EasySaveApp**  
   - Main application (console in v1.0/1.1, graphical in v2.0).  
   - Relies on **EasySave.Core** for backup management.

2. **EasySave.Core**  
   - Contains business logic (`BackupManager`, backup execution, etc.).
   - Implements design patterns (Command, Observer, Repository, Template Method, Strategy).

3. **EasySaveLogs**  
   - Manages logging functionality.
   - **Singleton Logger**: Ensures a single instance that writes daily logs (in JSON or XML based on user selection).

### Main project folders/files:

- **`Commands`**: Command classes (e.g., AddJobCommand, ExecuteJobCommand) – **Command Pattern**.
- **`Models`**: Entities (BackupJob, BackupType, BackupState) and backup strategies.
- **`Observers`**: Contains `IBackupObserver` and `FileStateObserver` (updates `state.json`).
- **`Repositories`**: Implements `IBackupJobRepository` with JSON file handling.
- **`Template`**: Contains `AbstractBackupAlgorithm` and its concrete implementations for full and differential backups.
- **`Utils`**: Contains utilities such as `LanguageHelper` for internationalization.
- **`Facade`**: Provides a simplified interface (`EasySaveFacade`) to manage backup operations.
- **`BackupManager.cs`**: Core class managing job execution.
- **`Program.cs`**: Entry point (console for v1.0/1.1; launcher for the graphical interface in v2.0).

---

## 4. **Architecture and Design Patterns**

### **Repository Pattern**
- **Purpose**: Decouples persistence (file storage in `backup_jobs.json`) from business logic.
- **Implementation**: `JsonBackupJobRepository` handles the loading and saving of backup jobs.

### **Command Pattern**
- **Purpose**: Encapsulates operations (add, remove, execute) in command objects.
- **Implementation**: Various command classes are instantiated and executed via the facade or backup manager.

### **Observer Pattern**
- **Purpose**: Enables real-time notifications of backup progress (e.g., current file, progress percentage).
- **Implementation**: `FileStateObserver` updates the `state.json` file.

### **Template Method Pattern**
- **Purpose**: Outlines the steps of a backup process while allowing variations for full vs. differential backups.
- **Implementation**: `AbstractBackupAlgorithm` and its derived classes.

### **Strategy Pattern**
- **Purpose**: Determines file-by-file backup actions (especially for differential backups).
- **Implementation**: Different backup strategies (`FullBackupStrategy`, `DifferentialBackupStrategy`) decide whether to copy a file.

### **Singleton Pattern (Logger)**
- **Purpose**: Ensures that only one instance of the logger is created.
- **Implementation**: `Logger` in the **EasySaveLogs** project, now extended to support both JSON and XML log formats.

### **Facade Pattern**
- **Purpose**: Simplifies interactions with the backup subsystem.
- **Implementation**: `EasySaveFacade` provides high-level methods like `AddJob()`, `RemoveJob()`, and `ExecuteAllJobs()`.

---

## 5. **Important Files**

1. **`backup_jobs.json`**: Persists backup job configurations.
2. **`Logs/xxxx-xx-xx.[json|xml]`**: Daily log files; format is now selectable (JSON or XML).  
   - Logs include additional encryption timing info.
3. **`state.json`**: Provides real-time backup progress.
4. **`appsettings.json`**: Stores application settings such as language, log format preference, encryption extensions, and business software identifiers.

---

## 6. **Application Startup**

### **Version 1.0 / 1.1 (Console Mode)**
- **Build the Project**: Navigate to the `/EasySaveApp` folder and run `dotnet build`.
- **Run**: Execute `dotnet run` or use command-line arguments (e.g., `dotnet run -- "1"` to execute a specific job).

### **Version 2.0 (Graphical Mode)**
- **Launch the GUI**: The application now provides a graphical interface. Follow the provided launcher instructions (or double-click the executable) to start the application.
- **Configuration**: All settings (language, log format, encryption file extensions, business software) are managed via the GUI or the `appsettings.json` file.

---

## 7. **Internationalization (fr/en)**

The application supports both French and English. In all versions:
- On startup, the application reads the preferred language from `appsettings.json` or prompts the user.
- All user messages, labels, and logs are displayed accordingly.

---

## 8. **Version Comparison & Future Roadmap**

| **Feature**                          | **Version 1.0**       | **Version 1.1**                       | **Version 2.0**                        |
|--------------------------------------|-----------------------|---------------------------------------|----------------------------------------|
| **Interface**                        | Console               | Console                               | Graphical (WPF/alternative)            |
| **Backup Jobs Limit**                | Limited to 5          | Limited to 5                          | **Unlimited**                          |
| **Daily Log File**                   | JSON only             | **JSON or XML** (user selectable)     | **JSON or XML** (user selectable)      |
| **Log Encryption Info**              | Not included          | Included (encryption time in ms)      | Included (encryption time in ms)       |
| **Encryption (CryptoSoft)**          | No                    | No                                    | **Yes – selective encryption**         |
| **Business Software Detection**      | No                    | No                                    | **Yes – prevents new job launch**      |
| **Backup Control (Play/Pause/Stop)** | N/A                   | N/A                                   | Deferred to version 3.0                |
| **Command Line Arguments**           | Yes                   | Yes                                   | Not applicable (Graphical mode)        |

**Future Roadmap:**
- **Version 3.0**: Introduction of job control interface (Play, Pause, Stop) and further optimizations like asynchronous execution and scheduling.
