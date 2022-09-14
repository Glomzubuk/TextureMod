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


        //public static bool InMatch => World.instance != null && (DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.CDOFDJMLGLO || DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.LGILIJKMKOD) && !LLScreen.UIScreen.loadingScreenActive;
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
            if (!neverApplyOpponentsSkin.Value)
            {
                if (doSkinPost) { postTimer++; }
                if (localPlayer != null && localPlayer.customSkin != null)
                {
                    if (postTimer >= postTimerLimit)
                    {
                        doSkinPost = false;
                        postTimer = 0;
                        try
                        {
                            ExchangeClient.SendCustomSkinChange(localPlayer.customSkin.SkinHash);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error sending skin change to other players {ex.ToString()}");
                        }
                    }
                }
            }
            */
            /*
            if (silouetteTimer > 0) silouetteTimer--;
            if (reloadCustomSkinTimer > 0) reloadCustomSkinTimer--;*/
        } //POST and GET requests

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

                        if (localPlayer == null)
                        {
                            localPlayer = new TexModPlayer(Player.GetPlayer(0), null);
                        }

                        if (InLobby(GameType.Offline))
                        {
                            if (localPlayer.characterModel == null)
                            {
                                localPlayer.UpdateModel();
                            }
                        }
                        else if (InGame(GameType.Offline))
                        {
                            if (localPlayer.customSkin != null)
                            {
                                if (Input.GetKeyDown(reloadCustomSkin.Value))
                                {
                                    try { localPlayer.customSkin.ReloadSkin(); }
                                    catch { AudioHandler.PlaySfx(Sfx.MENU_BACK); }
                                }
                            }
                        }
                        break;
                    #endregion
                    case GameMode._1v1:
                    case GameMode.FREE_FOR_ALL:
                    case GameMode.COMPETITIVE:
                        #region In ranked and online lobby
                        if (InLobby(GameType.Any))
                        {
                            if (localPlayer == null)
                            {
                                localPlayer = new LocalTexModPlayer(Player.GetPlayer(NetworkApi.LocalPlayerNumber), null);
                            }
                            IEnumerable<Player> players = Player.GetPlayerList().Where((player) => player != null);
                            if (Opponents.Count != (players.Count() - 1))
                            {
                                foreach (Player player in players)
                                {
                                    if (player != null && player.nr != NetworkApi.LocalPlayerNumber)
                                    {
                                        tmPlayers[player.nr] = new RemoteTexModPlayer(player);
                                    }
                                }
                            }

                        }
                        break;
                }
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
#if showOLD

        [Obsolete("Method1 is deprecated, please use Method2 instead.", true)]
        private Texture2D GetLoadedTexture(Character c, Texture2D currentTexture, bool previous, bool random)
        {
            Texture2D ret = null;
            var texname = "";

            bool flipped;
            if (currentGameMode == GameMode._1v1 && localLobbyPlayer.CJFLMDNNMIE == 1) flipped = true;
            else flipped = false;


            if (random)
            {
                var n = rng.Next(0, TextureMod.Instance.tl.characterTextures[c].Count());
                localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key);
                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key), flipped);
                localPlayerCharVar = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key);
                texname = TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key;
                ret = TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Value;
            }
            else
            {
                if (!previous)
                {
                    if (currentTexture == null && TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value != null)
                    {
                        localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                        localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key), flipped);
                        localPlayerCharVar = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                        texname = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key;
                        ret = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value;
                    }
                    else
                    {
                        bool retnext = false;
                        foreach (KeyValuePair<string, Texture2D> pair in TextureMod.Instance.tl.characterTextures[c])
                        {
                            if (retnext == true)
                            {
                                localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, pair.Key);
                                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, pair.Key), flipped);
                                localPlayerCharVar = GetVariantFromFileName(c, pair.Key);
                                texname = pair.Key;
                                ret = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == TextureMod.Instance.tl.characterTextures[c].Last().Value)
                                {
                                    localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                                    localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key), flipped);
                                    localPlayerCharVar = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                                    texname = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key;
                                    ret = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (currentTexture == null && TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value != null)
                    {
                        KeyValuePair<string, Texture2D> lastSkin = TextureMod.Instance.tl.characterTextures[c].Last();
                        localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, lastSkin.Key);
                        localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, lastSkin.Key), flipped);
                        localPlayerCharVar = GetVariantFromFileName(c, lastSkin.Key);
                        texname = lastSkin.Key;
                        ret = lastSkin.Value;
                    }
                    else
                    {
                        bool retnext = false;
                        KeyValuePair<string, Texture2D> firstSkin = TextureMod.Instance.tl.characterTextures[c].First();
                        KeyValuePair<string, Texture2D> lastSkin = TextureMod.Instance.tl.characterTextures[c].Last();

                        for (int i = TextureMod.Instance.tl.characterTextures[c].Count - 1; i != -1; i--)
                        {
                            KeyValuePair<string, Texture2D> pair = TextureMod.Instance.tl.characterTextures[c].ElementAt(i);
                            if (retnext == true)
                            {
                                localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, pair.Key);
                                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, pair.Key), flipped);
                                localPlayerCharVar = GetVariantFromFileName(c, pair.Key);
                                texname = pair.Key;
                                ret = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == firstSkin.Value)
                                {
                                    localLobbyPlayer.CharacterVariant = GetVariantFromFileName(c, lastSkin.Key);
                                    localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, lastSkin.Key), flipped);
                                    localPlayerCharVar = GetVariantFromFileName(c, lastSkin.Key);
                                    texname = lastSkin.Key;
                                    ret = lastSkin.Value;
                                }
                            }
                        }
                    }
                }
            }
            localPlayerChar = c;
            texname = CleanTextureName(texname);
            localSkinNameLabel = texname;
            return ret;
        }

#endif

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

#if showOLD
        [Obsolete("Method1 is deprecated, please use Method2 instead.", true)]
        private Texture2D GetLoadedTextureForUnlocksModel(Texture2D currentTexture, bool previous)
        {
            Texture2D texture = null;
            var texname = "";
            ScreenUnlocksSkins sus = FindObjectOfType<ScreenUnlocksSkins>();

            if (sus != null)
            {
                var curCharactersTextures = TextureMod.Instance.tl.characterTextures[sus.character];
                if (previous)
                {
                    if (currentTexture == null && curCharactersTextures.ElementAt(0).Value != null)
                    {
                        sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, curCharactersTextures.Last().Key), false);
                        texname = curCharactersTextures.Last().Key;
                        texture = curCharactersTextures.Last().Value;
                    }
                    else
                    {
                        bool retnext = false;
                        for (int i = curCharactersTextures.Count - 1; i != -1; i--)
                        {
                            KeyValuePair<string, Texture2D> pair = curCharactersTextures.ElementAt(i);
                            if (retnext == true)
                            {
                                sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, pair.Key), false);
                                texname = pair.Key;
                                texture = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == curCharactersTextures.First().Value)
                                {
                                    sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, curCharactersTextures.Last().Key), false);
                                    texname = curCharactersTextures.Last().Key;
                                    texture = curCharactersTextures.Last().Value;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (currentTexture == null && curCharactersTextures.ElementAt(0).Value != null)
                    {
                        sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, curCharactersTextures.ElementAt(0).Key), false);
                        texname = curCharactersTextures.ElementAt(0).Key;
                        texture = curCharactersTextures.ElementAt(0).Value;
                    }
                    else
                    {
                        bool retnext = false;
                        foreach (KeyValuePair<string, Texture2D> pair in curCharactersTextures)
                        {
                            if (retnext == true)
                            {
                                sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, pair.Key), false);
                                texname = pair.Key;
                                texture = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == curCharactersTextures.Last().Value)
                                {
                                    sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, curCharactersTextures.ElementAt(0).Key), false);
                                    texname = curCharactersTextures.ElementAt(0).Key;
                                    texture = curCharactersTextures.ElementAt(0).Value;
                                }
                            }
                        }
                    }
                }
            }

            texname = CleanTextureName(texname);

            siluetteTimer = 5;

            localSkinNameLabel = texname;
            return texture;
        }

#endif


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

