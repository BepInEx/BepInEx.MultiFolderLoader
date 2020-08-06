# BepInEx.MultiFolderLoader

A simple loader to allow loading BepInEx plugins from isolated folders.  
Useful for integrating BepInEx with mod loaders or mod repositories that require 
by-folder setup like Steam Workshop.

## Requirements

BepInEx 5.3 or newer

## Installation

1. Download the latest DLL from releases
2. Put the downloaded DLL into `BepInEx\preloaders`
3. Add the following lines to `doorstop_config.ini`:
   
   ```ini
   [MultiFolderLoader]
   baseDir = <FULL PATH TO THE MODS FOLDER>
   ```
   
   where you specify the full path to the folder that will work as mod base.
4. Run the game

## Mod folder layout

Mod folder consists of subfolders. A subfolder is considered a mod when it has at least one of the following folders inside it:
* `plugins` - contains any plugin DLLs of the mod
* `patchers` - contains any preloader patcher DLLs of the mod

An example tree of a mod folder setup:

```
.
├── ConfigManager
│   └── plugins
│       ├── ConfigurationManager.dll
│       └── ConfigurationManager.xml
├── DebugConsole
│   └── plugins
│       └── BepInEx.DeveloperConsole.dll
└── MirrorInternalLogs
    └── patchers
        └── MirrorInternalLogs.dll
```