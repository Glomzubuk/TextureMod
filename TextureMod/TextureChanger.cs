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
        #region Config Fields
        public ConfigEntry<KeyCode> holdKey1;
        public ConfigEntry<KeyCode> nextSkin;
        public ConfigEntry<KeyCode> previousSkin;
        public ConfigEntry<KeyCode> cancelKey;
        public ConfigEntry<KeyCode> reloadCustomSkin;
        public ConfigEntry<KeyCode> reloadEntireSkinLibrary;
        public ConfigEntry<bool> useOnlySetKey;
        public ConfigEntry<bool> neverApplyOpponentsSkin;
        public ConfigEntry<bool> showDebugInfo;
        public ConfigEntry<bool> lockButtonsOnRandom;
        public ConfigEntry<bool> reloadCustomSkinOnInterval;
        public ConfigEntry<int> skinReloadIntervalInFrames;
        public ConfigEntry<bool> assignFirstSkinOnCharacterSelection;
        #endregion

        public int localSkinIndex = -1;

        private void Start()
        {
            InitConfig();
        }

        private void OnGUI()
        {
            //ShowSkinNametags();
        }

        private void FixedUpdate()
        {
            /*
            if (silouetteTimer > 0) silouetteTimer--;
            if (reloadCustomSkinTimer > 0) reloadCustomSkinTimer--;*/
        }

        private void Update()
        {


            InLobbyOrGameChecks();
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(reloadEntireSkinLibrary.Value))
            {
                TextureMod.Instance.tl.LoadLibrary(); //Reloads the entire texture folder
            }
        }





        void InLobbyOrGameChecks()
        {/*
            if (InLobby(GameType.Any) || InGame(GameType.Any) || InPostGame())
            {
                switch (currentGameMode)
                {
                    case GameMode.TRAINING:
                    case GameMode.TUTORIAL:
                        #region In training and tutorial
                        else if (InGame(GameType.Offline))
                        {
                            if (localPlayer.customSkin != null)
                            {
                                // TODO Reloading the skin library                           
                                if (Input.GetKeyDown(reloadCustomSkin.Value))
                                {
                                    try { localPlayer.customSkin.ReloadSkin(); }
                                    catch { AudioHandler.PlaySfx(Sfx.MENU_BACK); }
                                }
                            }
                        }
                        break;
                    #endregion
            }
            */
        }

        
        public static bool InPostGame()
        {
            return (StateApi.CurrentGameMode == GameMode._1v1 || StateApi.CurrentGameMode == GameMode.FREE_FOR_ALL || StateApi.CurrentGameMode == GameMode.COMPETITIVE)
                && LLBML.States.GameStates.GetCurrent() == LLBML.States.GameState.GAME_RESULT;
        }


        CustomSkin GetCustomSkin(Character character, bool isRandom = false)
        {
            List<CustomSkinHandler> customSkins = TextureMod.customSkinCache[character];
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
            Traverse tv_previewModel = Traverse.Create(screen.previewModel);
            Character character = tv_previewModel.Field<Character>("character").Value;
            CharacterVariant characterVariant = tv_previewModel.Field<CharacterVariant>("characterVariant").Value;

            List <CustomSkinHandler> customSkins = TextureMod.customSkinCache[character];
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

            Character susCharacter = Traverse.Create(screenUnlocksSkins).Field<Character>("character").Value;
            List<CustomSkinHandler> customSkins = TextureMod.customSkinCache[susCharacter];
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




        private void InitConfig()
        {

            ConfigFile config = TextureMod.Instance.Config;

            config.Bind("TextureChanger", "lobby_settings_header", "Lobby Settings:", "modmenu_header");
            holdKey1 = config.Bind<KeyCode>("TextureChanger", "holdKey1", KeyCode.LeftShift);
            nextSkin = config.Bind<KeyCode>("TextureChanger", "nextSkin", KeyCode.Mouse0);
            previousSkin = config.Bind<KeyCode>("TextureChanger", "previousSkin", KeyCode.Mouse1);
            cancelKey = config.Bind<KeyCode>("TextureChanger", "cancelKey", KeyCode.A);
            useOnlySetKey = config.Bind<bool>("TextureChanger", "useOnlySetKey", false);
            neverApplyOpponentsSkin = config.Bind<bool>("TextureChanger", "neverApplyOpponentsSkin", false);
            lockButtonsOnRandom = config.Bind<bool>("TextureChanger", "lockButtonsOnRandom", false);
            assignFirstSkinOnCharacterSelection = config.Bind<bool>("TextureChanger", "assignFirstSkinOnCharacterSelection", false);
            config.Bind("TextureChanger", "gap1", "20", "modmenu_gap");

            config.Bind("TextureChanger", "rt_skin_edit_header", "Real-time Skin editing:", "modmenu_header");
            reloadCustomSkin = config.Bind<KeyCode>("TextureChanger", "reloadCustomSkin", KeyCode.F5);
            reloadEntireSkinLibrary = config.Bind<KeyCode>("TextureChanger", "reloadEntireSkinLibrary", KeyCode.F9);
            reloadCustomSkinOnInterval = config.Bind<bool>("TextureChanger", "reloadCustomSkinOnInterval", true);
            skinReloadIntervalInFrames = config.Bind<int>("TextureChanger", "skinReloadIntervalInFrames", 60);
            config.Bind("TextureChanger", "gap2", "20", "modmenu_gap");

            config.Bind("TextureChanger", "general_header", "General:", "modmenu_header");
            showDebugInfo = config.Bind<bool>("TextureChanger", "showDebugInfo", false);
            config.Bind("TextureChanger", "gap3", "20", "modmenu_gap");
        }
    }
}

