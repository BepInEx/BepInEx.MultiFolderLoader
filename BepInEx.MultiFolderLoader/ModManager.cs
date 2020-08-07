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
        private const string CONFIG_NAME = "doorstop_config.ini";
        private static readonly List<Mod> Mods = new List<Mod>();
        private static string modsBaseDir;

        private static readonly HashSet<string> blockedMods =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

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
            if (!Directory.Exists(modsBaseDir))
            {
                MultiFolderLoader.Logger.LogWarning("No mod folder found!");
                return;
            }

            foreach (var dir in Directory.GetDirectories(modsBaseDir))
            {
                var dirName = Path.GetFileName(dir);
                if (blockedMods.Contains(dirName))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"Skipping loading [{dirName}] because it's marked as disabled");
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
                if (!ini.TryGetValue("MultiFolderLoader", out var section))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"No [MultiFolderLoader] section in {CONFIG_NAME}, skipping loading mods...");
                    return false;
                }

                if (!section.Entries.TryGetValue("baseDir", out modsBaseDir))
                {
                    MultiFolderLoader.Logger.LogWarning(
                        $"No [MultiFolderLoader].baseDir found in {CONFIG_NAME}, no mods to load!");
                    return false;
                }

                if (!section.Entries.TryGetValue("disabledModsListPath", out var disabledModsListPath))
                    MultiFolderLoader.Logger.LogWarning(
                        $"No [MultiFolderLoader].disabledModsListPath found in {CONFIG_NAME}, no disabled mods list to load!");
                else
                    InitDisabledList(disabledModsListPath);
                
                return true;
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogWarning($"Failed to read {CONFIG_NAME}: {e}");
                return false;
            }
        }

        private static void InitDisabledList(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    MultiFolderLoader.Logger.LogWarning($"Disable list {path} does not exist, skipping loading");
                    return;
                }

                foreach (var line in File.ReadAllLines(path))
                    blockedMods.Add(line.Trim());
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogWarning($"Failed to load list of disabled mods: {e}");
            }
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