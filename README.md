# 📚 EasySave 3.0

[🇫🇷 Lire en Français](README_FR.md) | [🇬🇧 Read in English](README.md)

## 📌 Introduction
EasySave 3.0 is an advanced backup software designed for efficiency, flexibility, and ease of use. It offers:
- **Graphical User Interface (GUI)** for an intuitive experience.
- **Remote Console** to monitor and control backups from another device.
- **Command Line Interface (CLI)** for automation and advanced control.

---

## 🧐 Installation

### 📅 Prerequisites
Before installing EasySave, ensure you have:
- **.NET 8.0** installed.
- **Visual Studio 2022** (or newer).
- **Git** (for version control).

### 🚀 Installation Steps
1. **Clone the repository**
   ```sh
   git clone https://github.com/theoplzr/EasySave.git
   ```
2. **Navigate to the project directory**
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

## 🖥️ EasySave.GUI - Graphical User Interface

### 🏠 Home Screen
The GUI displays a list of existing backup jobs upon launch.

### ⚙️ **Settings Tab**
Located in the top-right corner, the **Settings** tab allows you to configure:
- **Language**: Switch between English and French.
- **Log Format**: Choose between JSON or XML.
- **Log Directory**: Set the folder where logs are saved.
- **Priority Extensions**: Define file extensions that should be backed up first.
- **Encrypted Extensions**: Select file extensions to encrypt during backup.

### 🔄 **Main Panel (Center)**
- Displays all created **Backup Jobs**.
- Shows their name, source directory, target directory, backup type, and current status.

### 📌 **Left-Side Buttons**
1. **Add a Backup Job** ➕
2. **Modify an Existing Job** ✏️
3. **View Backup History (Logs)** 📝
4. **Delete a Backup Job** ❌
5. **Execute All Backup Jobs** ▶️

### 📊 **Bottom Section: Real-Time Status**
- Displays live updates on the progress of running backup jobs.
- Useful for monitoring execution and detecting issues.

### ⏸️ **Pause / Stop / Resume Buttons** (Above Real-Time Status)
- **Pause** ⏸️: Temporarily halts an ongoing backup.
- **Stop** ⏹️: Cancels an active backup job.
- **Resume** ▶️: Restarts a paused job.

🚨 **To use these buttons, a job must be selected from the list.**

---

## 🖥️ EasySave.Client - Remote Console
The **Remote Console** allows users to control EasySave from another device via **Sockets Communication**.

### 🛠 Available Buttons
1. **Connect**: Establishes a connection to the EasySave Server.
2. **List Jobs**: Displays all backup jobs stored on the server.
3. **Execute Jobs**: Starts all backup jobs.
4. **Pause Jobs**: Pauses a selected backup job.
5. **Resume Jobs**: Resumes a paused job.

---

## 🖥️ EasySaveApp (CLI) - Command Line Interface
The CLI version provides full control through terminal commands.

### Via Console
1. Navigate to the **EasySaveApp** folder.
2. Run:
   ```sh
   dotnet run
   ```
3. Choose the language (en/fr).
4. Follow the menu (options 1 to 6).

### Via Command Line
Example:
```sh
dotnet run -- "1-3"  # Runs jobs 1, 2, and 3.
dotnet run -- "1;3"  # Runs jobs 1 and 3.
```

### 🛠 Main Functions
1. **Create a Job**  
   - Select **Option 1** (Add a backup job).
   - Enter *Name*, *Source Directory*, *Target Directory*, *Type* (1: Complete, 2: Differential).

2. **List Jobs**  
   - Select **Option 3** (List all jobs).

3. **Execute Jobs**  
   - Select **Option 2** (Execute all jobs).
   - Or execute specific jobs via the **command line** (e.g., `"1-3"`).

4. **Remove a Job**  
   - Select **Option 4** (Remove a job), then enter the job index.

5. **Update a Job**  
   - Select **Option 5** (Update a job), enter the index and new fields (leave empty to keep existing values).

---

## 🧪 Technical Information
### 📍 File Locations
| File | Description |
|------|------------|
| `EasySave.GUI/` | Graphical User Interface (Avalonia-based) |
| `EasySave.Server/` | Backup management server |
| `EasySave.Client/` | Remote client for monitoring and control |
| `appsettings.json` | Configuration file |
| `Logs/` | Directory containing log files |

### 💻 Minimum System Requirements
| Component | Minimum Requirement |
|-----------|--------------------|
| Processor | Intel Core i3 or equivalent |
| RAM | 4 GB |
| Disk Space | 500 MB |
| OS | Windows 10 / macOS / Linux |

---

## 🚀 Future Enhancements
- 📊 Improved file encryption performance.
- 🌐 Integration of a **REST API** for remote management.
- ⚡ Optimized **multi-threading** for faster backups.

---

## 📞 Contact & Support
📌 **GitHub Repository**: [EasySave Repository](https://github.com/theoplzr/EasySave)

📚 [🇫🇷 Lire en Français](README_FR.md)

---

🛠️ **Developed by the EasySave Community - 2024** 🚀
