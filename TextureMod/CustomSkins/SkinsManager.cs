using System;
using System.IO;
using UnityEngine;
using BepInEx.Logging;

namespace TextureMod.CustomSkins
{
    public class SkinsManager : MonoBehaviour
    {
        private static ManualLogSource Logger => TextureMod.Log;

        public static CustomSkinCache skinCache = new CustomSkinCache();

        public static void LoadLibrary()
        {
            try
            {
                skinCache.Clear();
                Resources.UnloadUnusedAssets();

                var moddingFolder = TextureMod.ModdingFolder.CreateSubdirectory("Characters");
                Logger.LogInfo($"Loading Modding folder at: {moddingFolder}");
                skinCache.LoadSkins(moddingFolder);

                var resourcesCharacterFolder = new DirectoryInfo(BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "Characters"));
                Logger.LogInfo($"Loading Resources folder at: {resourcesCharacterFolder}");
                if (!resourcesCharacterFolder.Exists) resourcesCharacterFolder.Create();
                skinCache.LoadSkins(resourcesCharacterFolder);

                var remoteCharacterFolder = new DirectoryInfo(BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "RemoteSkinCache"));
                Logger.LogInfo($"Loading Remote folder at: {remoteCharacterFolder}");
                if (!remoteCharacterFolder.Exists) remoteCharacterFolder.Create();
                skinCache.LoadSkins(remoteCharacterFolder);
            }
            catch (Exception e)
            {
                TextureMod.loadingText = $"TextureMod failed to load textures.";
                Logger.LogError($"TextureMod failed to load textures: {e}");
                throw e;
            }
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(TextureMod.reloadEntireSkinLibrary.Value))
            {
                SkinsManager.LoadLibrary(); //Reloads the entire texture folder
            }
        }
    }
}
