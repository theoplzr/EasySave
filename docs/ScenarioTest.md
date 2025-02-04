# EasySave Test Scenarios – Version 1.0

This document outlines several **test scenarios** that can be executed **live** to demonstrate that the **EasySave** application meets all the requirements of **Deliverable 1**. These scenarios cover **job creation**, **execution** (full/differential), **logging**, **real-time state updates** (`state.json`), and **multi-language support** (fr/en).

---

## **Prerequisites**

1. Have **.NET 8** (or later) installed.  
2. Ensure the **solution** (`EasySaveApp` + `EasySaveLogs` folder) is correctly built (`dotnet build` or equivalent).  
3. Have the **`backup_jobs.json`** file (can be empty or not), the **`Logs/`** folder (even empty), and an initial **`state.json`** file (can be empty).  
4. (Optional) Edit **`appsettings.json`** if you want to test the default language or log directory.

---

## **Scenario 1: Start the application and choose the language**

**Objective**: Verify **multi-language support** (fr/en) and correct application initialization.

1. **Run** the application via the console:
   ```bash
   dotnet run
   ```
2. The application **prompts**: "Choose language (en/fr)".  
3. **Enter** `fr`.  
   - **Expected**: The menu should display in **French** with options (1) Ajouter un travail, (2) Exécuter tous les travaux, etc.  
4. **Exit** the application and restart. Enter `en` this time.  
   - **Expected**: The menu should now be in **English** (1) Add a backup job, etc.

**Validation**: The application correctly supports two languages, and the menu adapts accordingly.

---

## **Scenario 2: Create a job and verify `backup_jobs.json`**

**Objective**: Confirm that a backup job can be created (up to 5 max) and stored in `backup_jobs.json`.

1. **Start** the application (language choice does not matter).  
2. Select **Option 1**: "Add a backup job".  
3. **Enter**:  
   - Name: `Backup1`  
   - Source directory: `(existing source path)`  
   - Target directory: `(existing target path)`  
   - Type: `1` (Complete)  
4. **Expected**: The app displays "Backup job 'Backup1' added." (or translated version).  
5. **Exit** the application and **open** `backup_jobs.json`.  
   - **Expected**: It contains a JSON object representing `Backup1`, with `SourceDirectory`, `TargetDirectory`, `BackupType` (Complete), etc.

**Validation**: The job is successfully created and persists in the JSON file.

---

## **Scenario 3: List and Update a Job**

**Objective**: Verify that existing jobs can be listed and modified.

1. **Restart** the application.  
2. Select **Option 3**: "List all jobs".  
   - **Expected**: See `[1] Name: Backup1, Source: …, Target: …, Type: Complete`.  
3. Select **Option 5**: "Update a job".  
   - When prompted for **Index**, enter `1`.  
   - **Name**: Enter a new name, e.g., `BackupRenamed`.  
   - **Source dir**: Leave blank (just press `Enter`) to keep existing.  
   - **Target dir**: Leave blank as well.  
   - **BackupType**: Enter `2` for Differential.  
4. **Expected**: Message "Job ‘BackupRenamed’ updated successfully."  
5. **List** jobs again (Option 3).  
   - **Expected**: `[1] Name: BackupRenamed, Source: (old path), Target: (old path), Type: Differential`.

**Validation**: Updating a job and listing works as expected.

---

## **Scenario 4: Execute a Job (Full Backup)**

**Objective**: Verify file copying, logging (`Logs/*.json`), and state tracking (`state.json`) in full mode.

1. **Create** (or have) a `Complete` job (Scenario 2).  
2. **Option 2**: "Execute all jobs".  
3. **Observations**:
   - The application may display "Copied: … -> …" for each file or mention a log entry.  
   - At completion: "Backup 'BackupRenamed' completed." (or FR version).  
4. **Go to** the `Logs/` folder.  
   - **Expected**: A file named `yyyy-MM-dd.json` (today's date).  
   - Open it: it should contain **multiple entries** (`LogEntry`), each copy action with `Timestamp`, `BackupName`, `SourceFilePath`, `TargetFilePath`, `FileSize`, `TransferTimeMs`, `Status`.  
5. **Check** `state.json`.  
   - It should indicate a final state (Remaining files = 0, etc.).

**Validation**: Full copy, logging, and state update are working correctly.

---

## **Scenario 5: Execute a Differential Job**

**Objective**: Confirm that only modified files are copied and logged correctly.

1. **Create** or **update** a job to `Differential`.  
2. **Modify** a single file in the source directory to change its date.  
3. **Option 2**: "Execute all jobs".  
4. **Observations**:
   - If this is the first execution, all files are copied.  
   - On a **second execution**, only modified files are copied (e.g., one file).  
5. **Check** `Logs/…`:  
   - **Expected**: New entries should only include modified files.

**Validation**: Differential logic works based on the modification date.

---

## **Scenario 6: Remove a Job and Verify**

1. **List** jobs first (Option 3).  
2. **Option 4**: "Remove a job". Enter the corresponding index (e.g., `1`).  
3. **Expected**: "Backup job 'X' removed."  
4. **List** jobs again:
   - The job should no longer appear.  
5. **Open** `backup_jobs.json`: the deleted job should be removed.

---

## **Scenario 7: Run the Application with Arguments** (e.g., `"1;3"`)

**Objective**: Verify command-line execution (e.g., `dotnet run -- "1;3"`).

1. **Terminal**:
   ```bash
   dotnet run -- "1;3"
   ```
2. **Expected**:
   - The app parses `"1;3"`, executes jobs #1 and #3 sequentially, then exits.  
   - Check `Logs/` and `state.json`.  
   - No interactive menu (only output display).

**Validation**: Allows automation without interactive input.

---

## **Scenario 8: (Optional) Verify the 5-Job Limit**

1. **Attempt** to create 6 jobs (Option 1 → Add job).  
2. On the 6th job, the application should show an **error** or raise an **exception**:
   - "Maximum of 5 backup jobs allowed."  

**Validation**: The application enforces the job limit.

---

## **Conclusion**

Executing these **eight test scenarios** confirms:

- **Job creation, update, deletion, execution** (full/differential).
- **Logging in `Logs/`**.
- **State updates in `state.json`**.
- **Multi-language support**.
- **Command-line usage (arguments)**.
- **Enforcing a 5-job limit**.

