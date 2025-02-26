# 📖 EasySave 3.0 - User Manual

## 📌 Introduction
EasySave 3.0 is a powerful backup software that provides:
- **Graphical User Interface (GUI)** for an intuitive experience.
- **Remote Console** for monitoring and controlling backups from another machine.
- **Command Line Interface (CLI)** for advanced users and automation.

---

# 🛠 Installation

## 📥 Prerequisites
Before installing EasySave, ensure you have:
- **.NET 8.0** installed.
- **Visual Studio 2022** (or newer).
- **Git** (for version control).

## 🚀 Installation Steps
1. **Download the repository**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Navigate to the project folder**
   ```sh
   cd EasySave
   ```
3. **Restore dependencies**
   ```sh
   dotnet restore
   ```
4. **Build the application**
   ```sh
   dotnet build
   ```
5. **Run the application**
   ```sh
   dotnet run
   ```

---

# 🖥️ EasySave.GUI - Graphical User Interface

### 🏠 Home Screen
Upon launching, the GUI displays a list of existing backup jobs.

### ⚙️ **Settings Tab**
Located in the top-right corner, the **Settings** tab allows you to configure:
- **Language**: Switch between English and French.
- **Log Format**: Choose between JSON or XML.
- **Log Directory**: Set the folder where logs will be saved.
- **Priority Extensions**: Define file extensions that should be backed up first.
- **Encrypted Extensions**: Select which file extensions should be encrypted during backup.

### 🔄 **Main Panel (Center)**
- Displays all created **Backup Jobs**.
- Shows their name, source directory, target directory, backup type, and current status.

### 📌 **Left-Side Buttons**
1. **Add a Backup Job** ➕
2. **Modify an Existing Job** ✏️
3. **View Backup History (Logs)** 📜
4. **Delete a Backup Job** ❌
5. **Execute All Backup Jobs** ▶️

### 📊 **Bottom Section: Real-Time Status**
- Displays live updates on the progress of running backup jobs.
- Useful for monitoring execution and detecting issues.

### ⏸️ **Pause / Stop / Resume Buttons** (Above Real-Time Status)
- **Pause** ⏸️: Temporarily halts an ongoing backup.
- **Stop** ⏹️: Cancels an active backup job.
- **Resume** ▶️: Restarts a paused job.

⚠️ **To use these buttons, a job must be selected from the list.**

---

# 🖥️ EasySave.Client - Remote Console
The Remote Console allows users to control EasySave from another device using **Sockets Communication**.

### 🔘 Available Buttons
1. **Connect**: Establishes a connection to the EasySave Server.
2. **List Jobs**: Displays all backup jobs stored on the server.
3. **Execute Jobs**: Starts all backup jobs.
4. **Pause Jobs**: Pauses a selected backup job.
5. **Resume Jobs**: Resumes a paused job.

---

# 🖥️ EasySaveApp (CLI) - Command Line Interface
The CLI version provides full control through terminal commands.

## Via Console

Go to the EasySaveApp folder.
Run the following command:
```sh
dotnet run
```
Choose the language (en/fr).
Navigate the menu with options 1 to 6.

## Via Command Line

Example:
```sh
dotnet run -- ‘1-3’ → Runs jobs 1, 2 and 3.
dotnet run -- ‘1;3’ → Runs jobs 1 and 3 only.
```

## 🔧 Main Functions

1. **Create a Job**
   - Select **Option 1** (Add a backup job).
   - Enter *Name*, *Source Directory*, *Target Directory*, *Type* (1: Complete, 2: Differential).

2. **List Jobs**
   - Select **Option 3** (List all jobs).

3. **Execute Jobs**
   - Select **Option 2** (Execute all jobs) to run all jobs.
   - Or execute specific jobs via the **command line** (e.g., `"1-3"`).

4. **Remove a Job**
   - Select **Option 4** (Remove a job), then enter the job index.

5. **Update a Job**
   - Select **Option 5** (Update a job), enter the index and new fields (leave empty to keep existing values).

---

# 🎯 Conclusion
EasySave 3.0 provides a flexible, reliable, and powerful backup solution for users with different needs:
- **GUI** for ease of use.
- **Remote Console** for remote management.
- **CLI** for automation and scripting.

📌 For additional support, visit our **GitHub Repository**: [EasySave Repository](https://github.com/theoplzr/EasySave).

