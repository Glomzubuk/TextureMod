using System;
using System.IO;
using UnityEngine;
using LLHandlers;
using BepInEx.Logging;
using LLBML;
using LLBML.States;
using LLBML.Networking;
using TextureMod.TMPlayer;

namespace TextureMod.CustomSkins
{
    public class SkinsManager : MonoBehaviour
    {
        private static ManualLogSource Logger => TextureMod.Log;

        public static CustomSkinCache skinCache;

        private static int reloadCustomSkinTimer = 0;


        public static void LoadLibrary()
        {
            try
            {
                skinCache.Clear();
                Resources.UnloadUnusedAssets();

                Logger.LogInfo("Loading Modding folder");
                skinCache.LoadSkins(TextureMod.ModdingFolder.CreateSubdirectory("Characters"));

                Logger.LogInfo("Loading Resources folder");
                var resourcesCharacterFolder = new DirectoryInfo(BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "Characters"));
                skinCache.LoadSkins(resourcesCharacterFolder);

                Logger.LogInfo("Loading Remote folder");
                var remoteCharacterFolder = new DirectoryInfo(BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "RemoteSkinCache"));
                skinCache.LoadSkins(resourcesCharacterFolder);
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

        private void FixedUpdate()
        {
            if (TextureMod.reloadCustomSkinOnInterval.Value && reloadCustomSkinTimer < TextureMod.skinReloadIntervalInFrames.Value)
            {
                reloadCustomSkinTimer++;
            }
        }
        private void Update()
        {

            CheckForSkinReload();
        }


        public static bool InPostGame()
        {
            return (StateApi.CurrentGameMode == GameMode._1v1 || StateApi.CurrentGameMode == GameMode.FREE_FOR_ALL || StateApi.CurrentGameMode == GameMode.COMPETITIVE)
                && GameStates.GetCurrent() == GameState.GAME_RESULT;
        }

        private void CheckForSkinReload()
        {
            if (Input.GetKeyDown(TextureMod.reloadCustomSkin.Value) && (StateApi.CurrentGameMode == GameMode.TRAINING || StateApi.CurrentGameMode == GameMode.TUTORIAL))
            {
                if ((GameStates.IsInLobby() || GameStates.IsInMatch() || InPostGame()) && !NetworkApi.IsOnline)
                {
                    ReloadCurrentSkins();
                    reloadCustomSkinTimer = 0;
                    return;
                }
            }
            if (reloadCustomSkinTimer >= TextureMod.skinReloadIntervalInFrames.Value)
            {
                ReloadCurrentSkins();
                reloadCustomSkinTimer = 0;
                return;
            }
        }

        public static void ReloadCurrentSkins()
        {
            TexModPlayerManager.ForAllLocalTexmodPlayers((tmp) =>
            {
                try { tmp.skinHandler.ReloadSkin(); }
                catch { AudioHandler.PlaySfx(Sfx.MENU_BACK); }
            });
        }
    }
}
