# EasySave – Version 1.0 Release Notes

**Date**: February 5, 2025  
**Version**: 1.0 

## New Features

1. **Creation and configuration of backup jobs** (up to 5):
   - Name, source directory, target directory, type (full or differential).
2. **Full or differential backup**:
   - Full: unconditional copy of all files.
   - Differential: copies only modified files (based on date).
3. **Real-time logging** in daily JSON format:
   - Timestamp, file size, transfer time, execution status, etc.
4. **State tracking** in `state.json`:
   - Progress, remaining files, current file, etc.
5. **Internationalization** (French / English):
   - Selection at application startup or via a configuration file.

## Compatibility and Requirements

- **.NET 8.0** or later.
- Windows systems (10+) or any OS supporting .NET 8.
- Read/write access to source and target directories (local or network).

## Known Issues / Bugs

- Copying very large volumes: JSON logs may slow down reading.
- Access errors on certain files are simply logged (no advanced handling).

## Future / Roadmap

- **Version 2.0**: Graphical user interface (WPF) with MVVM architecture.
- **Version 3.0**: Scheduled backups, asynchronous execution.

---

**FISA A3 Computer Science Project – CESI Engineering School, Software Engineering Module**  
**Developed by** Théo PELLIZZARI, Basile ROSIER, Axel Mourot.

