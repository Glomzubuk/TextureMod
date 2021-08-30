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
using HarmonyLib;
using LLBML;

namespace TextureMod
{
    public class TextureChanger : MonoBehaviour
    {
        #region General Fields
        public static string imageFolder = Path.Combine(TextureMod.ResourceFolder, "Images");

        private JOFJHDJHJGI gameState => StateApi.CurrentGameState;
        private GameMode currentGameMode => StateApi.CurrentGameMode;
        private bool IsOnline => NetworkApi.IsOnline;

        public static bool InMatch => World.instance != null && (DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.CDOFDJMLGLO || DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.LGILIJKMKOD) && !LLScreen.UIScreen.loadingScreenActive;
        public string[] debug = new string[20];
        public bool doSkinPost = false;
        public int postTimer = 0;
        private int postTimerLimit = 30;
        public bool doSkinGet = false;
        public int getTimer = 0;
        private int getTimerLimit = 20;
        private bool calculateMirror = true;
        private bool setAntiMirrior = false;
        public Character packetSkinCharacter = Character.NONE;
        public CharacterVariant packetSkinCharacterVariant = CharacterVariant.CORPSE;
        public bool sendCancelRequestToServer = true;
        private System.Random rng = new System.Random();
        private bool randomizedChar = false;
        private int siluetteTimer = 0;
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
        #region LocalPlayer Fields
        public ALDOKEMAOMB localLobbyPlayer => ALDOKEMAOMB.BJDPHEHJJJK(P2P.localPeer?.playerNr ?? 0);
        private PlayerEntity localGamePlayerEntity => ALDOKEMAOMB.BJDPHEHJJJK(P2P.localPeer?.playerNr ?? 0)?.JCCIAMJEODH;

        private CharacterModel localLobbyPlayerModel = null;
        public int localPlayerNr = -1;
        public int localSkinIndex = -1;
        public Character localPlayerChar = Character.NONE;
        public CharacterVariant localPlayerCharVar = CharacterVariant.CORPSE;
        public CustomSkin localCustomSkin;
        private bool initLocalPlayer = false;
        #endregion
        #region LocalPlayer ingame Model Fiels
        public int localPlayerIngameModelRendererCount = 0;
        public string localPlayerIngameRendererNames = "";
        #endregion
        #region Remote Player Fields
        public ALDOKEMAOMB opponentPlayer = null;
        private CharacterModel opponentLobbyCharacterModel = null;
        private PlayerEntity opponentPlayerEntity = null;
        public int opponentPlayerNr = -1;
        public Character opponentCustomSkinCharacter = Character.NONE;
        public CharacterVariant opponentCustomSkinCharacterVariant = CharacterVariant.CORPSE;
        public Texture2D opponentCustomTexture = null;
        private bool initOpponentPlayer = false;
        public bool newSkinToApply = false;
        private bool cancelOpponentSkin = false;
        #endregion

        #region Ball Fields
        private BallEntity mainBall;
        #endregion

        #region Effects
        Texture2D candySplashWhite;

        Texture2D gridStartBG;
        Texture2D gridStartFG;
        Texture2D gridTrail;
        Texture2D gridArrive;

        Texture2D bubbleBG;
        Texture2D bubbleFG;
        Texture2D bubblePopBG;
        Texture2D bubblePopFG;
        #endregion



        private void Start()
        {
            string effectsFolder = Path.Combine(imageFolder, "Effects");

            candySplashWhite = TextureHelper.LoadPNG(Path.Combine(effectsFolder, "candySplashWhite.png"));

            gridStartBG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "GridSpecial", "gridStartBG.png"));
            gridStartFG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "GridSpecial", "gridStartFG.png"));
            gridTrail = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "GridSpecial", "gridTrail.png"));
            gridArrive = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "GridSpecial", "gridArrive.png"));

            bubbleBG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "JetSpecial", "bubbleBG.png"));
            bubbleFG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "JetSpecial", "bubbleFG.png"));
            bubblePopBG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "JetSpecial", "bubblePopBG.png"));
            bubblePopFG = TextureHelper.LoadPNG(Utility.CombinePaths(effectsFolder, "JetSpecial", "bubblePopFG.png"));


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

        private void OnGUI()
        {

            if (localCustomSkin != null) //Show skin nametags
            {
                string labelTxt = localCustomSkin.GetSkinLabel();
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
                                if (localLobbyPlayer == ALDOKEMAOMB.BJDPHEHJJJK(0)) GUI.Box(new Rect(Screen.width / 10, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt); //Check if local player is the player with ID 0
                                else GUI.Box(new Rect((Screen.width / 20) * 12.95f, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                            case GameMode.FREE_FOR_ALL:
                            case GameMode.COMPETITIVE:
                                if (localLobbyPlayer == ALDOKEMAOMB.BJDPHEHJJJK(0)) GUI.Box(new Rect(0 + Screen.width / 250, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
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
            }
        }  //Show skin nametags

        private void FixedUpdate()
        {
            if (!neverApplyOpponentsSkin.Value)
            {
                if (doSkinPost) { postTimer++; }
                if (localLobbyPlayer != null && localCustomSkin != null)
                {
                    if (postTimer >= postTimerLimit)
                    {
                        doSkinPost = false;
                        postTimer = 0;
                        try
                        {
                            StartCoroutine(TextureMod.Instance.ec.PostSkin(localLobbyPlayer.KLEEADMGHNE.peerId, localPlayerChar, localPlayerCharVar, localCustomSkin.SkinTexture));
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.ToString());
                        }
                    }
                }

                if (doSkinGet) { getTimer++; }
                if (opponentPlayer != null)
                {
                    if (getTimer >= getTimerLimit)
                    {
                        doSkinGet = false;
                        getTimer = 0;
                        try
                        {
                            if (opponentPlayer.KLEEADMGHNE?.peerId != null)
                            {
                                StartCoroutine(TextureMod.Instance.ec.GetSkin(opponentPlayer.KLEEADMGHNE.peerId));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.ToString());
                        }
                    }
                }
            }

            if (siluetteTimer > 0) siluetteTimer--;
            if (reloadCustomSkinTimer > 0) reloadCustomSkinTimer--;
        } //POST and GET requests


        bool OnSkinChangeButtonDown()
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
                try { md.AddToWindow("Skin Exchange", "Do Skin Get", doSkinGet.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Get Timer", getTimer.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "New Skin To Apply", newSkinToApply.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Set Anti Mirror", setAntiMirrior.ToString()); } catch { }

                try { md.AddToWindow("Local Player", "Lobby Player", localLobbyPlayer.ToString()); } catch { md.AddToWindow("Local Player", "Lobby Player", "null"); }
                try { md.AddToWindow("Local Player", "Lobby Skin Index", localSkinIndex.ToString()); } catch { md.AddToWindow("Local Player", "Lobby Skin Index", "null"); }
                try { md.AddToWindow("Local Player", "Lobby Player Model", localLobbyPlayerModel.ToString()); } catch { md.AddToWindow("Local Player", "Lobby Player Model", "null"); }
                try { md.AddToWindow("Local Player", "Game PlayerEntity", localGamePlayerEntity.ToString()); } catch { md.AddToWindow("Local Player", "Game PlayerEntity", "null"); }
                try { md.AddToWindow("Local Player", "Name", localPlayerNr.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Character", localPlayerChar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Variant", localPlayerCharVar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Custom Texture", localCustomSkin.ToString()); } catch { md.AddToWindow("Local Player", "Custom Texture", "null"); }
                try { md.AddToWindow("Local Player", "Initiate Player", initLocalPlayer.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Randomized Character", randomizedChar.ToString()); } catch { }
                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localGamePlayerEntity.gameObject.GetComponentsInChildren<Renderer>())
                    {
                        rendererNames = rendererNames + r.name + ", ";
                    }
                    md.AddToWindow("Local Player", "Character renderers ingame", rendererNames);
                }
                catch { }

                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localGamePlayerEntity.skinRenderers)
                    {
                        rendererNames = rendererNames + r.name + ", ";
                    }
                    md.AddToWindow("Local Player", "VisualEntity renderers ingame", rendererNames);
                }
                catch { }

                try
                {
                    string rendererNames = "";
                    foreach (Renderer r in localGamePlayerEntity.skinRenderers)
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

                try { md.AddToWindow("Remote Player", "Lobby Player", opponentPlayer.ToString()); } catch { md.AddToWindow("Remote Player", "Lobby Player", "null"); }
                try { md.AddToWindow("Remote Player", "Lobby Player Model", opponentLobbyCharacterModel.ToString()); } catch { md.AddToWindow("Remote Player", "Lobby Player Model", "null"); }
                try { md.AddToWindow("Remote Player", "Game PlayerEntity", opponentPlayerEntity.ToString()); } catch { md.AddToWindow("Remote Player", "Game PlayerEntity", "null"); }
                try { md.AddToWindow("Remote Player", "Name", opponentPlayerNr.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Character", opponentCustomSkinCharacter.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Variant", opponentCustomSkinCharacterVariant.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Custom Texture", opponentCustomTexture.ToString()); } catch { md.AddToWindow("Remote Player", "Custom Texture", "null"); }
                try { md.AddToWindow("Remote Player", "Initiate Player", initOpponentPlayer.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Cancel Skin", cancelOpponentSkin.ToString()); } catch { }

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

        void OpponetCustomCharacter()
        {
            if (opponentPlayer != null && opponentLobbyCharacterModel != null)
            {
                Traverse tv_opponentLobbyCharacterModel = Traverse.Create(opponentLobbyCharacterModel);
                Character opponentModelCharacter = tv_opponentLobbyCharacterModel.Field<Character>("character").Value;
                CharacterVariant opponentModelCharacterVariant = tv_opponentLobbyCharacterModel.Field<CharacterVariant>("characterVariant").Value;
                if ((opponentModelCharacter != opponentCustomSkinCharacter || opponentModelCharacterVariant != opponentCustomSkinCharacterVariant) && (opponentPlayer.DOFCCEDJODB != Character.NONE || opponentPlayer.AIINAIDBHJI != CharacterVariant.CORPSE))
                {
                    if (packetSkinCharacter != Character.NONE && opponentPlayer.DOFCCEDJODB != (Character)32 && packetSkinCharacterVariant != CharacterVariant.CORPSE)
                    {
                        opponentCustomSkinCharacter = packetSkinCharacter;
                        opponentCustomSkinCharacterVariant = packetSkinCharacterVariant;

                        if (currentGameMode == GameMode._1v1 && opponentPlayer.CJFLMDNNMIE == 1) opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, packetSkinCharacter, packetSkinCharacterVariant, true);
                        else opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, packetSkinCharacter, packetSkinCharacterVariant, false);
                    }
                    else
                    {
                        if (opponentPlayer.DOFCCEDJODB != Character.NONE && opponentPlayer.DOFCCEDJODB != (Character)32 && opponentPlayer.AIINAIDBHJI != CharacterVariant.CORPSE)
                        {
                            opponentCustomSkinCharacter = opponentPlayer.DOFCCEDJODB;
                            opponentCustomSkinCharacterVariant = opponentPlayer.AIINAIDBHJI;
                            if (currentGameMode == GameMode._1v1 && opponentPlayer.CJFLMDNNMIE == 1) opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, packetSkinCharacter, packetSkinCharacterVariant, true);
                            else opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentPlayer.DOFCCEDJODB, opponentPlayer.AIINAIDBHJI, false);
                        }
                        initOpponentPlayer = true;
                    }
                }

                if (newSkinToApply)
                {
                    if (!cancelOpponentSkin)
                    {
                        if (opponentCustomSkinCharacter == packetSkinCharacter || opponentCustomSkinCharacterVariant == packetSkinCharacterVariant)
                        {
                            opponentCustomTexture = TextureHelper.LoadPNG(Path.Combine(imageFolder, "opponent.png"));
                            packetSkinCharacter = Character.NONE;
                            packetSkinCharacterVariant = CharacterVariant.CORPSE;
                            opponentLobbyCharacterModel.PlayCamAnim();
                        }
                    }
                    calculateMirror = true;
                    newSkinToApply = false;
                }

                if (opponentPlayer.DOFCCEDJODB != opponentCustomSkinCharacter && opponentPlayer.DOFCCEDJODB != (Character)32 && opponentPlayer.DOFCCEDJODB != Character.NONE)
                {
                    opponentCustomSkinCharacter = opponentPlayer.DOFCCEDJODB;
                    opponentCustomSkinCharacterVariant = opponentPlayer.AIINAIDBHJI;
                    if (currentGameMode == GameMode._1v1 && opponentPlayer.CJFLMDNNMIE == 1) opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentPlayer.DOFCCEDJODB, opponentPlayer.AIINAIDBHJI, true);
                    else opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentPlayer.DOFCCEDJODB, opponentPlayer.AIINAIDBHJI, false);
                }
            }
        }

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

            OpponetCustomCharacter();

            AssignSkinToLocalPlayer();

            DisableCharacterButtons();

            CheckMirror();

            InLobbyOrGameChecks();

            UpdateCustomSkinsInMenu();

            ApplyInGameEffects();
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(reloadEntireSkinLibrary.Value))
            {
                TextureMod.Instance.tl.LoadLibrary(); //Reloads the entire texture folder
            }
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

        void CheckMirror()
        {
            if (opponentPlayer != null && opponentCustomTexture != null && localCustomSkin?.SkinTexture != null && InLobby(GameType.Any) && calculateMirror)
            {
                if (localPlayerChar == opponentCustomSkinCharacter)
                {
                    Color[] ot = opponentCustomTexture.GetPixels();
                    var otCount = 0;
                    foreach (Color col in ot)
                    {
                        if (col.r > 0.5f && col.g > 0.5f && col.b > 0.5f && col.a > 0.5f) { if (col.r != 1f && col.g != 1f && col.b != 1f) { otCount++; } }
                        else if (col.r < 0.5f && col.g < 0.5f && col.b < 0.5f) { if (col.r == 0f && col.g == 0f && col.b == 0f) { otCount++; } }
                    }
                    Color[] pt = localCustomSkin.SkinTexture.GetPixels();
                    var ptCount = 0;
                    foreach (Color col in pt)
                    {
                        if (col.r > 0.5f && col.g > 0.5f && col.b > 0.5f && col.a > 0.5f) { if (col.r != 1f && col.g != 1f && col.b != 1f) { ptCount++; } }
                        else if (col.r < 0.5f && col.g < 0.5f && col.b < 0.5f) { if (col.r == 0f && col.g == 0f && col.b == 0f) { ptCount++; } }
                    }
                    PlayerEntity[] pes = FindObjectsOfType<PlayerEntity>();
                    if (otCount == ptCount)
                    {
                        setAntiMirrior = true;
                        MakeTextureGrayscale(opponentCustomTexture);
                    }
                    else setAntiMirrior = false;
                    calculateMirror = false;
                }
            } //Check if your skin matches your opponents, and if it does set theirs to grayscale

            if (localCustomSkin?.SkinTexture == null || opponentCustomTexture == null)
            {
                setAntiMirrior = false;
            }
        }

        void ApplyInGameEffects()
        {
            if (InGame(GameType.Any))
            {
                mainBall = mainBall ?? BallHandler.instance.GetBall(0);
                //Player.AllInMatch
                ALDOKEMAOMB.ICOCPAFKCCE((ALDOKEMAOMB player) =>
                {
                    if (localGamePlayerEntity?.character == player.DOFCCEDJODB || opponentPlayerEntity?.character == player.DOFCCEDJODB)
                    {
                        switch (player.DOFCCEDJODB)
                        {
                            case Character.CANDY: CandymanIngameEffects(); break;
                            case Character.BAG: AshesIngameEffects(); break;
                            case Character.GRAF: ToxicIngameEffects(); break;
                            case Character.ELECTRO: GridIngameEffects(); break;
                            case Character.SKATE: JetIngameEffects(localCustomSkin?.SkinTexture); break;
                        }
                    }
                });
            }
            else
            {
                mainBall = null;
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

                        if (initLocalPlayer)
                        {
                            InitLocalPlayer();
                        }

                        if (InLobby(GameType.Offline))
                        {
                            if (localLobbyPlayerModel == null)
                            {
                                localLobbyPlayerModel = GetCurrentCharacterModel(localLobbyPlayer.CJFLMDNNMIE);
                            }

                            localLobbyPlayerModel?.SetSilhouette(false);
                            if (localCustomSkin != null)
                            {
                                AssignTextureToCharacterModelRenderers(localLobbyPlayerModel, localCustomSkin.SkinTexture);
                            }
                        }
                        else if (InGame(GameType.Offline))
                        {
                            if (localCustomSkin != null)
                            {
                                if (Input.GetKeyDown(reloadCustomSkin.Value))
                                {
                                    try { localCustomSkin.ReloadSkin(); }
                                    catch { AudioHandler.PlaySfx(Sfx.MENU_BACK); }
                                }

                                AssignTextureToIngameCharacter(localGamePlayerEntity, localCustomSkin.SkinTexture);
                                localPlayerNr = AssignTextureToHud(localGamePlayerEntity, localCustomSkin.SkinTexture);
                            }
                        }
                        break;
                    #endregion
                    case GameMode._1v1:
                    case GameMode.FREE_FOR_ALL:
                    case GameMode.COMPETITIVE:
                        #region In ranked and online lobby

                        if (Input.GetKeyDown(cancelKey.Value))
                        {
                            cancelOpponentSkin = !cancelOpponentSkin;
                            if (opponentLobbyCharacterModel != null)
                            {
                                opponentCustomTexture = null;
                                opponentCustomSkinCharacter = opponentPlayer.DOFCCEDJODB;
                                opponentCustomSkinCharacterVariant = opponentPlayer.AIINAIDBHJI;
                                if (currentGameMode == GameMode._1v1 && opponentPlayer.CJFLMDNNMIE == 1) opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentCustomSkinCharacter, CharacterVariant.CORPSE, true);
                                else opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentCustomSkinCharacter, CharacterVariant.CORPSE, false);
                                initOpponentPlayer = true;
                            }
                        }

                        if (InLobby(GameType.Any))
                        {
                            if (initLocalPlayer)
                            {
                                InitLocalPlayer();
                            }

                            if (initOpponentPlayer)
                            {
                                InitOpponentPlayer();
                            }

                            try
                            {
                                if (sendCancelRequestToServer == true)
                                {
                                    StartCoroutine(TextureMod.Instance.ec.PostCancel(localLobbyPlayer.KLEEADMGHNE.peerId));
                                    sendCancelRequestToServer = false;
                                }
                            }
                            catch { }

                            //if (localLobbyPlayer.CJFLMDNNMIE == 3) initLocalPlayer = true;

                            if (localLobbyPlayerModel == null)
                            {
                                localLobbyPlayerModel = GetCurrentCharacterModel(localLobbyPlayer.CJFLMDNNMIE);
                            }
                            else
                            {
                                localLobbyPlayerModel.SetSilhouette(false);
                                if (localCustomSkin != null)
                                {
                                    AssignTextureToCharacterModelRenderers(localLobbyPlayerModel, localCustomSkin.SkinTexture);
                                }
                            }

                            if (InLobby(GameType.Online) && localLobbyPlayer?.PNHOIDECPJE != null)
                            {
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
                            }
                            
                        }
                        else if (InGame(GameType.Any))
                        {
 
                            try
                            {
                                if (sendCancelRequestToServer == true)
                                {
                                    StartCoroutine(TextureMod.Instance.ec.PostCancel(localLobbyPlayer.KLEEADMGHNE.peerId));
                                    sendCancelRequestToServer = false;
                                }
                            }
                            catch { }

                            if (localCustomSkin != null)
                            {
                                AssignTextureToIngameCharacter(localGamePlayerEntity, localCustomSkin.SkinTexture);
                                localPlayerNr = AssignTextureToHud(localGamePlayerEntity, localCustomSkin.SkinTexture);
                            }
                            
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
                        }
                        else if (InPostGame())
                        {
                            AssignSkinToWinnerModel();

                            if (localCustomSkin != null)
                            {
                                AssignTextureToPostGameHud(localPlayerNr, localCustomSkin.SkinTexture);
                            }

                            if (opponentCustomTexture != null)
                            {
                                AssignTextureToPostGameHud(opponentPlayerNr, opponentCustomTexture);
                            }
                        }
                        else
                        {
                            if (localLobbyPlayer?.NGLDMOLLPLK == false)
                            {
                                initLocalPlayer = true;
                            }

                            initOpponentPlayer = true;
                        }
                        break;
                }
                #endregion
            }
        }

        void AssignSkinToLocalPlayer()
        {
            if (localLobbyPlayer != null && randomizedChar == false) // Determine and assign skin to local player
            {
                //Player.Selected - Has the Player selected their character yet.
                if (localLobbyPlayer.CHNGAKOIJFE)
                {
                    var changeSkin = false;

                    if (localPlayerChar != localLobbyPlayer.DOFCCEDJODB || localPlayerCharVar != localLobbyPlayer.AIINAIDBHJI)
                    {
                        localCustomSkin = null;
                        if (assignFirstSkinOnCharacterSelection.Value && localPlayerChar != localLobbyPlayer.DOFCCEDJODB && localLobbyPlayer.DOFCCEDJODB != Character.RANDOM)
                        {
                            localSkinIndex = 0;
                            changeSkin = true;
                        }
                        else
                        {
                            initLocalPlayer = true;
                        }
                        localPlayerChar = localLobbyPlayer.DOFCCEDJODB;
                        localPlayerCharVar = localLobbyPlayer.AIINAIDBHJI;
                    }

                    LLButton[] buttons = FindObjectsOfType<LLButton>();
                    HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();

                    if (useOnlySetKey.Value == false)
                    {
                        if (Input.GetKey(holdKey1.Value) && buttons.Length > 0)
                        {
                            if (OnSkinChangeButtonDown())
                            {
                                changeSkin = true;
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
                        changeSkin = true;
                    }

                    if (changeSkin && InLobby(GameType.Any)) // Assign skin to local player
                    {
                        if (setAntiMirrior)
                        {
                            opponentCustomTexture = TextureHelper.LoadPNG(Application.dataPath.Replace("/", @"\") + @"\Managed\TextureModResources\Images\opponent.png");
                            setAntiMirrior = false;
                        }

                        if (InLobby(GameType.Online))
                        {
                            AccessTools.Method(typeof(HDLIJDBFGKN), "JPNNBHNHHJC").Invoke(gameStatesOnlineLobby, null); // AutoReadyReset
                            AccessTools.Method(typeof(HDLIJDBFGKN), "EMFKKOJEIPN").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer.CJFLMDNNMIE, false }); // SetReady

                            AccessTools.Method(typeof(HDLIJDBFGKN), "BFIGLDLHKPO").Invoke(gameStatesOnlineLobby, null); // UpdateReadyButton
                            AccessTools.Method(typeof(HDLIJDBFGKN), "OFGNNIBJOLH").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer }); // SendPlayerState
                            /*
                            gameStatesOnlineLobby.JPNNBHNHHJC(); // gameStatesOnlineLobby.AutoReadyReset
                            gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, false); // SetReady
                            gameStatesOnlineLobby.BFIGLDLHKPO(); // gameStatesOnlineLobby.UpdateReadyButton
                            gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer); // gameStatesOnlineLobby.SendPlayerState
                            */
                        }

                        bool isRandom = false;
                        if (localLobbyPlayer.DOFCCEDJODB == Character.RANDOM) // Randomize skin and char
                        {
                            //Creats a list of characters that have no skins and should be excluded from the character randomizer
                            List<Character> characters = new List<Character>();
                            foreach (var character in TextureMod.Instance.tl.newCharacterTextures)
                            {
                                if (character.Value.Count == 0)
                                {
                                    characters.Add(character.Key);
                                }
                            }

                            Character randomChar = localLobbyPlayer.HGPNPNPJBMK(characters.ToArray());
                            localLobbyPlayer.DOFCCEDJODB = randomChar;

                            if (InLobby(GameType.Online))
                            {
                                AccessTools.Method(typeof(HDLIJDBFGKN), "EMFKKOJEIPN").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer.CJFLMDNNMIE, true }); // SetReady
                                AccessTools.Method(typeof(HDLIJDBFGKN), "OFGNNIBJOLH").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer }); // SendPlayerState
                                /*
                                gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, true); // SetReady
                                gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer); //Send player state (Signalizes that we have changes characters and that we are ready)
                                */

                                if (lockButtonsOnRandom.Value)
                                {
                                    foreach (LLButton b in buttons) b.SetActive(false);
                                    randomizedChar = true;
                                }
                            }

                            isRandom = true;
                        }

                        SetLocalCustomSkin(localLobbyPlayer.DOFCCEDJODB, isRandom);

                        if (InLobby(GameType.Online))
                        {
                            doSkinPost = true;
                            postTimer = 0;
                            setAntiMirrior = false;
                            calculateMirror = true;
                        }
                    }
                }
                else if (initLocalPlayer)
                {
                    InitLocalPlayer();
                }
            }
        }

        void UpdateCustomSkinsInMenu()
        {
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (InMenu())
            {
                sendCancelRequestToServer = true;
                if (screenOne?.screenType != ScreenType.UNLOCKS_SKINS)
                {
                    InitLocalPlayer();
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
                    if (siluetteTimer > 0)
                    {
                        if (localCustomSkin != null)
                        {
                            characterModel.SetSilhouette(false);
                            AssignTextureToCharacterModelRenderers(characterModel, localCustomSkin.SkinTexture);
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
                                SetUnlocksCharacterModel(localCustomSkin.SkinTexture);
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
                                    SetUnlocksCharacterModel(localCustomSkin.SkinTexture);
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

        public enum GameState
        {
            NONE,
            INTRO,
            MENU,
            QUIT,
            LOBBY_LOCAL,
            LOBBY_ONLINE,
            LOBBY_CHALLENGE,
            CHALLENGE_LADDER,
            CHALLENGE_LOST,
            STORY_GRID,
            STORY_COMIC,
            LOBBY_TRAINING,
            LOBBY_TUTORIAL,
            CREDITS,
            OPTIONS_GAME,
            OPTIONS_INPUT,
            OPTIONS_AUDIO,
            OPTIONS_VIDEO,
            GAME_INTRO,
            GAME,
            GAME_PAUSE,
            GAME_RESULT,
            UNLOCKS,
            LOBBY_STORY
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

        public bool InPostGame()
        {
            return (currentGameMode == GameMode._1v1 || currentGameMode == GameMode.FREE_FOR_ALL || currentGameMode == GameMode.COMPETITIVE) && gameState == (JOFJHDJHJGI)GameState.GAME_RESULT;
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

        private ALDOKEMAOMB GetOpponentPlayerInLobby()
        {
            ALDOKEMAOMB player = null;
            if (localLobbyPlayer != null)
            {
                if (localLobbyPlayer.CJFLMDNNMIE == 0)
                {
                    player = ALDOKEMAOMB.BJDPHEHJJJK(1);
                }
                else if (localLobbyPlayer.CJFLMDNNMIE == 1)
                {
                    player = ALDOKEMAOMB.BJDPHEHJJJK(0);
                }
            }

            if (player.DOFCCEDJODB != Character.NONE && player.DOFCCEDJODB != Character.RANDOM) return player;
            else return null;
        }

        public void AssignTextureToCharacterModelRenderers(CharacterModel model, Texture2D tex)
        {
            Renderer[] rs = model.curModel?.transform.GetComponentsInChildren<Renderer>() ?? new Renderer[0];
            for (int i = 0; i < rs.Length; i++)
            {
                AssignTextureToRenderer(rs[i], tex);
            }
        }

        private PlayerEntity GetLocalPlayerInGame()
        {
            return localLobbyPlayer.JCCIAMJEODH;
        }

        private PlayerEntity GetOpponentPlayerInGame()
        {
            return opponentPlayer.JCCIAMJEODH;
        }

        private void AssignTextureToIngameCharacter(PlayerEntity playerEntity, Texture2D tex)
        {
            VisualEntity ve = playerEntity.gameObject.GetComponent<VisualEntity>();
            if (ve != null)
            {
                if (ve.skinRenderers.Count > 0)
                {
                    foreach (Renderer r in ve.skinRenderers)
                    {
                        AssignTextureToRenderer(r, tex);
                    }

                    if (playerEntity.character == Character.GRAF)
                    {
                        AssignToxicEffectColors(playerEntity.player.CJFLMDNNMIE, tex, playerEntity.variant);
                    }
                }
            }
        }

        private int AssignTextureToHud(PlayerEntity playerEntity, Texture tex)
        {
            int ret = -1;
            GameHudPlayerInfo[] ghpis = FindObjectsOfType<GameHudPlayerInfo>();
            if (ghpis.Length > 0)
            {
                foreach (GameHudPlayerInfo ghpi in ghpis)
                {
                    ALDOKEMAOMB shownPlayer = Traverse.Create(ghpi).Field<ALDOKEMAOMB>("shownPlayer").Value;
                    if (shownPlayer == playerEntity.player)
                    {
                        Renderer[] rs = ghpi.gameObject.transform.GetComponentsInChildren<Renderer>();
                        if (rs.Length > 0)
                        {
                            foreach (Renderer r in rs)
                            {
                                AssignTextureToRenderer(r, tex);
                            }
                        }
                        ret = shownPlayer.CJFLMDNNMIE;
                    }
                }
            }
            return ret;
        }

        private void AssignTextureToPostGameHud(int playerNr, Texture tex)
        {
            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            if (screenZero?.screenType == ScreenType.GAME_RESULTS)
            {
                PostScreen postScreen = screenZero as PostScreen;
                Renderer[] rs = postScreen.playerBarsByPlayer[playerNr].gameObject.transform.GetComponentsInChildren<Renderer>();

                for (int i = 0; i < rs.Length; i++)
                {
                    AssignTextureToRenderer(rs[i], tex, playerNr);
                }
            }
        }

        void AssignTextureToRenderer(Renderer r, Texture tex, int playerIndex = -1, PlayerEntity playerEntity = null)
        {
            Character character = Character.NONE;
            CharacterVariant characterVariant = CharacterVariant.DEFAULT;
            playerEntity = playerEntity ?? r.transform.GetComponentInParent<PlayerEntity>();

            if (playerEntity != null)
            {
                character = playerEntity.character;
                characterVariant = playerEntity.variant;
            }
            else
            {
                GameHudPlayerInfo playerHud = r.transform.GetComponentInParent<GameHudPlayerInfo>();
                if (playerHud != null)
                {
                    ALDOKEMAOMB shownPlayer = Traverse.Create(playerHud).Field<ALDOKEMAOMB>("shownPlayer").Value;
                    character = shownPlayer.DOFCCEDJODB;
                    characterVariant = shownPlayer.AIINAIDBHJI;
                }
                else
                {
                    CharacterModel characterModel = r.transform.GetComponentInParent<CharacterModel>();
                    if (characterModel != null)
                    {
                        Traverse tv_charModel = Traverse.Create(characterModel);
                        character = tv_charModel.Field<Character>("character").Value;
                        characterVariant = tv_charModel.Field<CharacterVariant>("characterVariant").Value;
                    }
                    else
                    {
                        if (playerIndex > -1)
                        {
                            ALDOKEMAOMB player = ALDOKEMAOMB.BJDPHEHJJJK(playerIndex);
                            character = player.DOFCCEDJODB;
                            characterVariant = player.AIINAIDBHJI;
                        }
                    }
                }
            }

            if (!r.gameObject.name.EndsWith("Outline"))
            {
                string materialTexName = r.material.mainTexture?.name ?? "";
                if (characterVariant == CharacterVariant.STATIC_ALT && r.material.name.Contains("ScreenSpaceNoise"))
                {
                    AOOJOMIECLD modelValues = JPLELOFJOOH.NEBGBODHHCG(character, characterVariant != CharacterVariant.STATIC_ALT ? characterVariant : CharacterVariant.DEFAULT);
                    r.material = modelValues.DMAMFHLFOJF(0, false);
                }

                if (!materialTexName.Contains("Silhouett") && materialTexName != "")
                {
                    //r.material.shader = Shader.Find("LethalLeague/GameplayOpaque");
                    r.material.SetTexture("_MainTex", tex);
                }
            }

            if (character == Character.GRAF)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    AssignNurseToxicCanisters(r, (Texture2D)tex);
                }
            }
            else if (character == Character.SKATE)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                {
                    AssignJetScubaVisor(r, (Texture2D)tex);
                }
            }
            else if (character == Character.BOSS)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                {
                    AssignOmegaDoomboxSmearsAndArms(r, (Texture2D)tex);
                }
                else if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    AssignVisualizer(character, r, (Texture2D)tex);
                }
            }
            else if (character == Character.BAG)
            {
                AssignAshesOutlineColor(r, characterVariant, (Texture2D)tex);
            }
            else if (character == Character.BOOM)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    AssignVisualizer(character, r, (Texture2D)tex);
                }
            }
        }

        private void AssignSkinToWinnerModel()
        {
            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            if (screenZero?.screenType == ScreenType.GAME_RESULTS)
            {
                PostScreen ps = screenZero as PostScreen;

                if (localCustomSkin != null)
                {
                    if (ps.winnerCharacter == localPlayerChar && ps.winnerCharacterVariant == localPlayerCharVar) AssignTextureToCharacterModelRenderers(ps.winnerModel, localCustomSkin.SkinTexture);
                }

                if (opponentCustomTexture != null)
                {
                    if (ps.winnerCharacter == opponentCustomSkinCharacter && ps.winnerCharacterVariant == opponentCustomSkinCharacterVariant) AssignTextureToCharacterModelRenderers(ps.winnerModel, opponentCustomTexture);
                }
            }
        }

        private void SetCharacterModelTex(CharacterModel characterModel, Texture2D tex)
        {
            characterModel.SetSilhouette(false);
            AssignTextureToCharacterModelRenderers(characterModel, tex);
        }

        private void SetUnlocksCharacterModel(Texture2D tex)
        {
            CharacterModel[] cms = FindObjectsOfType<CharacterModel>();
            if (cms.Length > 0)
            {
                foreach (CharacterModel cm in cms)
                {
                    cm.SetSilhouette(false);
                    AssignTextureToCharacterModelRenderers(cm, tex);
                }
            }
        }

        private void InitLocalPlayer()
        {
            //localLobbyPlayer = null;
            localLobbyPlayerModel = null;
            //localGamePlayerEntity = null;
            localPlayerNr = -1;
            localSkinIndex = -1;
            localCustomSkin = null;
            doSkinPost = false;
            postTimer = 0;
            randomizedChar = false;
            initLocalPlayer = false;
        }
        private void InitOpponentPlayer()
        {
            opponentPlayer = null;
            opponentLobbyCharacterModel = null;
            opponentPlayerEntity = null;
            opponentPlayerNr = -1;
            opponentCustomTexture = null;
            doSkinGet = false;
            getTimer = 0;
            initOpponentPlayer = false;
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

        private void SetLocalCustomSkin(Character character, bool isRandom = false)
        {
            localCustomSkin = GetCustomSkin(character, isRandom);
            if (localCustomSkin == null) { return; }

            bool flipped = currentGameMode == GameMode._1v1 && localLobbyPlayer.CJFLMDNNMIE == 1 ? true : false;

            localPlayerCharVar = GetCustomSkinVariant(localCustomSkin.Variant, localLobbyPlayer.AIINAIDBHJI);
            localPlayerChar = character;
            localLobbyPlayer.AIINAIDBHJI = localPlayerCharVar;
            localLobbyPlayerModel = GetCurrentCharacterModel(P2P.localPeer?.playerNr ?? 0);
            localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, localPlayerChar, localPlayerCharVar, flipped);
            localLobbyPlayerModel.PlayCamAnim();
        }

        private CharacterModel GetCurrentCharacterModel(int playerNr = 0)
        {
            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];

            if (screenZero?.screenType == ScreenType.PLAYERS)
            {
                var screenPlayers = screenZero as ScreenPlayers;
                return screenPlayers.playerSelections[playerNr].characterModel;
            }
            else if (screenOne?.screenType == ScreenType.UNLOCKS_SKINS)
            {
                var screenUnlocksSkins = screenOne as ScreenUnlocksSkins;
                return screenUnlocksSkins.previewModel;
            }
            else if (screenOne?.screenType == ScreenType.UNLOCKS_CHARACTERS)
            {
                var screenUnlocksCharacters = screenOne as ScreenUnlocksCharacters;
                return screenUnlocksCharacters.previewModel;
            }
            return null;
        }


        CharacterVariant GetCustomSkinVariantShowcase(CustomSkin.VariantType variantType)
        {
            switch (variantType)
            {
                case CustomSkin.VariantType.Alternative:
                    return CharacterVariant.MODEL_ALT;
                case CustomSkin.VariantType.DLC:
                    return CharacterVariant.MODEL_ALT3;
                case CustomSkin.VariantType.Default:
                    return CharacterVariant.DEFAULT;
                default:
                    return CharacterVariant.DEFAULT;
            }
        }

        CharacterVariant GetCustomSkinVariant(CustomSkin.VariantType variantType, CharacterVariant characterVariant)
        {
            switch (variantType)
            {
                case CustomSkin.VariantType.Alternative:
                    if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.DOFCCEDJODB == localLobbyPlayer.DOFCCEDJODB)
                    {
                        return opponentPlayer.AIINAIDBHJI == CharacterVariant.MODEL_ALT ? CharacterVariant.MODEL_ALT2 : CharacterVariant.MODEL_ALT;
                    }
                    else return CharacterVariant.MODEL_ALT;
                case CustomSkin.VariantType.DLC:
                    if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.DOFCCEDJODB == localLobbyPlayer.DOFCCEDJODB)
                    {
                        return opponentPlayer.AIINAIDBHJI == CharacterVariant.MODEL_ALT3 ? CharacterVariant.MODEL_ALT4 : CharacterVariant.MODEL_ALT3;
                    }
                    else return CharacterVariant.MODEL_ALT3;
                default:
                    if (characterVariant < CharacterVariant.STATIC_ALT)
                    {
                        return characterVariant;
                    }
                    else if (opponentPlayer?.DOFCCEDJODB == localLobbyPlayer.DOFCCEDJODB)
                    {
                        return opponentPlayer.AIINAIDBHJI == CharacterVariant.DEFAULT ? CharacterVariant.ALT0 : CharacterVariant.DEFAULT;
                    }
                    else return CharacterVariant.DEFAULT;
            }
        }

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
                localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key);
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
                        localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
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
                                localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, pair.Key);
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
                                    localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
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
                        localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, lastSkin.Key);
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
                                localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, pair.Key);
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
                                    localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, lastSkin.Key);
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

            localCustomSkin = GetCustomSkin(character);
            screen.previewModel.SetCharacterResultScreen(0, character, GetCustomSkinVariant(localCustomSkin.Variant, characterVariant));
            siluetteTimer = 5;
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

            localCustomSkin = GetCustomSkin(susCharacter);
            screenUnlocksSkins.ShowCharacter(susCharacter, GetCustomSkinVariantShowcase(localCustomSkin.Variant), true);
            siluetteTimer = 5;
            SetCharacterModelTex(screenUnlocksSkins.previewModel, localCustomSkin?.SkinTexture);
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

        private CharacterVariant GetVariantFromFileName(Character localPlayerC, string name)
        {
            Character c = Character.NONE;
            CharacterVariant cv = CharacterVariant.DEFAULT;
            if (opponentPlayer != null)
            {
                c = opponentPlayer.DOFCCEDJODB;
                cv = opponentPlayer.AIINAIDBHJI;
            }
            if (name.Contains("_ALT2"))
            {
                if (TextureMod.hasDLC)
                {
                    var proceed = false;

                    foreach (Character DLC in TextureMod.ownedDLCs)
                    {
                        if (DLC == localPlayerC)
                        {
                            proceed = true;
                        }
                    }

                    if (proceed)
                    {
                        if (localPlayerC != c) return CharacterVariant.MODEL_ALT3;
                        else
                        {
                            if (cv == CharacterVariant.MODEL_ALT3) return CharacterVariant.MODEL_ALT4;
                            else return CharacterVariant.MODEL_ALT3;
                        }
                    }
                    else
                    {
                        if (localPlayerC != c) return CharacterVariant.DEFAULT;
                        else
                        {
                            if (cv == CharacterVariant.DEFAULT) return CharacterVariant.ALT0;
                            else return CharacterVariant.DEFAULT;
                        }
                    }
                }
                else
                {
                    if (localPlayerC != c) return CharacterVariant.DEFAULT;
                    else
                    {
                        if (cv == CharacterVariant.DEFAULT) return CharacterVariant.ALT0;
                        else return CharacterVariant.DEFAULT;
                    }
                }
            }
            else if (name.Contains("_ALT"))
            {
                if (localPlayerC != c) return CharacterVariant.MODEL_ALT;
                else
                {
                    if (cv == CharacterVariant.MODEL_ALT) return CharacterVariant.MODEL_ALT2;
                    else return CharacterVariant.MODEL_ALT;
                }
            }
            else
            {
                if (localPlayerC != c) return CharacterVariant.DEFAULT;
                else
                {
                    if (cv == CharacterVariant.DEFAULT) return CharacterVariant.ALT0;
                    else return CharacterVariant.DEFAULT;
                }
            }
        }


        public enum GameType
        {
            Online = 0,
            Offline = 1,
            Any = 2
        }

        #region Texture Manipulation [Color Replacers and effect colors, etc]

        #region General Effects
        public void MakeTextureGrayscale(Texture2D tex)
        {
            var texColors = tex.GetPixels();
            for (var i = 0; i < texColors.Length; i++)
            {
                var grayValue = texColors[i].grayscale;
                texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
            }
            tex.SetPixels(texColors);
            tex.Apply();
        }
        #endregion

        #region Ball Effects
        private void SetBallColors()
        {
            Color color = Color.grey;
            mainBall.GetVisual("main").renderers[2].materials[0].color = color;
            mainBall.SetColorOutlinesColor(color);
            for (int i = 0; i < mainBall.GetVisual("main2D").meshRendererTrail.Length; i++)
            {
                color.a = mainBall.GetVisual("main2D").meshRendererTrail[i].material.color.a;
                mainBall.GetVisual("main2D").meshRendererTrail[i].material.color = color;
            }
        }
        #endregion

        #region Dust&Ashes Effects
        public void AshesIngameEffects()
        {
            if (localGamePlayerEntity != null && localCustomSkin?.SkinTexture != null) if (localGamePlayerEntity.character == Character.BAG) AssignAshesOutlineColor(localGamePlayerEntity, localCustomSkin.SkinTexture);
                else if (opponentPlayerEntity != null && opponentCustomTexture != null) if (opponentPlayerEntity.character == Character.BAG) AssignAshesOutlineColor(opponentPlayerEntity, opponentCustomTexture);
        }


        public void AssignAshesOutlineColor(PlayerEntity pe, Texture2D tex)
        {
            foreach (Renderer r in pe.skinRenderers)
            {
                AssignAshesOutlineColor(r, pe.variant, tex);
            }
        }

        public void AssignAshesOutlineColor(Renderer r, CharacterVariant variant, Texture2D tex)
        {
            Color c = new Color(1, 1, 1, 1);
            switch (variant)
            {
                case CharacterVariant.DEFAULT: c = tex.GetPixel(58, 438); break;
                case CharacterVariant.ALT0: c = tex.GetPixel(58, 438); break;
                case CharacterVariant.MODEL_ALT: c = tex.GetPixel(69, 298); break;
                case CharacterVariant.MODEL_ALT2: c = tex.GetPixel(69, 298); break;
                case CharacterVariant.MODEL_ALT3: c = tex.GetPixel(113, 334); break;
                case CharacterVariant.MODEL_ALT4: c = tex.GetPixel(113, 334); break;
            }

            if (r.name != "mesh1Outline" && r.name != "mesh1MetalOutline" && r.name != "mesh1TenguOutline" && r.name.Contains("Outline")) r.material.color = c;
        }

        #endregion

        #region Candyman Effects
        public void CandymanIngameEffects()
        {
            if (mainBall.ballData.hitstunState == HitstunState.CANDY_STUN || mainBall.ballData.ballState == BallState.CANDYBALL)
            {
                if (mainBall.GetLastPlayerHitter().character == Character.CANDY)
                {
                    if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && localCustomSkin != null) AssignSkinToCandyball(mainBall, mainBall.GetLastPlayerHitter(), localCustomSkin.SkinTexture);
                    else if (mainBall.GetLastPlayerHitter() == opponentPlayerEntity && opponentCustomTexture != null) AssignSkinToCandyball(mainBall, mainBall.GetLastPlayerHitter(), opponentCustomTexture);
                    else AssignSkinToCandyball(mainBall, mainBall.GetLastPlayerHitter(), null);
                }
            }
            AssignSkinColorToCandySplash();
        }


        public void AssignSkinToCandyball(BallEntity ball, PlayerEntity pe, Texture2D tex)
        {
            if (tex == null)
            {
                tex = (Texture2D)pe.skinRenderers.First().material.mainTexture;
            }

            ball.GetVisual($"candyBall{(int)pe.variant}Visual").mainRenderer.material.mainTexture = tex;
        }

        int lastCandyBallPlayerIndex = -1;
        public void AssignSkinColorToCandySplash()
        {
            EffectEntity[] effects = FindObjectsOfType<EffectEntity>();
            if (mainBall.ballData.ballState == BallState.CANDYBALL)
            {
                lastCandyBallPlayerIndex = mainBall.GetLastPlayerHitter().playerIndex;
            }

            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    EffectEntity effect = effects[i];

                    if (effect.name == "candySplash")
                    {
                        Texture2D tex = CheckPlayerOrMainBallSplashEffect(localGamePlayerEntity, effect, localCustomSkin.SkinTexture) ?? CheckPlayerOrMainBallSplashEffect(opponentPlayerEntity, effect, opponentCustomTexture) ?? null;

                        if (tex != null)
                        {
                            SetCandySplashEffect(effects[i], tex);
                            for (int j = 0; j < effects.Length; j++)
                            {
                                //Searches for the intiial candySplash and sets it to custom texture
                                if (effects[j].effectData.graphicName == "candySplash" && effects[j].effectData.active)
                                {
                                    SetCandySplashEffect(effects[j], tex);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        Texture2D CheckPlayerOrMainBallSplashEffect(PlayerEntity playerEntity, EffectEntity effect, Texture2D characterTex)
        {
            if (playerEntity?.character == Character.CANDY && characterTex != null)
            {
                bool playerNearEffect = HHBCPNCDNDH.HPLPMEAOJPM(IBGCBLLKIHA.FCKBPDNEAOG(playerEntity.GetPosition(), effect.GetPosition()).KEMFCABCHLO, HHBCPNCDNDH.NKKIFJJEPOL(0.2m));
                bool ballNearEffect = HHBCPNCDNDH.HPLPMEAOJPM(IBGCBLLKIHA.FCKBPDNEAOG(mainBall.GetPosition(), effect.GetPosition()).KEMFCABCHLO, HHBCPNCDNDH.NKKIFJJEPOL(0.6m));
                if (playerNearEffect && !playerEntity.moveableData.velocity.HCBCKAHGJCA(IBGCBLLKIHA.DBOMOJGKIFI) && playerEntity.abilityData.abilityState.Contains("CROUCH"))
                {
                    return characterTex;
                }
                else if (ballNearEffect && lastCandyBallPlayerIndex == playerEntity.playerIndex)
                {
                    return characterTex;
                }
            }
            return null;
        }

        void SetCandySplashEffect(EffectEntity effect, Texture2D tex)
        {
            Renderer r = effect.GetVisual("main").mainRenderer;
            r.material.mainTexture = candySplashWhite;
            r.material.color = (localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT3 || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT4) ? tex.GetPixel(130, 92) : tex.GetPixel(103, 473);
            effect.name = "candySplashModified";
        }

        #region Grid effects
        public void GridIngameEffects()
        {
            if (mainBall?.ballData.hitstunState == HitstunState.TELEPORT_STUN || mainBall?.ballData.hitstunState == HitstunState.BUNT_STUN)
            {
                if (localGamePlayerEntity != null)
                {
                    if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && localCustomSkin != null) AssignGridSpecialColor(localGamePlayerEntity, localCustomSkin.SkinTexture);
                }

                if (opponentPlayerEntity != null)
                {
                    if (mainBall.GetLastPlayerHitter() == opponentPlayerEntity && opponentCustomTexture != null) { AssignGridSpecialColor(opponentPlayerEntity, opponentCustomTexture); }
                }
            }
            else
            {
                if (localGamePlayerEntity != null)
                {
                    if (localGamePlayerEntity.GetCurrentAbility()?.name.Contains("ELECTROCHARGE") != null && localCustomSkin != null) AssignGridSpecialColor(localGamePlayerEntity, localCustomSkin.SkinTexture);
                }

                if (opponentPlayerEntity != null)
                {
                    if (opponentPlayerEntity.GetCurrentAbility()?.name.Contains("ELECTROCHARGE") != null && opponentCustomTexture != null) AssignGridSpecialColor(opponentPlayerEntity, opponentCustomTexture);
                }
            }
        }

        public void AssignGridSpecialColor(PlayerEntity pe, Texture2D tex)
        {
            Color pixelColor = new Color(1, 1, 1, 1);

            //Gets the pixel color from the skin texture at a certain location based on if it's a DLC skin or Normal/Model_Alt
            pixelColor = (pe.variant <= CharacterVariant.MODEL_ALT2) ? tex.GetPixel(451, 454) : tex.GetPixel(442, 484);


            VisualEntity[] ves = FindObjectsOfType<VisualEntity>();
            foreach (VisualEntity ve in ves)
            {
                if (ve.name == "gridStart1")
                {
                    foreach (Renderer r in ve.GetComponentsInChildren<Renderer>())
                    {
                        r.material.mainTexture = gridStartFG;
                        Material m1 = r.material;
                        Material m2 = new Material(r.material.shader);
                        m2.mainTexture = gridStartBG;
                        Material[] mArray = new Material[] { m1, m2 };
                        r.materials = mArray;
                        r.materials[0].color = pixelColor;
                    }
                }

                if (ve.name == "gridArrive")
                {
                    foreach (Renderer r in ve.GetComponentsInChildren<Renderer>())
                    {
                        r.material.mainTexture = gridArrive;
                        r.materials[0].color = pixelColor;
                    }
                }

                if (ve.name == "gridTrail")
                {
                    foreach (Renderer r in ve.GetComponentsInChildren<Renderer>())
                    {
                        r.material.mainTexture = gridTrail;
                        r.materials[0].color = pixelColor;
                    }
                }
            }
        }

        #endregion

        #region Doombox Effects
        public void AssignVisualizer(PlayerEntity pe, Texture2D tex)
        {
            AssignVisualizer(pe.character, pe.GetVisual("main").mainRenderer, tex);
        }

        void AssignVisualizer(Character character, Renderer r, Texture2D tex)
        {
            FNDGCLEDHAD visualizer = r.gameObject.GetComponentInParent<FNDGCLEDHAD>();
            if (visualizer != null && tex != null)
            {
                Color c1 = character == Character.BOSS ? tex.GetPixel(493, 510) : tex.GetPixel(82, 10);
                Color c2 = character == Character.BOSS ? tex.GetPixel(508, 510) : tex.GetPixel(96, 10);
                Material vismat = Traverse.Create(visualizer).Field<Material>("FHAMOPAJHNJ").Value;
                try
                {
                    vismat.mainTexture = tex;
                    vismat.SetColor("_AmpColor0", c1);
                    vismat.SetColor("_AmpColor1", c2);
                }
                catch { Debug.Log($"Visulizer Broke for a moment and I don't know why ``\\_(-.-)_/``"); }
            }
        }

        public void AssignOmegaDoomboxSmearsAndArms(PlayerEntity pe, Texture2D tex)
        {
            AssignOmegaDoomboxSmearsAndArms(pe.GetVisual("main").mainRenderer, tex);
        }

        public void AssignOmegaDoomboxSmearsAndArms(Renderer r, Texture2D tex)
        {
            Color arm1 = tex.GetPixel(28, 336);
            Color arm2 = tex.GetPixel(28, 325);

            Color bright1 = tex.GetPixel(113, 336);
            Color bright2 = tex.GetPixel(113, 325);

            Color alpha = tex.GetPixel(178, 332);

            foreach (Material m in r.materials)
            {
                if (m.name.Contains("bossOmegaGlassMat") || m.name.Contains("bossOmegaEffectMat"))
                {
                    m.SetTexture("_MainTex", tex);
                    m.SetColor("_LitColor", new Color(arm1.r, arm1.g, arm1.b, bright1.r));
                    m.SetColor("_ShadowColor", new Color(arm2.r, arm2.g, arm2.b, bright2.r));
                    m.SetFloat("_Transparency", alpha.r);
                }
            }
        }
        #endregion

        #region Toxic Effects

        public void ToxicIngameEffects()
        {
            if (localGamePlayerEntity != null && localCustomSkin != null)
            {
                if (localGamePlayerEntity.character == Character.GRAF && localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT3 || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT4) AssignNurseToxicCanisters(localGamePlayerEntity, localCustomSkin.SkinTexture);
            }
            if (opponentPlayerEntity != null && opponentCustomTexture != null)
            {
                if (opponentPlayerEntity.character == Character.GRAF && opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT3 || opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT4) AssignNurseToxicCanisters(opponentPlayerEntity, opponentCustomTexture);
            }
        }

        public void AssignNurseToxicCanisters(PlayerEntity pe, Texture2D tex)
        {
            AssignNurseToxicCanisters(pe.GetVisual("main").mainRenderer, tex);
        }

        public void AssignNurseToxicCanisters(Renderer r, Texture2D tex)
        {
            Color light = tex.GetPixel(158, 414);
            Color shad = tex.GetPixel(158, 406);

            Color bright1 = tex.GetPixel(158, 397);
            Color bright2 = tex.GetPixel(158, 389);

            Color alpha = tex.GetPixel(158, 380);

            foreach (Material m in r.materials)
            {
                if (m.name.Contains("grafNurseGlass"))
                {
                    m.SetTexture("_MainTex", tex);
                    m.SetColor("_LitColor", new Color(light.r, light.g, light.b, bright1.r));
                    m.SetColor("_ShadowColor", new Color(shad.r, shad.g, shad.b, bright2.r));
                    m.SetFloat("_Transparency", alpha.r);
                }
            }
        }

        public void AssignToxicEffectColors(int playerId, Texture2D tex, CharacterVariant cv)
        {
            GrafPlayer[] toxics = FindObjectsOfType<GrafPlayer>();
            foreach (GrafPlayer toxic in toxics)
            {
                if (toxic.player.CJFLMDNNMIE == playerId)
                {
                    Color32 c = tex.GetPixel(258, 345);
                    c.a = byte.MaxValue;
                    toxic.GetVisual("paintBlobVisual").mainRenderer.material.color = c;
                    toxic.outfitEffectColors[(int)cv] = c;
                }
            }
        }


        #endregion

        #region Jet Effects
        public void JetIngameEffects(Texture2D skinTexture)
        {
            if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && skinTexture != null) AssignBubbleVisual(localGamePlayerEntity.variant, skinTexture);
            if (mainBall.GetLastPlayerHitter() == opponentPlayerEntity && opponentCustomTexture != null) AssignBubbleVisual(opponentPlayerEntity.variant, opponentCustomTexture);

            if (localGamePlayerEntity != null && skinTexture != null)
            {
                if (localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT2) AssignJetScubaVisor(localGamePlayerEntity, skinTexture);
            }

            if (opponentPlayerEntity != null && opponentCustomTexture != null)
            {
                if (opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT || opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT2) AssignJetScubaVisor(opponentPlayerEntity, opponentCustomTexture);
            }
        }
        public void AssignJetScubaVisor(PlayerEntity pe, Texture2D tex)
        {
            AssignJetScubaVisor(pe.GetVisual("main").mainRenderer, tex);
        }

        public void AssignJetScubaVisor(Renderer r, Texture2D tex)
        {
            Color light = tex.GetPixel(60, 328);
            Color shad = tex.GetPixel(60, 325);

            Color transparency = tex.GetPixel(60, 322);

            foreach (Material m in r.materials)
            {
                if (m.name.Contains("skateScubaGlass"))
                {
                    m.SetTexture("_MainTex", tex);
                    m.SetColor("_LitColor", new Color(light.r, light.g, light.b, transparency.g));
                    m.SetColor("_ShadowColor", new Color(shad.r, shad.g, shad.b, transparency.b));
                    m.SetFloat("_Transparency", transparency.r);
                }
            }
        }

        public void AssignBubbleVisual(CharacterVariant variant, Texture2D tex)
        {
            Color pixelColor = new Color(0, 1, 1);
            if (variant <= CharacterVariant.ALT6)
            {
                pixelColor = tex.GetPixel(59, 326);
            }
            else if (variant == CharacterVariant.MODEL_ALT || variant == CharacterVariant.MODEL_ALT2)
            {
                pixelColor = tex.GetPixel(59, 306);
            }
            else if (variant == CharacterVariant.MODEL_ALT3 || variant == CharacterVariant.MODEL_ALT4)
            {
                pixelColor = tex.GetPixel(59, 388);
            }

            MeshRenderer[] mrs = FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
            {
                if (mr.name == "bubbleVisual")
                {
                    mr.material.mainTexture = bubbleBG;
                    Material m1 = mr.material;
                    Material m2 = new Material(mr.material.shader);
                    m2.mainTexture = bubbleFG;
                    Material[] mArray = new Material[] { m1, m2 };
                    mr.materials = mArray;
                    mr.materials[0].color = pixelColor; break;
                }
            }

            VisualEntity[] ves = FindObjectsOfType<VisualEntity>();
            foreach (VisualEntity ve in ves)
            {
                if (ve.name == "bubblePop")
                {
                    foreach (Renderer r in ve.GetComponentsInChildren<Renderer>())
                    {
                        r.material.mainTexture = bubblePopFG;
                        Material m1 = r.material;
                        Material m2 = new Material(r.material.shader);
                        m2.mainTexture = bubblePopBG;
                        Material[] mArray = new Material[] { m1, m2 };
                        r.materials = mArray;
                        r.materials[1].color = pixelColor; break;
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}

