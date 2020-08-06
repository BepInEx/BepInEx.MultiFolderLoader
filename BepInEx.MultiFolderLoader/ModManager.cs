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

        public static void Init()
        {
            var modsPath = GetModPath();
            if (modsPath == null)
                return;
            if (!Directory.Exists(modsPath))
            {
                MultiFolderLoader.Logger.LogWarning("No mod folder found!");
                return;
            }

            foreach (var dir in Directory.GetDirectories(modsPath))
                AddMod(dir);

            // Also resolve assemblies like bepin does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static string GetModPath()
        {
            try
            {
                var ini = GhettoIni.Read(Path.Combine(Paths.GameRootPath, CONFIG_NAME));
                if (ini.TryGetValue("MultiFolderLoader", out var section) &&
                    section.Entries.TryGetValue("baseDir", out var path))
                    return path;
                MultiFolderLoader.Logger.LogWarning(
                    $"No [MultiFolderLoader].baseDir found in {CONFIG_NAME}, no mods to load!");
                return null;
            }
            catch (Exception e)
            {
                MultiFolderLoader.Logger.LogWarning($"Failed to read {CONFIG_NAME}: {e}");
                return null;
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