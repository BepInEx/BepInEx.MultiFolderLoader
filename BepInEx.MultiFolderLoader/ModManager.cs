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
        private static readonly List<Mod> mods = new List<Mod>();

        public static void Init()
        {
            if (!Directory.Exists(Util.ModsPath))
            {
                Util.Logger.LogWarning("No mod folder found!");
                return;
            }

            foreach (var dir in Directory.GetDirectories(Util.ModsPath))
                AddMod(dir);

            // Also resolve assemblies like bepin does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static Assembly ResolveModDirectories(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            foreach (var mod in mods)
                if (Utility.TryResolveDllAssembly(name, mod.ModDir, out var ass))
                    return ass;

            return null;
        }

        public static IEnumerable<string> GetPreloaderPatchesDirs()
        {
            return mods.Select(m => m.PreloaderPatchesPath).Where(s => s != null);
        }

        public static IEnumerable<string> GetPluginDirs()
        {
            return mods.Select(m => m.PluginsPath).Where(s => s != null);
        }

        private static void AddMod(string dir)
        {
            var patchesDir = Path.Combine(dir, "patchers");
            var pluginsDir = Path.Combine(dir, "plugins");

            var patchesExists = Directory.Exists(patchesDir);
            var pluginsExists = Directory.Exists(pluginsDir);

            if (!patchesExists && !pluginsExists)
                return;
            mods.Add(new Mod
            {
                PluginsPath = pluginsExists ? pluginsDir : null,
                PreloaderPatchesPath = patchesExists ? patchesDir : null,
                ModDir = dir
            });
        }
    }
}