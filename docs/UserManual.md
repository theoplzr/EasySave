# **User Manual – EasySave 1.0**

This document provides a **quick** guide to installing and using the **EasySave** console application.

---

## Installation

1. **Install .NET 8** or later.
2. **Download** the `EasySaveApp` folder (or clone the repository).
3. **Place** the folder in the desired location for running the application.

## Running the Application

- **Via Console**:
  1. Navigate to the `EasySaveApp` folder.
  2. Run:
     ```bash
     dotnet run
     ```
  3. Choose the language (en/fr).
  4. Follow the menu (options 1 to 6).

- **Via Command Line**:
  - Example:
    - `dotnet run -- "1-3"` => Executes jobs 1, 2, and 3.
    - `dotnet run -- "1;3"` => Executes jobs 1 and 3.

## Main Functions

1. **Create a job**:
   - Select **Option 1** (Add a backup job).
   - Enter *Name*, *Source Directory*, *Target Directory*, *Type* (1: Complete, 2: Differential).
2. **List jobs**:
   - Select **Option 3** (List all jobs).
3. **Execute jobs**:
   - Select **Option 2** (Execute all jobs) to run all jobs.
   - Or execute specific jobs via the *command line* (e.g., `"1-3"`).
4. **Remove a job**:
   - Select **Option 4** (Remove a job), then enter the job index.
5. **Update a job**:
   - Select **Option 5** (Update a job), enter the index and new fields (leave empty to keep existing values).

## Important Files

- **`backup_jobs.json`**: Stores the list of jobs (name, source, target, type).
- **`Logs/<date>.json`**: Daily log files containing backup history (timestamp, size, etc.).
- **`state.json`**: Real-time status file updating job progress (percentage, current file, etc.).

## Tips

- Ensure read/write permissions on directories.
- Keep `backup_jobs.json` accessible (do not delete it while the application is running).
- If errors occur, check the daily log in `Logs/yyyy-MM-dd.json`.

---

**FISA A3 Computer Science Project – CESI Engineering School, Software Engineering Module**  
**Developed by** Théo PELLIZZARI, Basile ROSIER, Axel Mourot.

