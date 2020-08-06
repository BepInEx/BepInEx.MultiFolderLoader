using System.IO;
using BepInEx.Logging;

namespace BepInEx.MultiFolderLoader
{
    public static class Util
    {
        /// <summary>
        ///     The actual path to the folder with the mods.
        ///     Used in ModManager to load actual mods.
        ///     Note that at this point of execution mono is just initialized so some methods might not work.
        ///     In addition, you shouldn't reference Assembly-CSharp here as it can trigger assembly loading which will break
        ///     any preloader patchers.
        /// </summary>
        public static readonly string ModsPath = Path.Combine(Paths.GameRootPath, "Mods");

        public static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("LoaderPrototype");
    }
}