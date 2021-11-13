using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameplayEntities;
using LLGUI;
using LLHandlers;
using LLScreen;
using Multiplayer;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LLBML;
using LLBML.Players;
using LLBML.Math;
using LLBML.States;
using TextureMod.TMPlayer;

namespace TextureMod
{
    public class TextureChanger : MonoBehaviour
    {
        public static ManualLogSource Logger = TextureMod.Log;
        #region General Fields
        public static string imageFolder = Path.Combine(TextureMod.ResourceFolder, "Images");

        private JOFJHDJHJGI gameState => GameStates.GetCurrent();
        private GameMode currentGameMode => StateApi.CurrentGameMode;
        private bool IsOnline => NetworkApi.IsOnline;

        public static bool InMatch => World.instance != null && (DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.CDOFDJMLGLO || DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.LGILIJKMKOD) && !LLScreen.UIScreen.loadingScreenActive;
        public string[] debug = new string[20];
        public bool doSkinPost = false;
        public int postTimer = 0;
        private int postTimerLimit = 30;

        private bool doMirrorCheck = true;

        private System.Random rng = new System.Random();
        private bool randomizedChar = false;
        private int silouetteTimer = 0;
        private int reloadCustomSkinTimer = 0;
        private bool intervalMode = false;
        private List<Character> playersInCurrentGame = new List<Character>();


        public Color32[] originalDNAColors = new Color32[BagPlayer.outfitOutlineColors.Length];

        #endregion
        #region Config Fields
        private ConfigEntry<KeyCode> holdKey1;
        private ConfigEntry<KeyCode> nextSkin;
        private ConfigEntry<KeyCode> previousSkin;
        private ConfigEntry<KeyCode> cancelKey;
        public ConfigEntry<KeyCode> reloadCustomSkin;
        public ConfigEntry<KeyCode> reloadEntireSkinLibrary;
        private ConfigEntry<bool> useOnlySetKey;
        private ConfigEntry<bool> neverApplyOpponentsSkin;
        public ConfigEntry<bool> showDebugInfo;
        private ConfigEntry<bool> lockButtonsOnRandom;
        public ConfigEntry<bool> reloadCustomSkinOnInterval;
        public ConfigEntry<int> skinReloadIntervalInFrames;
        public ConfigEntry<bool> assignFirstSkinOnCharacterSelection;
        #endregion

        public List<TexModPlayer> tmPlayers = new List<TexModPlayer>(Player.MAX_PLAYERS);

        public List<OpponentTexModPlayer> Opponents {
            get
            {
                return tmPlayers.Where((tmPlayer) => tmPlayer != null && tmPlayer.GetType() == typeof(OpponentTexModPlayer)).Cast<OpponentTexModPlayer>().ToList();
            }
        }

        public TexModPlayer localPlayer {
            get { return tmPlayers[NetworkApi.LocalPlayerNumber]; }
            set { tmPlayers[NetworkApi.LocalPlayerNumber] = value; }
        }
        public int localSkinIndex = -1;

        private void Start()
        {
            InitConfig();
        }

        private void OnGUI()
        {
            ShowSkinNametags();
        }

        private void FixedUpdate()
        {
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
                            //StartCoroutine(TextureMod.Instance.ec.PostSkin(LocalLobbyPlayer.Player.peer.peerId, localPlayerChar, localPlayerCharVar, localCustomSkin.Texture));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error sending skin change to other players {ex.ToString()}");
                        }
                    }
                }
            }

            if (silouetteTimer > 0) silouetteTimer--;
            if (reloadCustomSkinTimer > 0) reloadCustomSkinTimer--;
        } //POST and GET requests

        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                InitLocalPlayer();
                InitOpponentPlayer();
            } 
#endif

            DebugOptions();
            /*
            if (localPlayer != null)
            {
                localPlayer.Update();
            }
            else
            {
                InitLocalPlayer();
            }
            */
            foreach (TexModPlayer tmPlayer in tmPlayers)
            {
                if (tmPlayer != null)
                {
                    tmPlayer.Update();
                }
            }

            DisableCharacterButtons();

            CheckMirror();

            InLobbyOrGameChecks();

            UpdateCustomSkinsInMenu();
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(reloadEntireSkinLibrary.Value))
            {
                TextureMod.Instance.tl.LoadLibrary(); //Reloads the entire texture folder
            }
        }


        private void ShowSkinNametags()
        {
            if (localPlayer.customSkin != null) //Show skin nametags
            {
                string labelTxt = localPlayer.customSkin.GetSkinLabel();
                GUI.skin.box.wordWrap = false;
                GUIContent content;
                if (!intervalMode) content = new GUIContent(labelTxt);
                else content = new GUIContent(labelTxt + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.skin.box.fontSize = 22;

                ScreenBase screenOne = ScreenApi.CurrentScreens[1];

                if (InLobby(GameType.Any))
                {
                    if (screenOne == null)
                    {
                        switch (currentGameMode)
                        {
                            case GameMode.TUTORIAL:
                            case GameMode.TRAINING:
                                GUI.Box(new Rect((Screen.width / 8), (Screen.height / 12.5f), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                            case GameMode._1v1:
                                if (localPlayer.Player.nr == 0) GUI.Box(new Rect(Screen.width / 10, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt); //Check if local player is the player with ID 0
                                else GUI.Box(new Rect((Screen.width / 20) * 12.95f, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                            case GameMode.FREE_FOR_ALL:
                            case GameMode.COMPETITIVE:
                                if (localPlayer.Player.nr == 0) GUI.Box(new Rect(0 + Screen.width / 250, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                else GUI.Box(new Rect((Screen.width / 4) + (Screen.width / 250), Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                        }
                    }
                }

                if (screenOne != null)
                {
                    if (screenOne.screenType == ScreenType.UNLOCKS_SKINS)
                    {
                        if (TextureMod.Instance.showcaseStudio.showUI == false)
                        {
                            TextureMod.Instance.showcaseStudio.skinName = labelTxt;
                            TextureMod.Instance.showcaseStudio.refreshTimer = reloadCustomSkinTimer;
                            TextureMod.Instance.showcaseStudio.refreshMode = intervalMode;
                        }
                        else
                        {
                            if (intervalMode) GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                            else GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                        }
                    }
                }
            }  //Show skin nametags
        }


        private bool OnSkinChangeButtonDown()
        {
            if (Input.GetKeyDown(nextSkin.Value) || Controller.all.GetButtonDown(InputAction.EXPRESS_RIGHT))
            {
                localSkinIndex++;
                return true;
            }
            else if (Input.GetKeyDown(previousSkin.Value) || Controller.all.GetButtonDown(InputAction.EXPRESS_LEFT))
            {
                localSkinIndex--;
                return true;
            }
            else return false;
        }



        void DisableCharacterButtons()
        {
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (randomizedChar && screenOne != null) // If you have randomized your character, activate buttons again
            {
                if (screenOne.screenType == ScreenType.PLAYERS_STAGE || screenOne.screenType == ScreenType.PLAYERS_STAGE_RANKED)
                {
                    LLButton[] buttons = FindObjectsOfType<LLButton>();
                    foreach (LLButton b in buttons) b.SetActive(true);
                }
            }
        }
        /*
        void CheckMirror()
        {
            if (InLobby(GameType.Any) && doMirrorCheck)
            {
                if (SingleOpponent != null)
                {
                    OpponentTexModPlayer opponent = opponents[0];
                    if (localPlayer.customSkin.SkinHash == SingleOpponent.customSkin.SkinHash)
                    {
                        SingleOpponent.SetColorFilter(SkinColorFilter.GRAY);
                    }
                } //Check if your skin matches your opponents, and if it does set theirs to grayscale
                else if (opponents.Count > 1)
                {
                    foreach (TexModPlayer player in opponents.Where((player) => player.customSkin != null))
                    {
                        throw new NotImplementedException("Coming later");
                    }
                }
                doMirrorCheck = false;
            }
        }*/


        private void ForAllTexModPlayersInMatch(Action<TexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in tmPlayers)
            {
                if (tmPlayer.Player.IsInMatch) action(tmPlayer);
            }
        }

        void InLobbyOrGameChecks()
        {
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
                        /*
                        if (Input.GetKeyDown(cancelKey.Value))
                        {
                            cancelOpponentSkin = !cancelOpponentSkin;
                            if (opponentLobbyCharacterModel != null)
                            {
                                opponentCustomTexture = null;
                                opponentCustomSkinCharacter = opponentPlayer.CharacterSelected;
                                opponentCustomSkinCharacterVariant = opponentPlayer.CharacterVariant;
                                if (currentGameMode == GameMode._1v1 && opponentPlayer.CJFLMDNNMIE == 1) opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentCustomSkinCharacter, CharacterVariant.CORPSE, true);
                                else opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentCustomSkinCharacter, CharacterVariant.CORPSE, false);
                                initOpponentPlayer = true;
                            }
                        }
                        */
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
                                        tmPlayers[player.nr] = new OpponentTexModPlayer(player);
                                    }
                                }
                            }

                            if (InLobby(GameType.Online) && localPlayer?.Player.DidJoinedMatch != null)
                            {
                                /*
                                if (opponentPlayer == null)
                                {
                                    opponentPlayer = GetOpponentPlayerInLobby();
                                }
                                else
                                {
                                    doSkinGet = true;
                                    if (opponentLobbyCharacterModel == null)
                                    {
                                        opponentLobbyCharacterModel = GetCurrentCharacterModel(opponentPlayer.CJFLMDNNMIE);
                                    }
                                    else
                                    {
                                        opponentLobbyCharacterModel.SetSilhouette(false);
                                        if (opponentCustomTexture != null)
                                        {
                                            AssignTextureToCharacterModelRenderers(opponentLobbyCharacterModel, opponentCustomTexture);
                                        }
                                    }
                                }
                                */
                            }

                        }
                        else if (InGame(GameType.Any))
                        {
                            /*
                            if (InGame(GameType.Online))
                            {
                                if (opponentCustomTexture != null)
                                {
                                    if (opponentPlayerEntity == null) { opponentPlayerEntity = GetOpponentPlayerInGame(); }
                                    else
                                    {
                                        AssignTextureToIngameCharacter(opponentPlayerEntity, opponentCustomTexture);
                                        opponentPlayerNr = AssignTextureToHud(opponentPlayerEntity, opponentCustomTexture);
                                    }
                                }
                            }
                            */
                        }
                        /*
                        else if (InPostGame())
                        {
                        }
                        else
                        {
                            if (localLobbyPlayer?.NGLDMOLLPLK == false)
                            {
                                initLocalPlayer = true;
                            }

                            initOpponentPlayer = true;
                        }*/
                        break;
                }
                #endregion
            }
        }

        public static bool InPostGame()
        {
            return (StateApi.CurrentGameMode == GameMode._1v1 || StateApi.CurrentGameMode == GameMode.FREE_FOR_ALL || StateApi.CurrentGameMode == GameMode.COMPETITIVE)
                && LLBML.States.GameStates.GetCurrent() == LLBML.States.GameState.GAME_RESULT;
        }

        private bool HandleSwitchSkinInputs()
        {
            LLButton[] buttons = FindObjectsOfType<LLButton>();

            if (useOnlySetKey.Value == false)
            {
                if (Input.GetKey(holdKey1.Value) && buttons.Length > 0)
                {
                    if (OnSkinChangeButtonDown())
                    {
                        return true;
                    }

                    foreach (LLButton b in buttons)
                    {
                        b.SetActive(false); //Deactivate buttons
                    }
                }
                else if (Input.GetKeyUp(holdKey1.Value) && buttons.Length > 0)
                {
                    foreach (LLButton b in buttons) b.SetActive(true); //Reactivate buttons
                }
            }
            else if (OnSkinChangeButtonDown())
            {
                return true;
            }
            return false;
        }



        void UpdateInMenu()
        {
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (InMenu())
            {

                if (screenOne?.screenType != ScreenType.UNLOCKS_SKINS && tmPlayers.Count > 0)
                {
                    tmPlayers.Clear();
                }

                if (initOpponentPlayer == true)
                {
                    InitOpponentPlayer();
                }
            }

            if (screenOne != null)
            {
                if (screenOne?.screenType == ScreenType.UNLOCKS_SKINS)
                {
                    var screenUnlocksSkins = screenOne as ScreenUnlocksSkins;

                    CharacterModel characterModel = screenUnlocksSkins.previewModel;
                    if (silouetteTimer > 0)
                    {
                        if (localPlayer.customSkin != null)
                        {
                            characterModel.SetSilhouette(false);
                            AssignTextureToCharacterModelRenderers(characterModel, localPlayer.customSkin.Texture);
                        }
                    }

                    if (Input.GetKey(holdKey1.Value))
                    {
                        if (OnSkinChangeButtonDown())
                        {
                            SetSkinForUnlocksModel(screenUnlocksSkins);
                        }
                    }

                    if (localCustomSkin != null) // Reload a skin from its file
                    {
                        if (Input.GetKeyDown(reloadCustomSkin.Value))
                        {
                            if (!intervalMode)
                            {
                                if (reloadCustomSkinOnInterval.Value)
                                {
                                    intervalMode = true;
                                    reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                                }
                            }
                            else intervalMode = false;

                            try
                            {
                                localCustomSkin.ReloadSkin();
                                //localTex = TextureHelper.ReloadSkin(screenUnlocksSkins.character, localTex);
                                SetUnlocksCharacterModel(localCustomSkin.Texture);
                                LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_CONFIRM);
                            }
                            catch { LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_BACK); }
                        }

                        if (intervalMode)
                        {
                            if (reloadCustomSkinTimer == 0)
                            {
                                try
                                {
                                    localCustomSkin.ReloadSkin();
                                    //localTex = TextureHelper.ReloadSkin(screenUnlocksSkins.character, localTex);
                                    SetUnlocksCharacterModel(localCustomSkin.Texture);
                                }
                                catch { LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_BACK); }
                                reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                            }
                        }
                    }
                }
                else if (screenOne?.screenType == ScreenType.UNLOCKS_CHARACTERS)
                {
                    localCustomSkin = null;
                    intervalMode = false;
                    reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                }
            }

        }

        /// End of Update()

        public List<Character> GetCharactersInGame()
        {
            List<Character> chars = new List<Character>();
            PlayerEntity[] playerEntities = FindObjectsOfType<PlayerEntity>();
            foreach (PlayerEntity pe in playerEntities)
            {
                if (!chars.Contains(pe.character)) chars.Add(pe.character);
            }
            return chars;
        }

        public bool InMenu()
        {
            if (ScreenApi.CurrentScreens[0]?.screenType == ScreenType.MENU)
            {
                return true;
            }
            else return false;
        }

        public bool InLobby(GameType gt)
        {
            switch (gt)
            {
                case GameType.Online:
                    return gameState == (JOFJHDJHJGI)GameState.LOBBY_ONLINE && UIScreen.loadingScreenActive == false;
                case GameType.Offline:
                    return (gameState == (JOFJHDJHJGI)GameState.LOBBY_TRAINING || gameState == (JOFJHDJHJGI)GameState.LOBBY_TUTORIAL || gameState == (JOFJHDJHJGI)GameState.LOBBY_LOCAL || gameState == (JOFJHDJHJGI)GameState.LOBBY_CHALLENGE || gameState == (JOFJHDJHJGI)GameState.LOBBY_STORY) && UIScreen.loadingScreenActive == false;
                case GameType.Any:
                    return (gameState == (JOFJHDJHJGI)GameState.LOBBY_ONLINE || gameState == (JOFJHDJHJGI)GameState.LOBBY_TRAINING || gameState == (JOFJHDJHJGI)GameState.LOBBY_TUTORIAL || gameState == (JOFJHDJHJGI)GameState.LOBBY_LOCAL || gameState == (JOFJHDJHJGI)GameState.LOBBY_CHALLENGE || gameState == (JOFJHDJHJGI)GameState.LOBBY_STORY) && UIScreen.loadingScreenActive == false;
            }
            return false;
        }

        public bool InGame(GameType gt)
        {
            switch (gt)
            {
                case GameType.Online:
                    return InMatch && IsOnline == true;
                case GameType.Offline:
                    return InMatch && IsOnline == false;
                case GameType.Any:
                    return InMatch;
                default:
                    return false;
            }
        }

        private ALDOKEMAOMB GetLocalPlayerInLobby(GameType gt)
        {
            ALDOKEMAOMB player = null;
            switch (gt)
            {
                case GameType.Online:
                    if (P2P.localPeer != null)
                    {
                        int nr = P2P.localPeer.playerNr;
                        Debug.Log($"Assigned player nr [{nr}] as the local player");
                        return ALDOKEMAOMB.BJDPHEHJJJK(nr);
                    }
                    break;
                case GameType.Offline:
                    return ALDOKEMAOMB.BJDPHEHJJJK(0);
            }
            return player;
        }

        private Player GetOpponentPlayerInLobby()
        {
            Player player = null;
            if (localPlayer.Player != null)
            {
                if (localPlayer.Player.nr == 0)
                {
                    player = Player.GetPlayer(1);
                }
                else if (localPlayer.Player.nr == 1)
                {
                    player = Player.GetPlayer(0);
                }
            }

            if (player.CharacterSelected != Character.NONE && player.CharacterSelected != Character.RANDOM) return player;
            else return null;
        }

        CustomSkin GetCustomSkin(Character character, bool isRandom = false)
        {
            List<CustomSkin> customSkins = TextureMod.Instance.tl.newCharacterTextures[character];
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
            return customSkins[localSkinIndex];
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

            List <CustomSkin> customSkins = TextureMod.Instance.tl.newCharacterTextures[character];
            if (customSkins.Count == 0)
            {
                Debug.Log($"[LLBMM] TextureMod: No skins for {character}");
                return;
            }

            localPlayer.customSkin = GetCustomSkin(character);
            screen.previewModel.SetCharacterResultScreen(0, character, GetCustomSkinVariant(localPlayer.customSkin.ModelVariant, characterVariant));
            silouetteTimer = 5;
        }

        private void SetSkinForUnlocksModel(ScreenUnlocksSkins screenUnlocksSkins, sbyte next = 0)
        {

            Character susCharacter = Traverse.Create(screenUnlocksSkins).Field<Character>("character").Value;
            List<CustomSkin> customSkins = TextureMod.Instance.tl.newCharacterTextures[susCharacter];
            if (customSkins.Count == 0)
            {
                Debug.Log($"[LLBMM] TextureMod: No skins for {susCharacter}");
                return;
            }

            localPlayer.customSkin = GetCustomSkin(susCharacter);
            screenUnlocksSkins.ShowCharacter(susCharacter, localPlayer.customSkin.CharacterVariant, true);
            silouetteTimer = 5;
            SetCharacterModelTex(screenUnlocksSkins.previewModel, localPlayer.customSkin?.Texture);
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



        void DebugOptions()
        {
            #region Set Debug Vars
            if (showDebugInfo.Value)
            {
                ModDebugging md = TextureMod.Instance.md;
                try { md.AddToWindow("General", "Gamestate", gameState.ToString()); } catch { }
                try { md.AddToWindow("General", "GameMode", currentGameMode.ToString()); } catch { }
                try { md.AddToWindow("General", "In Menu", InMenu().ToString()); } catch { }
                try { md.AddToWindow("General", "In Lobby", InLobby(GameType.Any).ToString()); } catch { }
                try { md.AddToWindow("General", "In Game", InGame(GameType.Any).ToString()); } catch { }
                try { md.AddToWindow("General", "In Post Game", InPostGame().ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[0]", ScreenApi.CurrentScreens[0]?.screenType.ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[1]", ScreenApi.CurrentScreens[1]?.screenType.ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[2]", ScreenApi.CurrentScreens[2]?.screenType.ToString()); } catch { }

                try { md.AddToWindow("Skin Exchange", "Do Skin Post", doSkinPost.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Post Timer", postTimer.ToString()); } catch { }
                /*
                try { md.AddToWindow("Skin Exchange", "Do Skin Get", doSkinGet.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Get Timer", getTimer.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "New Skin To Apply", newSkinToApply.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Set Anti Mirror", setAntiMirrior.ToString()); } catch { }
                */               

                try { md.AddToWindow("Local Player", "Lobby Player", localPlayer != null ? localPlayer.Player.ToString() : "null"); } catch { }
                try { md.AddToWindow("Local Player", "Lobby Skin Index", localSkinIndex.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Lobby Player Model", localPlayer.characterModel != null ? localPlayer.characterModel.ToString() : "null"); } catch { }
                try { md.AddToWindow("Local Player", "Game PlayerEntity", localPlayer.PlayerEntity != null ? localPlayer.PlayerEntity.ToString() : "null"); } catch { }
                try { md.AddToWindow("Local Player", "Name", localPlayer.Player.nr.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Character", localPlayer.Player.Character.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Variant", localPlayer.Player.CharacterVariant.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Custom Texture", localPlayer.customSkin != null ? localPlayer.customSkin.ToString() : "null"); } catch { }
                try { md.AddToWindow("Local Player", "Initiate Player", (localPlayer != null).ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Randomized Character", randomizedChar.ToString()); } catch { }
                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localPlayer.Player.playerEntity.gameObject.GetComponentsInChildren<Renderer>())
                    {
                        rendererNames = rendererNames + r.name + ", ";
                    }
                    md.AddToWindow("Local Player", "Character renderers ingame", rendererNames);
                }
                catch { }

                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localPlayer.Player.playerEntity.skinRenderers)
                    {
                        rendererNames = rendererNames + r.name + ", ";
                    }
                    md.AddToWindow("Local Player", "VisualEntity renderers ingame", rendererNames);
                }
                catch { }

                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localPlayer.Player.playerEntity.skinRenderers)
                    {
                        if (r.name == "meshNurse_MainRenderer")
                        {
                            foreach (Material m in r.materials)
                            {
                                rendererNames = rendererNames + m.name + ", ";
                            }
                        }
                    }
                    md.AddToWindow("Local Player", "Material names in meshNurse_MainRenderer", rendererNames);
                }
                catch { }
                /*
                try { md.AddToWindow("Remote Player", "Lobby Player", opponentPlayer.ToString()); } catch { md.AddToWindow("Remote Player", "Lobby Player", "null"); }
                try { md.AddToWindow("Remote Player", "Lobby Player Model", opponentLobbyCharacterModel.ToString()); } catch { md.AddToWindow("Remote Player", "Lobby Player Model", "null"); }
                try { md.AddToWindow("Remote Player", "Game PlayerEntity", opponentPlayerEntity.ToString()); } catch { md.AddToWindow("Remote Player", "Game PlayerEntity", "null"); }
                try { md.AddToWindow("Remote Player", "Name", opponentPlayerNr.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Character", opponentCustomSkinCharacter.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Variant", opponentCustomSkinCharacterVariant.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Custom Texture", opponentCustomTexture.ToString()); } catch { md.AddToWindow("Remote Player", "Custom Texture", "null"); }
                try { md.AddToWindow("Remote Player", "Initiate Player", initOpponentPlayer.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Cancel Skin", cancelOpponentSkin.ToString()); } catch { }
                */
                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in mainBall.gameObject.GetComponentInChildren<VisualEntity>().skinRenderers)
                    {
                        rendererNames = rendererNames + r.name + ", ";
                    }
                    md.AddToWindow("Ball", "Name of renderers", rendererNames);
                }
                catch { }

                try
                {
                    VisualEntity[] ves = FindObjectsOfType<VisualEntity>();
                    string rendererNames = "";
                    foreach (VisualEntity ve in ves)
                    {
                        rendererNames = rendererNames + ve.name + ", ";
                    }
                    md.AddToWindow("Effects", "VisualEntities", rendererNames);
                }
                catch { }


                try
                {
                    GameObject[] gos = FindObjectsOfType<GameObject>();
                    string ents = "";
                    foreach (GameObject go in gos)
                    {
                        ents = ents + go.name + ", ";
                    }
                    md.AddToWindow("GameObjects", "Active GameObjects", ents);
                }
                catch { }

            }


            #endregion
        }


    }


    public enum GameType
    {
        Online = 0,
        Offline = 1,
        Any = 2
    }
}

