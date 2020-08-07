# BepInEx.MultiFolderLoader

A simple loader to allow loading BepInEx plugins from isolated folders.  
Useful for integrating BepInEx with mod loaders or mod repositories that require 
by-folder setup like Steam Workshop.

## Requirements

BepInEx 5.3 or newer

## Installation

1. Download the latest DLL from releases
2. Put the downloaded DLL into `BepInEx\patchers`
3. Add the following lines to `doorstop_config.ini`:
   
   ```ini
   [MultiFolderLoader]
   baseDir = <FULL PATH TO THE MODS FOLDER>
   disabledModsListPath = <OPTIONAL FULL PATH TO MODS IGNORE FILE>
   ```
   
   where you specify the full path to the folder that will work as mod base.
   *Optionally*, add a path to a file that lists folders to exclude.
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

## Skipping mods

To skip mods, create a file where you list each folder name to skip loading. Separate folders by a newline (`\n` or `\r\n`). Then, specify the full path to said file in `doorstop_config.ini` using `disabledModsListPath` config value.

For example, the following file:

```
ConfigManager
MirrorInternalLogs
```

will skip loading `ConfigManager` and `MirrorInternalLogs` from the mod folder shown above.

Notes:
* Mod folder names are case-insensitive
* Name of the mod ignore list file can be anything, only the contents matter
* If loader fails to find or read the ignore list, all mods are loaded (and appropriate warning message emitted into bepin logs)