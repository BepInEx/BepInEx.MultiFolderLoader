using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using Mono.Cecil;

namespace BepInEx.MultiFolderLoader
{
    public static class ChainloaderHandler
    {
        private static bool shouldSaveCache = true;

        public static void Init()
        {
            TypeLoader.AssemblyResolve += TypeLoaderOnAssemblyResolve;
            var instance = new Harmony("org.bepinex.loaderprototype.chainloaderhandler");
            instance.Patch(
                AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.FindPluginTypes))
                    .MakeGenericMethod(typeof(PluginInfo)),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(PreFindPluginTypes))),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(PostFindPluginTypes))));

            instance.Patch(
                AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.SaveAssemblyCache))
                    .MakeGenericMethod(typeof(PluginInfo)),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(OnSaveAssemblyCache))));
        }

        private static AssemblyDefinition TypeLoaderOnAssemblyResolve(object sender, AssemblyNameReference reference)
        {
            var name = new AssemblyName(reference.FullName);
            foreach (var pluginDir in ModManager.GetPluginDirs())
                if (Utility.TryResolveDllAssembly(name, pluginDir, TypeLoader.ReaderParameters, out var assembly))
                    return assembly;
            return null;
        }

        private static void PreFindPluginTypes(string directory)
        {
            if (directory != Paths.PluginPath)
                return;
            // prevent saving cache in order to not overwrite it when loading all mods 
            shouldSaveCache = false;
        }

        private static void PostFindPluginTypes(Dictionary<string, List<PluginInfo>> __result, string directory,
            Func<TypeDefinition, PluginInfo> typeSelector, Func<AssemblyDefinition, bool> assemblyFilter,
            string cacheName)
        {
            // Prevent recursion
            if (directory != Paths.PluginPath)
                return;

            MultiFolderLoader.Logger.LogInfo("Finding plugins from mods...");
            foreach (var pluginDir in ModManager.GetPluginDirs())
            {
                var result = TypeLoader.FindPluginTypes(pluginDir, typeSelector, assemblyFilter, cacheName);
                foreach (var kv in result)
                    __result[kv.Key] = kv.Value;
            }

            shouldSaveCache = true;
            if (cacheName != null)
                TypeLoader.SaveAssemblyCache(cacheName, __result);
        }

        private static bool OnSaveAssemblyCache()
        {
            return shouldSaveCache;
        }
    }
}