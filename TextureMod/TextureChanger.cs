using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using GameplayEntities;
using LLScreen;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LLBML;
using TextureMod.CustomSkins;

namespace TextureMod
{
    public class TextureChanger : MonoBehaviour
    {
        public static ManualLogSource Logger = TextureMod.Log;
        #region General Fields
        public static string imageFolder = Path.Combine(TextureMod.ResourceFolder, "Images");

        public string[] debug = new string[20];
        public bool doSkinPost = false;
        public int postTimer = 0;
        private int postTimerLimit = 30;

        private System.Random rng = new System.Random();
        private bool randomizedChar = false;
        private int silouetteTimer = 0;
        private int reloadCustomSkinTimer = 0;
        private bool intervalMode = false;
        private List<Character> playersInCurrentGame = new List<Character>();


        public Color32[] originalDNAColors = new Color32[BagPlayer.outfitOutlineColors.Length];

        #endregion

        public int localSkinIndex = -1;


        private void OnGUI()
        {
            //ShowSkinNametags();
        }

        private void FixedUpdate()
        {
            /*
            if (silouetteTimer > 0) silouetteTimer--;*/
        }


        CustomSkin GetCustomSkin(Character character, bool isRandom = false)
        {
            List<CustomSkinHandler> customSkins = SkinsManager.skinCache[character];
            if (customSkins.Count == 0)
            {
                Debug.Log($"[LLBMM] TextureMod: No skins for {character}");
                return null;
            }
            int skinIndex = localSkinIndex;

            if (skinIndex > customSkins.Count - 1)
            {
                skinIndex = 0;
            }
            else if (skinIndex < 0)
            {
                skinIndex = customSkins.Count - 1;
            }

            localSkinIndex = skinIndex;
            return customSkins[localSkinIndex].CustomSkin;
        }

        //TODO colision prevention
        /*
        CharacterVariant GetCustomSkinVariant(ModelVariant variantType, CharacterVariant characterVariant)
        {
            switch (variantType)
            {
                case ModelVariant.Alternative:
                    if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                    {
                        return opponentPlayer.CharacterVariant == CharacterVariant.MODEL_ALT ? CharacterVariant.MODEL_ALT2 : CharacterVariant.MODEL_ALT;
                    }
                    else return CharacterVariant.MODEL_ALT;
                case ModelVariant.DLC:
                    if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                    {
                        return opponentPlayer.CharacterVariant == CharacterVariant.MODEL_ALT3 ? CharacterVariant.MODEL_ALT4 : CharacterVariant.MODEL_ALT3;
                    }
                    else return CharacterVariant.MODEL_ALT3;
                default:
                    if (characterVariant < CharacterVariant.STATIC_ALT)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                    {
                        return opponentPlayer.CharacterVariant == CharacterVariant.DEFAULT ? CharacterVariant.ALT0 : CharacterVariant.DEFAULT;
                    }
                    else return CharacterVariant.DEFAULT;
            }
        }
        */

        private void GetSkinForUnlocksModel(ScreenUnlocksCharacters screen, sbyte next = 0)
        {
            Character character = screen.previewModel.character;
            CharacterVariant characterVariant = screen.previewModel.characterVariant;

            List <CustomSkinHandler> customSkins = SkinsManager.skinCache[character];
            if (customSkins.Count == 0)
            {
                Logger.LogInfo($"No skins for {character}");
                return;
            }

            //localPlayer.customSkin = GetCustomSkin(character);
            /*screen.previewModel.SetCharacterResultScreen(0, character, GetCustomSkinVariant(localPlayer.customSkin.ModelVariant, characterVariant));
            silouetteTimer = 5;*/
        }

        private void SetSkinForUnlocksModel(ScreenUnlocksSkins screenUnlocksSkins, sbyte next = 0)
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

