using System.Collections.Generic;
using Mono.Cecil;

namespace BepInEx.MultiFolderLoader
{
    public static class MultiLoaderEntrypoint
    {
        // Add dummy property to fulfil the preloader patcher contract
        public static IEnumerable<string> TargetDLls => new string[0];

        public static void Initialize()
        {
            ModManager.Init();
            PreloaderHandler.Init();
        }

        public static void Finish()
        {
            // Hook chainloader only after preloader to not cause resolving on UnityEngine too soon
            ChainloaderHandler.Init();
        }

        public static void Patch(AssemblyDefinition ass)
        {
            // Not used, exists so that this works as preloader patch
        }
    }
}