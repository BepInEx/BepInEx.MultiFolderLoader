using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BepInEx.MultiFolderLoader
{
    public class Mod
    {
        public string PreloaderPatchesPath { get; set; }
        public string PluginsPath { get; set; }
        public string ModDir { get; set; }
    }


    public static class ModManager
    {
        private class ModDirSpec
        {
            public string baseDir;
            public HashSet<string> blockedMods, enabledMods;
        }

        private const string CONFIG_NAME = "doorstop_config.ini";
        public static readonly List<Mod> Mods = new List<Mod>();
        private static readonly List<ModDirSpec> ModDirs = new List<ModDirSpec>();

        public static void Init()
        {
            try
            {
                InitInternal();
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogError($"Failed to index mods, no mods will be loaded: {e}");
            }
        }

        private static void InitInternal()
        {
            if (!InitPaths())
                return;
            foreach (var dir in ModDirs)
            {
                LoadFrom(dir);
            }
        }

        private static void LoadFrom(ModDirSpec modDir)
        {
            var modsBaseDirFull = Path.GetFullPath(modDir.baseDir);
            if (!Directory.Exists(modsBaseDirFull))
            {
                MultiFolderLoader.Logger.LogWarning("No mod folder found!");
                return;
            }

            foreach (var dir in Directory.GetDirectories(modsBaseDirFull))
            {
                var dirName = Path.GetFileName(dir);
                if (modDir.blockedMods != null && modDir.blockedMods.Contains(dirName))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"Skipping loading [{dirName}] because it's marked as disabled");
                    continue;
                }

                if (modDir.enabledMods != null && !modDir.enabledMods.Contains(dirName))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"Skipping loading [{dirName}] because it's not enabled");
                    continue;
                }

                AddMod(dir);
            }

            // Also resolve assemblies like bepin does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static bool InitPaths()
        {
            try
            {
                var ini = GhettoIni.Read(Path.Combine(Paths.GameRootPath, CONFIG_NAME));
                if (!ini.TryGetValue("MultiFolderLoader", out var mainSection))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"No [MultiFolderLoader] section in {CONFIG_NAME}, skipping loading mods...");
                    return false;
                }

                InitSection(mainSection);

                if (mainSection.Entries.TryGetValue("enableAdditionalDirectories", out var enableAdd) &&
                    enableAdd.ToLower() == "true")
                {
                    foreach (var sectionName in ini.Keys.Where(sectionName =>
                        sectionName.StartsWith("MultiFolderLoader_".ToLower())))
                    {
                        MultiFolderLoader.Logger.LogInfo(
                            $"Loading additional section [{sectionName}] from {CONFIG_NAME}");
                        InitSection(ini[sectionName]);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogWarning($"Failed to read {CONFIG_NAME}: {e}");
                return false;
            }
        }

        private static void InitSection(GhettoIni.Section section)
        {
            var spec = new ModDirSpec();
            if (section.Entries.TryGetValue("baseDir", out var baseDir))
            {
                spec.baseDir = Environment.ExpandEnvironmentVariables(baseDir);
            }
            else
            {
                MultiFolderLoader.Logger.LogWarning(
                    $"No [{section.Name}].baseDir found in {CONFIG_NAME}, no mods to load!");
                return;
            }

            if (section.Entries.TryGetValue("disabledModsListPath", out var disabledModsListPath))
            {
                MultiFolderLoader.Logger.LogInfo(
                    $"[{section.Name}].disabledModsListPath found in {CONFIG_NAME}, enabling disabled mods list");
                spec.blockedMods = GetModList(Path.GetFullPath(disabledModsListPath));
            }

            if (section.Entries.TryGetValue("enabledModsListPath", out var enabledModsListPath))
            {
                MultiFolderLoader.Logger.LogInfo(
                    $"[{section.Name}].enabledModsListPath found in {CONFIG_NAME}, enabling enabled mods list");
                spec.enabledMods = GetModList(Path.GetFullPath(enabledModsListPath));
            }

            ModDirs.Add(spec);
        }

        private static HashSet<string> GetModList(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    MultiFolderLoader.Logger.LogWarning($"Mod list {path} does not exist, skipping loading");
                    return null;
                }

                var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var line in File.ReadAllLines(path))
                    result.Add(line.Trim());
                return result;
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogWarning($"Failed to load list of disabled mods: {e}");
            }

            return null;
        }

        private static Assembly ResolveModDirectories(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            foreach (var mod in Mods)
                if (Utility.TryResolveDllAssembly(name, mod.ModDir, out var ass))
                    return ass;

            return null;
        }

        public static IEnumerable<string> GetPreloaderPatchesDirs()
        {
            return Mods.Select(m => m.PreloaderPatchesPath).Where(s => s != null);
        }

        public static IEnumerable<string> GetPluginDirs()
        {
            return Mods.Select(m => m.PluginsPath).Where(s => s != null);
        }

        private static void AddMod(string dir)
        {
            // TODO: Maybe add support for MonoModLoader as well?
            var patchesDir = Path.Combine(dir, "patchers");
            var pluginsDir = Path.Combine(dir, "plugins");

            var patchesExists = Directory.Exists(patchesDir);
            var pluginsExists = Directory.Exists(pluginsDir);

            if (!patchesExists && !pluginsExists)
                return;
            Mods.Add(new Mod
            {
                PluginsPath = pluginsExists ? pluginsDir : null,
                PreloaderPatchesPath = patchesExists ? patchesDir : null,
                ModDir = dir
            });
        }
    }
}