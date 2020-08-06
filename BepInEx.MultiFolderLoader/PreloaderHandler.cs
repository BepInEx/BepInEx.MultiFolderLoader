using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Preloader.Patching;

namespace BepInEx.MultiFolderLoader
{
    public static class PreloaderHandler
    {
        public static void Init()
        {
            var patcherPlugins = AssemblyPatcher.PatcherPlugins.ToList();
            
            Util.Logger.LogInfo("Loading preloader patchers from mod");
            foreach (var preloaderPatchesDir in ModManager.GetPreloaderPatchesDirs())
                AssemblyPatcher.AddPatchersFromDirectory(preloaderPatchesDir);

            var newPatcherPlugins = AssemblyPatcher.PatcherPlugins.Except(patcherPlugins).ToList();
            Util.Logger.LogInfo($"Loading {newPatcherPlugins.Count} preloader patchers from mods...");
            InitializeModPatchers(newPatcherPlugins);
        }

        private static void InitializeModPatchers(List<PatcherPlugin> patchers)
        {
            foreach (var patcher in patchers)
            {
                try
                {
                    patcher.Initializer?.Invoke();
                }
                catch (Exception e)
                {
                    Util.Logger.LogError($"Failed to initialize patch [{patcher.TypeName}]: {e}");
                }
            }
        }
    }
}