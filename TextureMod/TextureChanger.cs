using GameplayEntities;
using LLGUI;
using LLHandlers;
using LLScreen;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TextureMod
{
    public class TextureChanger : MonoBehaviour
    {
        #region General Fields
        private JOFJHDJHJGI gameState;
        private GameMode currentGameMode;
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
        private KeyCode holdKey1 = KeyCode.LeftShift;
        private KeyCode nextSkin = KeyCode.Mouse0;
        private KeyCode previousSkin = KeyCode.Mouse1;
        private KeyCode cancelKey = KeyCode.A;
        public KeyCode reloadCustomSkin = KeyCode.F5;
        public KeyCode reloadEntireSkinLibrary = KeyCode.F9;
        private bool useOnlySetKey = false;
        private bool neverApplyOpponentsSkin = false;
        public bool showDebugInfo = false;
        private bool lockButtonsOnRandom = false;
        public bool reloadCustomSkinOnInterval = true;
        public int skinReloadIntervalInFrames = 60;
        public bool assignFirstSkinOnCharacterSelection = false;
        #endregion
        #region LocalPlayer Fields
        public ALDOKEMAOMB localLobbyPlayer = null;
        private CharacterModel localLobbyPlayerModel = null;
        private PlayerEntity localGamePlayerEntity = null;
        public string localPlayerName = "";
        public Character localPlayerChar = Character.NONE;
        public CharacterVariant localPlayerCharVar = CharacterVariant.CORPSE;
        public Texture2D localTex = null;
        public string localTexName = "";
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
        public string opponentPlayerName = "";
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
            candySplashWhite = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\candySplashWhite.png");

            gridStartBG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\GridSpecial\gridStartBG.png");
            gridStartFG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\GridSpecial\gridStartFG.png");
            gridTrail = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\GridSpecial\gridTrail.png");
            gridArrive = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\GridSpecial\gridArrive.png");

            bubbleBG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\JetSpecial\bubbleBG.png");
            bubbleFG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\JetSpecial\bubbleFG.png");
            bubblePopBG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\JetSpecial\bubblePopBG.png");
            bubblePopFG = TextureHelper.LoadPNG(TextureMod.resourceFolder + @"Effects\JetSpecial\bubblePopFG.png");
        }

        private void OnGUI()
        {
            if (localTex != null) //Show skin nametags
            {
                GUI.skin.box.wordWrap = false;
                GUIContent content;
                if (!intervalMode) content = new GUIContent(localTexName);
                else content = new GUIContent(localTexName + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.skin.box.fontSize = 22;
                if (InLobby(GameType.Any))
                {
                    if (UIScreen.currentScreens[1] == null)
                    {
                        switch (currentGameMode)
                        {
                            case GameMode.TUTORIAL:
                            case GameMode.TRAINING:
                                GUI.Box(new Rect((Screen.width / 8), (Screen.height / 12.5f), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                break;
                            case GameMode._1v1:
                                if (localLobbyPlayer == ALDOKEMAOMB.BJDPHEHJJJK(0)) GUI.Box(new Rect(Screen.width / 10, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName); //Check if local player is the player with ID 0
                                else GUI.Box(new Rect((Screen.width / 20) * 12.95f, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                break;
                            case GameMode.FREE_FOR_ALL:
                            case GameMode.COMPETITIVE:
                                if (localLobbyPlayer == ALDOKEMAOMB.BJDPHEHJJJK(0)) GUI.Box(new Rect(0 + Screen.width / 250, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                else GUI.Box(new Rect((Screen.width / 4) + (Screen.width / 250), Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                break;
                        }
                    }
                }

                if (UIScreen.currentScreens[1] != null)
                {
                    if (UIScreen.currentScreens[1].screenType == ScreenType.UNLOCKS_SKINS)
                    {
                        if (TextureMod.Instance.showcaseStudio.showUI == false)
                        {
                            TextureMod.Instance.showcaseStudio.skinName = localTexName;
                            TextureMod.Instance.showcaseStudio.refreshTimer = reloadCustomSkinTimer;
                            TextureMod.Instance.showcaseStudio.refreshMode = intervalMode;
                        }
                        else
                        {
                            if (intervalMode) GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                            else GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                        }
                    }
                }
            }
        }  //Show skin nametags

        private void FixedUpdate()
        {
            if (!neverApplyOpponentsSkin)
            {
                if (doSkinPost) { postTimer++; }
                if (localLobbyPlayer != null && localTex != null)
                {
                    if (postTimer >= postTimerLimit)
                    {
                        doSkinPost = false;
                        postTimer = 0;
                        try
                        {
                            StartCoroutine(TextureMod.Instance.ec.PostSkin(localLobbyPlayer.KLEEADMGHNE.peerId, localPlayerChar, localPlayerCharVar, localTex));
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
                            StartCoroutine(TextureMod.Instance.ec.GetSkin(opponentPlayer.KLEEADMGHNE.peerId));
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

        private void Update()
        {

#if DEBUG
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                InitLocalPlayer();
                InitOpponentPlayer();
            } 
#endif

            #region Set MMI Config Vars
            if (TextureMod.Instance.MMI != null)
            {
                var mmi = TextureMod.Instance.MMI;
                holdKey1 = mmi.GetKeyCode("enableSkinChanger");
                nextSkin = mmi.GetKeyCode("nextSkin");
                previousSkin = mmi.GetKeyCode("previousSkin");
                cancelKey = mmi.GetKeyCode("cancelOpponentCustomSkin");
                reloadCustomSkin = mmi.GetKeyCode("reloadCustomSkin");
                reloadEntireSkinLibrary = mmi.GetKeyCode("reloadEntireSkinLibrary");
                useOnlySetKey = mmi.NewGetTrueFalse("noHoldMode");
                neverApplyOpponentsSkin = mmi.GetTrueFalse("neverApplyOpponentsSkin");
                showDebugInfo = mmi.GetTrueFalse("showDebugInfo");
                lockButtonsOnRandom = mmi.GetTrueFalse("lockButtonsOnRandom");
                reloadCustomSkinOnInterval = mmi.GetTrueFalse("reloadCustomSkinOnInterval");
                skinReloadIntervalInFrames = mmi.GetSliderValue("skinReloadIntervalInFrames");
                assignFirstSkinOnCharacterSelection = mmi.GetTrueFalse("assignFirstSkinOnCharacterSelection");
            }
            #endregion
            #region Set Static Vars
            gameState = DNPFJHMAIBP.HHMOGKIMBNM();
            currentGameMode = JOMBNFKIHIC.GIGAKBJGFDI.PNJOKAICMNN;
            #endregion
            #region Set Debug Vars
            if (showDebugInfo)
            {
                ModDebugging md = TextureMod.Instance.md;
                try { md.AddToWindow("General", "Gamestate", gameState.ToString()); } catch { }
                try { md.AddToWindow("General", "GameMode", currentGameMode.ToString()); } catch { }
                try { md.AddToWindow("General", "In Menu", InMenu().ToString()); } catch { }
                try { md.AddToWindow("General", "In Lobby", InLobby(GameType.Any).ToString()); } catch { }
                try { md.AddToWindow("General", "In Game", InGame(GameType.Any).ToString()); } catch { }
                try { md.AddToWindow("General", "In Post Game", InPostGame().ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[0]", UIScreen.currentScreens[0].screenType.ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[1]", UIScreen.currentScreens[1].screenType.ToString()); } catch { }
                try { md.AddToWindow("General", "CurrentScreen[2]", UIScreen.currentScreens[2].screenType.ToString()); } catch { }

                try { md.AddToWindow("Skin Exchange", "Do Skin Post", doSkinPost.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Post Timer", postTimer.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Do Skin Get", doSkinGet.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Get Timer", getTimer.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "New Skin To Apply", newSkinToApply.ToString()); } catch { }
                try { md.AddToWindow("Skin Exchange", "Set Anti Mirror", setAntiMirrior.ToString()); } catch { }

                try { md.AddToWindow("Local Player", "Lobby Player", localLobbyPlayer.ToString()); } catch { md.AddToWindow("Local Player", "Lobby Player", "null"); }
                try { md.AddToWindow("Local Player", "Lobby Player Model", localLobbyPlayerModel.ToString()); } catch { md.AddToWindow("Local Player", "Lobby Player Model", "null"); }
                try { md.AddToWindow("Local Player", "Game PlayerEntity", localGamePlayerEntity.ToString()); } catch { md.AddToWindow("Local Player", "Game PlayerEntity", "null"); }
                try { md.AddToWindow("Local Player", "Name", localPlayerName.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Character", localPlayerChar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Variant", localPlayerCharVar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Custom Texture", localTex.ToString()); } catch { md.AddToWindow("Local Player", "Custom Texture", "null"); }
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
                try { md.AddToWindow("Remote Player", "Name", opponentPlayerName.ToString()); } catch { }
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


            if (opponentPlayer != null && opponentLobbyCharacterModel != null)
            {
                if ((opponentLobbyCharacterModel.character != opponentCustomSkinCharacter || opponentLobbyCharacterModel.characterVariant != opponentCustomSkinCharacterVariant) && (opponentPlayer.DOFCCEDJODB != Character.NONE || opponentPlayer.AIINAIDBHJI != CharacterVariant.CORPSE))
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
                            opponentCustomTexture = TextureHelper.LoadPNG(Path.Combine(Path.Combine(TextureMod.resourceFolder, "Images"), "opponent.png"));
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


            if (localLobbyPlayer != null && randomizedChar == false) // Determine and assign skin to local player
            {
                if (localLobbyPlayer.CHNGAKOIJFE)
                {
                    var setNextSkin = false;
                    var setPreviousSkin = false;

                    if ((localPlayerChar != localLobbyPlayer.DOFCCEDJODB || localPlayerCharVar != localLobbyPlayer.AIINAIDBHJI))
                    {
                        localTex = null;
                        if (assignFirstSkinOnCharacterSelection && localPlayerChar != localLobbyPlayer.DOFCCEDJODB && localLobbyPlayer.DOFCCEDJODB != Character.RANDOM) setNextSkin = true;
                        else initLocalPlayer = true;
                        localPlayerChar = localLobbyPlayer.DOFCCEDJODB;
                        localPlayerCharVar = localLobbyPlayer.AIINAIDBHJI;

                    }

                    LLButton[] buttons = FindObjectsOfType<LLButton>();
                    HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();



                    if (useOnlySetKey == false)
                    {
                        if (Input.GetKey(holdKey1) && buttons.Length > 0)
                        {
                            foreach (LLButton b in buttons) b.SetActive(false); //Deactivate buttons
                            if (Input.GetKeyDown(nextSkin)) setNextSkin = true;
                            else if (Input.GetKeyDown(previousSkin)) setPreviousSkin = true;

                        }
                        else if (Input.GetKeyUp(holdKey1) && buttons.Length > 0)
                        {
                            foreach (LLButton b in buttons) b.SetActive(true); //Reactivate buttons
                        }
                    }
                    else if (Input.GetKeyDown(nextSkin)) setNextSkin = true;
                    else if (Input.GetKeyDown(previousSkin)) setPreviousSkin = true;

                    if ((setNextSkin || setPreviousSkin) && InLobby(GameType.Any)) // Assign skin to local player
                    {
                        if (setAntiMirrior)
                        {
                            opponentCustomTexture = TextureHelper.LoadPNG(Application.dataPath.Replace("/", @"\") + @"\Managed\TextureModResources\Images\opponent.png");
                            setAntiMirrior = false;
                        }
                        if (InLobby(GameType.Online))
                        {
                            gameStatesOnlineLobby.JPNNBHNHHJC();
                            gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, false); //Set Ready
                            gameStatesOnlineLobby.BFIGLDLHKPO();
                            gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer);
                        }

                        if (localLobbyPlayer.DOFCCEDJODB == Character.RANDOM) // Randomize skin and char
                        {
                            Character randomChar = localLobbyPlayer.HGPNPNPJBMK();
                            localLobbyPlayer.DOFCCEDJODB = randomChar;
                            localLobbyPlayer.LALEEFJMMLH = randomChar;

                            localTex = GetLoadedTexture(randomChar, localTex, false, true);
                            if (InLobby(GameType.Online))
                            {
                                gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, true); //Set Ready
                                gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer); //Send player state (Signalizes that we have changes characters and that we are ready)

                                if (lockButtonsOnRandom)
                                {
                                    foreach (LLButton b in buttons) b.SetActive(false);
                                    randomizedChar = true;
                                }
                            }
                        }
                        else
                        {
                            if (setNextSkin) localTex = GetLoadedTexture(localLobbyPlayer.DOFCCEDJODB, localTex, false, false);
                            if (setPreviousSkin) localTex = GetLoadedTexture(localLobbyPlayer.DOFCCEDJODB, localTex, true, false);
                        }

                        localLobbyPlayerModel.PlayCamAnim();

                        if (InLobby(GameType.Online))
                        {
                            doSkinPost = true;
                            postTimer = 0;
                            setAntiMirrior = false;
                            calculateMirror = true;
                        }
                    }
                }
            }

            if (randomizedChar && UIScreen.currentScreens[1] != null) // If you have randomized your character, activate buttons again
            {
                if (UIScreen.currentScreens[1].screenType == ScreenType.PLAYERS_STAGE || UIScreen.currentScreens[1].screenType == ScreenType.PLAYERS_STAGE_RANKED)
                {
                    LLButton[] buttons = FindObjectsOfType<LLButton>();
                    foreach (LLButton b in buttons) b.SetActive(true);
                }
            }

            if (opponentPlayer != null && opponentCustomTexture != null && localTex != null && InLobby(GameType.Any) && calculateMirror)
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
                    Color[] pt = localTex.GetPixels();
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

            if (localTex == null || opponentCustomTexture == null) { setAntiMirrior = false; }


            switch (currentGameMode)
            {
                case GameMode.TRAINING:
                case GameMode.TUTORIAL:
                    #region In training and tutorial
                    if (initLocalPlayer) { InitLocalPlayer(); }
                    if (InLobby(GameType.Offline))
                    {
                        if (localLobbyPlayer == null) { localLobbyPlayer = GetLocalPlayerInLobby(GameType.Offline); }
                        else
                        {
                            if (localLobbyPlayerModel == null) { localLobbyPlayerModel = GetLobbyCharacterModel(localLobbyPlayer.CJFLMDNNMIE); }
                            else
                            {
                                localLobbyPlayerModel.SetSilhouette(false);
                                if (localTex != null)
                                {
                                    AssignTextureToCharacterModelRenderers(localLobbyPlayerModel, localTex);
                                }
                            }
                        }
                    }
                    if (InGame(GameType.Offline))
                    {
                        if (localGamePlayerEntity == null) { localGamePlayerEntity = GetLocalPlayerInGame(); }
                        else
                        {
                            if (localTex != null)
                            {
                                if (Input.GetKeyDown(reloadCustomSkin))
                                {
                                    try { localTex = TextureHelper.ReloadSkin(localPlayerChar, localTex); }
                                    catch { LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_BACK); }
                                }

                                AssignTextureToIngameCharacter(localGamePlayerEntity, localTex);
                                localPlayerName = AssignTextureToHud(localGamePlayerEntity, localTex);
                            }
                        }
                    }
                    break;
                #endregion
                case GameMode._1v1:
                case GameMode.FREE_FOR_ALL:
                case GameMode.COMPETITIVE:
                    #region In ranked and online lobby
                    if (Input.GetKeyDown(cancelKey))
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
                        if (initLocalPlayer) { InitLocalPlayer(); }
                        if (initOpponentPlayer) { InitOpponentPlayer(); }

                        if (localLobbyPlayer == null)
                        {
                            if (InLobby(GameType.Online)) localLobbyPlayer = GetLocalPlayerInLobby(GameType.Online);
                            else localLobbyPlayer = GetLocalPlayerInLobby(GameType.Offline);
                        }
                        else
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

                            //if (localLobbyPlayer.CJFLMDNNMIE == 3) initLocalPlayer = true;

                            if (localLobbyPlayerModel == null) { localLobbyPlayerModel = GetLobbyCharacterModel(localLobbyPlayer.CJFLMDNNMIE); }
                            else
                            {
                                localLobbyPlayerModel.SetSilhouette(false);
                                if (localTex != null)
                                {
                                    AssignTextureToCharacterModelRenderers(localLobbyPlayerModel, localTex);
                                }
                            }
                            if (InLobby(GameType.Online))
                            {
                                if (opponentPlayer == null) { opponentPlayer = GetOpponentPlayerInLobby(); }
                                else
                                {
                                    doSkinGet = true;
                                    if (opponentLobbyCharacterModel == null) { opponentLobbyCharacterModel = GetLobbyCharacterModel(opponentPlayer.CJFLMDNNMIE); }
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
                    }
                    else if (InGame(GameType.Any))
                    {
                        if (localGamePlayerEntity == null) { localGamePlayerEntity = GetLocalPlayerInGame(); }
                        else
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
                            if (localTex != null)
                            {
                                AssignTextureToIngameCharacter(localGamePlayerEntity, localTex);
                                localPlayerName = AssignTextureToHud(localGamePlayerEntity, localTex);
                            }
                        }
                        if (InGame(GameType.Online))
                        {
                            if (opponentCustomTexture != null)
                            {
                                if (opponentPlayerEntity == null) { opponentPlayerEntity = GetOpponentPlayerInGame(); }
                                else
                                {
                                    AssignTextureToIngameCharacter(opponentPlayerEntity, opponentCustomTexture);
                                    opponentPlayerName = AssignTextureToHud(opponentPlayerEntity, opponentCustomTexture);
                                }
                            }
                        }
                    }
                    else if (InPostGame())
                    {
                        AssignSkinToWinnerModel();

                        if (localTex != null) { AssignTextureToPostGameHud(localPlayerName, localTex); }
                        if (opponentCustomTexture != null) AssignTextureToPostGameHud(opponentPlayerName, opponentCustomTexture);
                    }
                    else
                    {
                        initLocalPlayer = true;
                        initOpponentPlayer = true;
                    }
                    break;
                    #endregion
            }


            if (InMenu())
            {
                sendCancelRequestToServer = true;
                if (UIScreen.currentScreens[1] != null)
                {
                    if (UIScreen.currentScreens[1].screenType != ScreenType.UNLOCKS_SKINS)
                    {
                        InitLocalPlayer();
                    }
                }
                InitOpponentPlayer();
            }

            if (UIScreen.currentScreens[1] != null)
            {
                if (UIScreen.currentScreens[1].screenType == ScreenType.UNLOCKS_SKINS)
                {
                    if (siluetteTimer > 0)
                    {
                        CharacterModel[] cms = FindObjectsOfType<CharacterModel>();
                        foreach (CharacterModel cm in cms) cm.SetSilhouette(false);
                        if (localTex != null) SetUnlocksCharacterModel(localTex);
                    }

                    if (Input.GetKey(holdKey1))
                    {
                        if (Input.GetKeyDown(nextSkin))
                        {
                            try
                            {
                                localTex = GetLoadedTextureForUnlocksModel(localTex, false);
                                SetUnlocksCharacterModel(localTex);
                            }
                            catch { }
                        }
                        else if (Input.GetKeyDown(previousSkin))
                        {
                            try
                            {
                                localTex = GetLoadedTextureForUnlocksModel(localTex, true);
                                SetUnlocksCharacterModel(localTex);
                            }
                            catch { }
                        }
                    }

                    if (localTex != null) // Reload a skin from its file
                    {
                        if (Input.GetKeyDown(reloadCustomSkin))
                        {
                            if (!intervalMode)
                            {
                                if (reloadCustomSkinOnInterval)
                                {
                                    intervalMode = true;
                                    reloadCustomSkinTimer = skinReloadIntervalInFrames;
                                }
                            }
                            else intervalMode = false;

                            try
                            {
                                ScreenUnlocksSkins sus = FindObjectOfType<ScreenUnlocksSkins>();
                                localTex = TextureHelper.ReloadSkin(sus.character, localTex);
                                SetUnlocksCharacterModel(localTex);
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
                                    ScreenUnlocksSkins sus = FindObjectOfType<ScreenUnlocksSkins>();
                                    localTex = TextureHelper.ReloadSkin(sus.character, localTex);
                                    SetUnlocksCharacterModel(localTex);
                                }
                                catch { LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_BACK); }
                                reloadCustomSkinTimer = skinReloadIntervalInFrames;
                            }
                        }
                    }
                }
                else if (UIScreen.currentScreens[1].screenType == ScreenType.UNLOCKS_CHARACTERS)
                {
                    localTex = null;
                    intervalMode = false;
                    reloadCustomSkinTimer = skinReloadIntervalInFrames;
                }
            }

            if (Input.GetKeyDown(reloadEntireSkinLibrary)) TextureMod.Instance.tl.LoadLibrary(); //Reloads the entire texture folder

            if (InGame(GameType.Any))
            {
                if (mainBall == null) mainBall = BallHandler.instance.GetBall(0);
                if (playersInCurrentGame.Count == 0) playersInCurrentGame = GetCharactersInGame();
                else
                {
                    foreach (Character character in playersInCurrentGame)
                    {
                        switch (character)
                        {
                            case Character.CANDY: CandymanIngameEffects(); break;
                            case Character.BAG: AshesIngameEffects(); break;
                            case Character.GRAF: ToxicIngameEffects(); break;
                            case Character.ELECTRO: GridIngameEffects(); break;
                            case Character.SKATE: JetIngameEffects(); break;
                        }
                    }
                }
            }
            else
            {
                mainBall = null;
                if (playersInCurrentGame.Count > 0) playersInCurrentGame.Clear();
            }
        }

        /// End of Update()
        /// End of Update()
        /// End of Update()
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
            if (UIScreen.currentScreens[0] != null)
            {
                if (UIScreen.currentScreens[0].screenType == ScreenType.MENU)
                { return true; }
                else { return false; }
            }
            else return false;
        }
        public bool InLobby(GameType gt)
        {
            switch (gt)
            {
                case GameType.Online:
                    if (gameState == (JOFJHDJHJGI)5)
                    { return true; }
                    break;
                case GameType.Offline:
                    if (gameState == (JOFJHDJHJGI)11 || gameState == (JOFJHDJHJGI)12 || gameState == (JOFJHDJHJGI)4 || gameState == (JOFJHDJHJGI)6 || gameState == (JOFJHDJHJGI)23)
                    { return true; }
                    break;
                case GameType.Any:
                    if (gameState == (JOFJHDJHJGI)11 || gameState == (JOFJHDJHJGI)12 || gameState == (JOFJHDJHJGI)5 || gameState == (JOFJHDJHJGI)6 || gameState == (JOFJHDJHJGI)4 || gameState == (JOFJHDJHJGI)23)
                    { return true; }
                    break;
            }
            return false;
        }
        public bool InGame(GameType gt)
        {
            switch (gt)
            {
                case GameType.Online:
                    if (JOMBNFKIHIC.GDNFJCCCKDM && gameState == (JOFJHDJHJGI)19)
                    { return true; }
                    break;
                case GameType.Offline:
                    if ((!JOMBNFKIHIC.GDNFJCCCKDM) && gameState == (JOFJHDJHJGI)19)
                    { return true; }
                    break;
                case GameType.Any:
                    if ((!JOMBNFKIHIC.GDNFJCCCKDM || JOMBNFKIHIC.GDNFJCCCKDM) && gameState == (JOFJHDJHJGI)19)
                    { return true; }
                    break;
            }
            return false;
        }
        public bool InPostGame()
        {
            if ((currentGameMode == GameMode._1v1 || currentGameMode == GameMode.FREE_FOR_ALL) && gameState == (JOFJHDJHJGI)21)
            { return true; }
            else { return false; }
        }

        private ALDOKEMAOMB GetLocalPlayerInLobby(GameType gt)
        {
            ALDOKEMAOMB player = null;
            switch (gt)
            {
                case GameType.Online:
                    ScreenPlayers sp = FindObjectOfType<ScreenPlayers>();
                    if (sp.isActive)
                    {
                        bool inLobby = false;
                        foreach (PlayersSelection ps in sp.playerSelections)
                        {
                            if (ps.btPlayerName.isActiveAndEnabled) inLobby = true;
                        }
                        if (inLobby)
                        {
                            HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();
                            if (gameStatesOnlineLobby != null)
                            {
                                Debug.Log("Assigned player nr [" + gameStatesOnlineLobby.HCCGAFLIABM.ToString() + "] as the local player");
                                return ALDOKEMAOMB.BJDPHEHJJJK(gameStatesOnlineLobby.HCCGAFLIABM);
                            }
                        }
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
                if (localLobbyPlayer.CJFLMDNNMIE == 0) player = ALDOKEMAOMB.BJDPHEHJJJK(1);
                else if (localLobbyPlayer.CJFLMDNNMIE == 1) player = ALDOKEMAOMB.BJDPHEHJJJK(0);
            }

            if (player.DOFCCEDJODB != Character.NONE && player.DOFCCEDJODB != Character.RANDOM) return player;
            else return null;
        }

        public CharacterModel GetLobbyCharacterModel(int playerNr)
        {
            CharacterModel retmodel = null;
            ScreenPlayers screenPlayers = FindObjectOfType<ScreenPlayers>();
            foreach (PlayersSelection playersSelection in screenPlayers.playerSelections)
            {
                if (playersSelection.playerNr == playerNr)
                {
                    retmodel = playersSelection.characterModel;
                }
            }
            return retmodel;
        }

        public void AssignTextureToCharacterModelRenderers(CharacterModel model, Texture2D tex)
        {
            try
            {
                Renderer[] rs = model.curModel.transform.GetComponentsInChildren<Renderer>();
                if (rs.Length > 0)
                {
                    foreach (Renderer r in rs)
                    {
                        switch (model.character)
                        {
                            case Character.BAG:
                                AssignAshesOutlineColorToRenderer(r, model.characterVariant, tex);
                                break;
                            case Character.BOSS:
                                if (model.characterVariant == CharacterVariant.MODEL_ALT3 || model.characterVariant == CharacterVariant.MODEL_ALT4) AssignDoomBoxVisualizerColorToRenderer(r, tex);
                                else if (model.characterVariant == CharacterVariant.MODEL_ALT || model.characterVariant == CharacterVariant.MODEL_ALT2) AssignOmegaDoomboxSmearsAndArmsToRenderer(r, tex);
                                break;
                            case Character.GRAF:
                                if (model.characterVariant == CharacterVariant.MODEL_ALT3 || model.characterVariant == CharacterVariant.MODEL_ALT4) AssignNurseToxicCanistersToRenderer(r, tex);
                                break;
                        }

                        r.material.SetTexture("_MainTex", tex);
                    }

                }
            }
            catch { }
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
                        if (!r.gameObject.name.EndsWith("Outline"))
                        {
                            r.material.SetTexture("_MainTex", tex);
                        }
                    }

                    if (playerEntity.character == Character.GRAF)
                    {
                        AssignToxicEffectColors(playerEntity.player.CJFLMDNNMIE, tex, playerEntity.variant);
                    }
                    else if (playerEntity.character == Character.BOSS)
                    {
                        if (playerEntity.variant == CharacterVariant.MODEL_ALT3 || playerEntity.variant == CharacterVariant.MODEL_ALT4) AssignDoomBoxVisualizerColorIngame(playerEntity, tex);
                        else if (playerEntity.variant == CharacterVariant.MODEL_ALT || playerEntity.variant == CharacterVariant.MODEL_ALT2) AssignOmegaDoomboxSmearsAndArmsIngame(playerEntity, tex);
                    }
                }
            }
        }

        private string AssignTextureToHud(PlayerEntity playerEntity, Texture tex)
        {
            string ret = "";
            GameHudPlayerInfo[] ghpis = FindObjectsOfType<GameHudPlayerInfo>();
            if (ghpis.Length > 0)
            {
                foreach (GameHudPlayerInfo ghpi in ghpis)
                {
                    if (ghpi.shownPlayer == playerEntity.player)
                    {
                        Renderer[] rs = ghpi.gameObject.transform.GetComponentsInChildren<Renderer>();
                        if (rs.Length > 0)
                        {
                            foreach (Renderer r in rs)
                            {
                                r.material.SetTexture("_MainTex", tex);
                                if (playerEntity.character == Character.GRAF)
                                {
                                    CharacterVariant variant = playerEntity.variant;
                                    if (variant == CharacterVariant.MODEL_ALT3 || variant == CharacterVariant.MODEL_ALT4)
                                    {
                                        AssignNurseToxicCanistersToRenderer(r, (Texture2D)tex);
                                    }
                                }
                            }
                        }
                        ret = ghpi.lbName.text;
                    }
                }
            }
            return ret;
        }

        private void AssignTextureToPostGameHud(string playerName, Texture tex)
        {
            PostScreen ps = FindObjectOfType<PostScreen>();
            PostSceenPlayerBar[] pspbs = ps.playerBarsByPlayer;
            if (pspbs.Length > 0)
            {
                foreach (PostSceenPlayerBar pspb in pspbs)
                {
                    try
                    {
                        if (pspb.btPlayerName.GetText() == playerName)
                        {
                            Renderer[] rs = pspb.gameObject.transform.GetComponentsInChildren<Renderer>();
                            if (rs.Length > 0) foreach (Renderer r in rs) r.material.SetTexture("_MainTex", tex);
                        }
                    }
                    catch { }
                }
            }
        }

        private void AssignSkinToWinnerModel()
        {
            PostScreen ps = FindObjectOfType<PostScreen>();
            if (ps != null)
            {
                if (localTex != null)
                {
                    try
                    {
                        if (ps.winnerCharacter == localPlayerChar && ps.winnerCharacterVariant == localPlayerCharVar) AssignTextureToCharacterModelRenderers(ps.winnerModel, localTex);
                    }
                    catch (Exception ex) { Debug.Log("Winner Model: " + ex); }
                }

                if (opponentCustomTexture != null)
                {
                    try
                    {
                        if (ps.winnerCharacter == opponentCustomSkinCharacter && ps.winnerCharacterVariant == opponentCustomSkinCharacterVariant) AssignTextureToCharacterModelRenderers(ps.winnerModel, opponentCustomTexture);
                    }
                    catch (Exception ex) { Debug.Log("Winner Model: " + ex); }
                }
            }
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
            localLobbyPlayer = null;
            localLobbyPlayerModel = null;
            localGamePlayerEntity = null;
            localPlayerName = "";
            localTex = null;
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
            opponentPlayerName = "";
            opponentCustomTexture = null;
            doSkinGet = false;
            getTimer = 0;
            initOpponentPlayer = false;
        }

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
            localTexName = texname;
            return ret;
        }

        private Texture2D GetLoadedTextureForUnlocksModel(Texture2D currentTexture, bool previous)
        {
            Texture2D ret = null;
            var texname = "";
            ScreenUnlocksSkins sus = FindObjectOfType<ScreenUnlocksSkins>();

            if (sus != null)
            {
                if (previous)
                {
                    if (currentTexture == null && TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value != null)
                    {
                        sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].Last().Key), false);
                        texname = TextureMod.Instance.tl.characterTextures[sus.character].Last().Key;
                        ret = TextureMod.Instance.tl.characterTextures[sus.character].Last().Value;
                    }
                    else
                    {
                        bool retnext = false;
                        for (int i = TextureMod.Instance.tl.characterTextures[sus.character].Count - 1; i != -1; i--)
                        {
                            KeyValuePair<string, Texture2D> pair = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(i);
                            if (retnext == true)
                            {
                                sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, pair.Key), false);
                                texname = pair.Key;
                                ret = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == TextureMod.Instance.tl.characterTextures[sus.character].First().Value)
                                {
                                    sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].Last().Key), false);
                                    texname = TextureMod.Instance.tl.characterTextures[sus.character].Last().Key;
                                    ret = TextureMod.Instance.tl.characterTextures[sus.character].Last().Value;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (currentTexture == null && TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value != null)
                    {
                        sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key), false);
                        texname = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key;
                        ret = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value;
                    }
                    else
                    {
                        bool retnext = false;
                        foreach (KeyValuePair<string, Texture2D> pair in TextureMod.Instance.tl.characterTextures[sus.character])
                        {
                            if (retnext == true)
                            {
                                sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, pair.Key), false);
                                texname = pair.Key;
                                ret = pair.Value;
                                break;
                            }
                            else if (retnext == false && currentTexture == pair.Value)
                            {
                                retnext = true;
                                if (currentTexture == TextureMod.Instance.tl.characterTextures[sus.character].Last().Value)
                                {
                                    sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key), false);
                                    texname = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key;
                                    ret = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value;
                                }
                            }
                        }
                    }
                }
            }

            texname = CleanTextureName(texname);

            siluetteTimer = 5;

            localTexName = texname;
            return ret;
        }

        private string CleanTextureName(string texname)
        {
            string ret = texname;
            if (ret.Contains(".png")) ret = ret.Replace(".png", "");
            if (ret.Contains(".PNG")) ret = ret.Replace(".PNG", "");
            if (ret.Contains("_ALT2")) ret = ret.Replace("_ALT2", "");
            else if (ret.Contains("_ALT")) ret = ret.Replace("_ALT", "");

            return ret;
        }

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

                    switch (localPlayerC)
                    {
                        case Character.PONG:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Dice") proceed = true;
                            break;
                        case Character.KID:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Raptor") proceed = true;
                            break;
                        case Character.BAG:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Dust&Ashes") proceed = true;
                            break;
                        case Character.BOSS:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Doombox") proceed = true;
                            break;
                        case Character.GRAF:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Toxic") proceed = true;
                            break;
                        case Character.ROBOT:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Switch") proceed = true;
                            break;
                        case Character.CROC:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Latch") proceed = true;
                            break;
                        case Character.BOOM:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Sonata") proceed = true;
                            break;
                        case Character.ELECTRO:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Grid") proceed = true;
                            break;
                        case Character.SKATE:
                            foreach (string DLC in TextureMod.ownedDLCs) if (DLC == "Jet") proceed = true;
                            break;
                        default:
                            proceed = false;
                            break;
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

        #region Dust&Ashes Effects
        public void AshesIngameEffects()
        {
            if (localGamePlayerEntity != null && localTex != null) if (localGamePlayerEntity.character == Character.BAG) AssignAshesOutlineColor(localGamePlayerEntity, localTex);
                else if (opponentPlayerEntity != null && opponentCustomTexture != null) if (opponentPlayerEntity.character == Character.BAG) AssignAshesOutlineColor(opponentPlayerEntity, opponentCustomTexture);
        }


        public void AssignAshesOutlineColor(PlayerEntity pe, Texture2D tex)
        {
            Color c = new Color(1, 1, 1, 1);
            switch (pe.variant)
            {
                case CharacterVariant.DEFAULT: c = tex.GetPixel(58, 438); break;
                case CharacterVariant.ALT0: c = tex.GetPixel(58, 438); break;
                case CharacterVariant.MODEL_ALT: c = tex.GetPixel(69, 298); break;
                case CharacterVariant.MODEL_ALT2: c = tex.GetPixel(69, 298); break;
                case CharacterVariant.MODEL_ALT3: c = tex.GetPixel(113, 334); break;
                case CharacterVariant.MODEL_ALT4: c = tex.GetPixel(113, 334); break;
            }

            foreach (Renderer r in pe.skinRenderers)
            {
                if (r.name != "mesh1Outline" && r.name != "mesh1MetalOutline" && r.name != "mesh1TenguOutline" && r.name.Contains("Outline")) r.material.color = c;
            }
        }

        public void AssignAshesOutlineColorToRenderer(Renderer r, CharacterVariant variant, Texture2D tex)
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
                    if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && localTex != null) AssignSkinToCandyball(mainBall, mainBall.GetLastPlayerHitter(), localTex);
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
                VisualEntity pve = pe.gameObject.GetComponent<VisualEntity>();
                tex = (Texture2D)pve.skinRenderers.First().material.mainTexture;
            }

            VisualEntity ve = ball.gameObject.GetComponentInChildren<VisualEntity>();
            foreach (Renderer r in ve.skinRenderers)
            {
                if (r.name == "mesh001_MainRenderer" || r.name == "mesh001Strait_MainRenderer")
                {
                    r.material.mainTexture = tex;
                }
            }
        }

        public void AssignSkinColorToCandySplash()
        {
            VisualEntity[] ves = FindObjectsOfType<VisualEntity>();
            foreach (VisualEntity v in ves)
            {
                if (v.name == "candySplash")
                {
                    Texture2D tex = null;
                    CandyPlayer[] cps = FindObjectsOfType<CandyPlayer>();
                    foreach (CandyPlayer cp in cps)
                    {
                        if (cp.player.CJFLMDNNMIE == localGamePlayerEntity.player.CJFLMDNNMIE && localTex != null)
                        {
                            if (localGamePlayerEntity.GetCurrentAbilityState().name.Contains("CROUCH") && (localGamePlayerEntity.moveableData.onLeftWallApprox || localGamePlayerEntity.moveableData.onRightWallApprox) && !localGamePlayerEntity.moveableData.velocity.HCBCKAHGJCA(IBGCBLLKIHA.DBOMOJGKIFI)) tex = localTex;
                            if (mainBall.ballData.ballState == BallState.CANDYBALL && mainBall.GetLastPlayerHitter() == localGamePlayerEntity) tex = localTex;
                        }

                        if (opponentPlayerEntity != null && opponentCustomTexture != null && tex == null)
                        {
                            if (cp.player.CJFLMDNNMIE == opponentPlayerEntity.player.CJFLMDNNMIE && opponentCustomTexture != null)
                            {
                                if ((opponentPlayerEntity.moveableData.onLeftWallApprox || opponentPlayerEntity.moveableData.onRightWallApprox) && !opponentPlayerEntity.moveableData.velocity.HCBCKAHGJCA(IBGCBLLKIHA.DBOMOJGKIFI)) tex = opponentCustomTexture;
                                if (mainBall.ballData.ballState == BallState.CANDYBALL && mainBall.GetLastPlayerHitter() == opponentPlayerEntity) tex = opponentCustomTexture;
                            }
                        }
                    }
                    Renderer[] rs = v.GetComponentsInChildren<Renderer>();
                    foreach (var r in rs)
                    {
                        r.material.mainTexture = candySplashWhite;
                        r.material.color = tex.GetPixel(103, 473);
                        v.name = "candySplashModified";
                    }
                }
            }
        }
        #endregion
        #region Grid effects
        public void GridIngameEffects()
        {
            if (mainBall.ballData.hitstunState == HitstunState.TELEPORT_STUN || mainBall.ballData.hitstunState == HitstunState.BUNT_STUN)
            {
                if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && localTex != null) AssignGridSpecialColor(localGamePlayerEntity, localTex);
                if (mainBall.GetLastPlayerHitter() == opponentPlayerEntity && opponentCustomTexture != null) AssignGridSpecialColor(opponentPlayerEntity, opponentCustomTexture);
            }
            else
            {
                if (localGamePlayerEntity.GetCurrentAbilityState().name.Contains("ELECTROCHARGE") && localTex != null) AssignGridSpecialColor(localGamePlayerEntity, localTex);
                if (opponentPlayerEntity.GetCurrentAbilityState().name.Contains("ELECTROCHARGE") && opponentCustomTexture != null) AssignGridSpecialColor(opponentPlayerEntity, opponentCustomTexture);
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
        public void AssignDoomBoxVisualizerColorIngame(PlayerEntity pe, Texture2D tex)
        {
            Color c1 = tex.GetPixel(493, 510);
            Color c2 = tex.GetPixel(508, 510);
            FNDGCLEDHAD visualizer = pe.GetVisual("main").mainRenderer.gameObject.GetComponent<FNDGCLEDHAD>();
            Material vismat = visualizer.FHAMOPAJHNJ;
            vismat.SetColor("_AmpColor0", c1);
            vismat.SetColor("_AmpColor1", c2);
        }

        public void AssignDoomBoxVisualizerColorToRenderer(Renderer r, Texture2D tex)
        {
            if (r.gameObject.GetComponent<FNDGCLEDHAD>() != null)
            {
                Color c1 = tex.GetPixel(493, 510);
                Color c2 = tex.GetPixel(508, 510);
                FNDGCLEDHAD visualizer = r.gameObject.GetComponent<FNDGCLEDHAD>();
                Material vismat = visualizer.FHAMOPAJHNJ;
                vismat.SetColor("_AmpColor0", c1);
                vismat.SetColor("_AmpColor1", c2);
            }
        }

        public void AssignOmegaDoomboxSmearsAndArmsIngame(PlayerEntity pe, Texture2D tex)
        {

            Color arm1 = tex.GetPixel(28, 336);
            Color arm2 = tex.GetPixel(28, 325);

            Color bright1 = tex.GetPixel(113, 336);
            Color bright2 = tex.GetPixel(113, 325);

            Color alpha = tex.GetPixel(178, 332);

            VisualEntity ve = pe.gameObject.GetComponent<VisualEntity>();
            if (ve != null)
            {
                if (ve.skinRenderers.Count > 0)
                {
                    foreach (Renderer r in ve.skinRenderers)
                    {
                        foreach (Material m in r.materials)
                        {
                            if (m.name.Contains("bossOmegaGlassMat") || m.name.Contains("bossOmegaEffectMat"))
                            {
                                m.SetColor("_LitColor", new Color(arm1.r, arm1.g, arm1.b, bright1.r));
                                m.SetColor("_ShadowColor", new Color(arm2.r, arm2.g, arm2.b, bright2.r));
                                m.SetFloat("_Transparency", alpha.r);
                            }
                        }
                    }
                }
            }
        }

        public void AssignOmegaDoomboxSmearsAndArmsToRenderer(Renderer r, Texture2D tex)
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
            if (localGamePlayerEntity != null && localTex != null)
            {
                if (localGamePlayerEntity.character == Character.GRAF && localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT3 || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT4) AssignNurseToxicCanistersIngame(localGamePlayerEntity, localTex);
            }
            if (opponentPlayerEntity != null && opponentCustomTexture != null)
            {
                if (opponentPlayerEntity.character == Character.GRAF && opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT3 || opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT4) AssignNurseToxicCanistersIngame(opponentPlayerEntity, opponentCustomTexture);
            }
        }

        public void AssignNurseToxicCanistersIngame(PlayerEntity pe, Texture2D tex)
        {

            Color light = tex.GetPixel(158, 414);
            Color shad = tex.GetPixel(158, 406);

            Color bright1 = tex.GetPixel(158, 397);
            Color bright2 = tex.GetPixel(158, 389);

            Color alpha = tex.GetPixel(158, 380);

            VisualEntity ve = pe.gameObject.GetComponent<VisualEntity>();
            if (ve != null)
            {
                if (ve.skinRenderers.Count > 0)
                {
                    foreach (Renderer r in ve.skinRenderers)
                    {
                        if (r.name == "meshNurse_MainRenderer")
                        {
                            foreach (Material m in r.materials)
                            {
                                if (m.name.Contains("grafNurseGlass"))
                                {
                                    m.SetColor("_LitColor", new Color(light.r, light.g, light.b, bright1.r));
                                    m.SetColor("_ShadowColor", new Color(shad.r, shad.g, shad.b, bright2.r));
                                    m.SetFloat("_Transparency", alpha.r);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AssignNurseToxicCanistersToRenderer(Renderer r, Texture2D tex)
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
        public void JetIngameEffects()
        {
            if (mainBall.GetLastPlayerHitter() == localGamePlayerEntity && localTex != null) AssignBubbleVisual(localGamePlayerEntity.variant, localTex);
            if (mainBall.GetLastPlayerHitter() == opponentPlayerEntity && opponentCustomTexture != null) AssignBubbleVisual(opponentPlayerEntity.variant, opponentCustomTexture);
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

