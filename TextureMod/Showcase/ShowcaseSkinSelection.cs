using System;
using System.Collections.Generic;
using UnityEngine;
using LLScreen;
using LLHandlers;
using BepInEx.Logging;
using LLBML;
using LLBML.Players;
using TextureMod.CustomSkins;

namespace TextureMod.Showcase
{
    public static class ShowcaseSkinSelection
    {

        private static ManualLogSource Logger => TextureMod.Log;
        public static ScreenUnlocksSkins SUS => UIScreen.GetScreen(1) is ScreenUnlocksSkins sus ? sus : null;

        private static int skinCounter = 0; 


        public static void Update()
        {
            if (SUS == null) skinCounter = 0;
            else
            {
                if (TextureMod.IsSkinKeyDown())
                {
                    if (InputHandler.MouseOrTouchDown() || Input.GetKeyDown(KeyCode.T) || Controller.all.GetButtonDown(InputAction.SHRIGHT))
                    {
                        NextSkin();
                    }
                    else if (Input.GetKeyDown(KeyCode.T) || Controller.all.GetButtonDown(InputAction.SHLEFT))
                    {
                        PreviousSkin();
                    }
                }
            }
        }

        public static void NextSkin()
        {
            skinCounter++;
            ChangeSkin(skinCounter);
        }

        public static void PreviousSkin()
        {
            skinCounter--;
            ChangeSkin(skinCounter);
        }

        public static void ChangeSkin(int index)
        {
            // TODO Improve that
            List<CustomSkinHandler> skins = SkinsManager.skinCache.GetUsableHandlers(SUS.previewModel.character);
            if (skins == null) return;

            Logger.LogDebug($"Counter: {skinCounter}, skin length: {skins.Count}");
            if (skins.Count > 0)
            {
                ShowcaseStudio.Instance.SetCustomSkin(skins?[mod(index, skins.Count)]);
            }
        }
        private static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }


        private static void GetSkinForUnlocksModel(ScreenUnlocksCharacters screen, sbyte next = 0)
        {
            Character character = screen.previewModel.character;
            CharacterVariant characterVariant = screen.previewModel.characterVariant;

            List<CustomSkinHandler> customSkins = SkinsManager.skinCache[character];
            if (customSkins.Count == 0)
            {
                Logger.LogInfo($"No skins for {character}");
                return;
            }

            //localPlayer.customSkin = GetCustomSkin(character);
            /*screen.previewModel.SetCharacterResultScreen(0, character, GetCustomSkinVariant(localPlayer.customSkin.ModelVariant, characterVariant));
            silouetteTimer = 5;*/
        }

        private static void SetSkinForUnlocksModel(ScreenUnlocksSkins screenUnlocksSkins, sbyte next = 0)
        {

            Character susCharacter = screenUnlocksSkins.character;
            List<CustomSkinHandler> customSkins = SkinsManager.skinCache[susCharacter];
            if (customSkins.Count == 0)
            {
                Logger.LogInfo($"No skins for {susCharacter}");
                return;
            }

            //localPlayer.customSkin = GetCustomSkin(susCharacter);
            //screenUnlocksSkins.ShowCharacter(susCharacter, localPlayer.customSkin.CharacterVariant, true);
            /*silouetteTimer = 5;
            SetCharacterModelTex(screenUnlocksSkins.previewModel, localPlayer.customSkin?.Texture);
            */
        }
    }
}
