using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameplayEntities;
using LLScreen;
using LLGUI;
using LLHandlers;

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


        public Color32[] originalDNAColors = new Color32[AOIOFOIHOCJ.outfitOutlineColors.Length];

        #endregion
        #region Config Fields
        private KeyCode holdKey1 = KeyCode.LeftShift;
        private KeyCode holdKey2 = KeyCode.RightShift;
        private KeyCode setSkinKey = KeyCode.Mouse0;
        private KeyCode cancelKey = KeyCode.A;
        private KeyCode reloadCustomSkin = KeyCode.F5;
        private KeyCode reloadEntireSkinLibrary = KeyCode.F9;
        private bool useOnlySetKey = false;
        private bool neverApplyOpponentsSkin = false;
        public bool showDebugInfo = false;
        private bool lockButtonsOnRandom = false;
        public bool reloadCustomSkinOnInterval = true;
        public int skinReloadIntervalInFrames = 60;
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

        private void Start()
        {
            SaveOriginalAshesColors();
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
                if (InLobby(GameType.Any)) //Show skin nametags
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
                                else GUI.Box(new Rect((Screen.width / 20)*12.95f, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                break;
                            case GameMode.FREE_FOR_ALL:
                            //case GameMode.COMPETITIVE:
                                if (localLobbyPlayer == ALDOKEMAOMB.BJDPHEHJJJK(0)) GUI.Box(new Rect(0 + Screen.width/250, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                else GUI.Box(new Rect((Screen.width/4) + (Screen.width / 250), Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                                break;
                        }
                    }
                }

                if (UIScreen.currentScreens[1] != null)
                {
                    if (UIScreen.currentScreens[1].screenType == ScreenType.UNLOCKS_SKINS)
                    {
                        if (intervalMode) GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                        else GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), localTexName);
                    }
                }
            }
        }

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
            #region Set MMI Config Vars
            if (TextureMod.Instance.MMI != null)
            {
                var mmi = TextureMod.Instance.MMI;
                holdKey1 = mmi.NewGetKeyCode("holdToEnableSkinChanger");
                holdKey2 = mmi.NewGetKeyCode("holdToEnableSkinChanger2");
                setSkinKey = mmi.NewGetKeyCode("setCustomSkin");
                cancelKey = mmi.NewGetKeyCode("cancelOpponentCustomSkin");
                reloadCustomSkin = mmi.NewGetKeyCode("reloadCustomSkin");
                reloadEntireSkinLibrary = mmi.NewGetKeyCode("reloadEntireSkinLibrary");
                useOnlySetKey = mmi.NewGetTrueFalse("noHoldMode");
                neverApplyOpponentsSkin = mmi.NewGetTrueFalse("neverApplyOpponentsSkin");
                showDebugInfo = mmi.NewGetTrueFalse("showDebugInfo");
                lockButtonsOnRandom = mmi.NewGetTrueFalse("lockButtonsOnRandom");
                reloadCustomSkinOnInterval = mmi.NewGetTrueFalse("reloadCustomSkinOnInterval");
                skinReloadIntervalInFrames = mmi.NewGetSliderValue("skinReloadIntervalInFrames");
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

                try { md.AddToWindow("Local Player", "Lobby Player", localLobbyPlayer.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Lobby Player Model", localLobbyPlayerModel.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Game PlayerEntity", localGamePlayerEntity.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Name", localPlayerName.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Character", localPlayerChar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Variant", localPlayerCharVar.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Custom Texture", localTex.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Initiate Player", initLocalPlayer.ToString()); } catch { }
                try { md.AddToWindow("Local Player", "Randomized Character", randomizedChar.ToString()); } catch { }

                try { md.AddToWindow("Remote Player", "Lobby Player", opponentPlayer.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Lobby Player Model", opponentLobbyCharacterModel.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Game PlayerEntity", opponentPlayerEntity.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Name", opponentPlayerName.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Character", opponentCustomSkinCharacter.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Customskin Variant", opponentCustomSkinCharacterVariant.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Custom Texture", opponentCustomTexture.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Initiate Player", initOpponentPlayer.ToString()); } catch { }
                try { md.AddToWindow("Remote Player", "Cancel Skin", cancelOpponentSkin.ToString()); } catch { }
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
                        opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, packetSkinCharacter, packetSkinCharacterVariant, false);
                    }
                    else
                    {
                        if (opponentPlayer.DOFCCEDJODB != Character.NONE && opponentPlayer.DOFCCEDJODB != (Character)32 && opponentPlayer.AIINAIDBHJI != CharacterVariant.CORPSE)
                        {
                            opponentCustomSkinCharacter = opponentPlayer.DOFCCEDJODB;
                            opponentCustomSkinCharacterVariant = opponentPlayer.AIINAIDBHJI;
                            opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentPlayer.DOFCCEDJODB, opponentPlayer.AIINAIDBHJI, false);
                        }
                        initOpponentPlayer = true;
                    }
                }

                if (newSkinToApply )
                {
                    if (!cancelOpponentSkin)
                    {
                        if (opponentCustomSkinCharacter == packetSkinCharacter || opponentCustomSkinCharacterVariant == packetSkinCharacterVariant)
                        {
                            opponentCustomTexture = TextureHelper.LoadPNG(Path.Combine(TextureMod.resourceFolder, "opponent.png"));
                            AssignAshesOutlineColor(opponentCustomTexture, opponentCustomSkinCharacterVariant);
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
                    opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentPlayer.DOFCCEDJODB, opponentPlayer.AIINAIDBHJI, false);
                }
            }



            if (localLobbyPlayer != null && randomizedChar == false) // Determine and assign skin to local player
            {
                if (localLobbyPlayer.CHNGAKOIJFE)
                {
                    if ((localPlayerChar != localLobbyPlayer.DOFCCEDJODB || localPlayerCharVar != localLobbyPlayer.AIINAIDBHJI))
                    {
                        localPlayerChar = localLobbyPlayer.DOFCCEDJODB;
                        localPlayerCharVar = localLobbyPlayer.AIINAIDBHJI;
                        initLocalPlayer = true;
                    }

                    LLButton[] buttons = FindObjectsOfType<LLButton>();
                    HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();

                    var setSkin = false;

                    if (useOnlySetKey == false)
                    {
                        if ((Input.GetKey(holdKey1) || Input.GetKey(holdKey2)) && buttons.Length > 0)
                        {
                            foreach (LLButton b in buttons) b.SetActive(false); //Deactivate buttons
                            if (Input.GetKeyDown(setSkinKey)) setSkin = true;
                        }
                        else if ((Input.GetKeyUp(holdKey1) || Input.GetKeyUp(holdKey2)) && buttons.Length > 0)
                        {
                            foreach (LLButton b in buttons) b.SetActive(true); //Reactivate buttons
                        }
                    }
                    else if (Input.GetKeyDown(setSkinKey)) setSkin = true;

                    if (setSkin && InLobby(GameType.Any)) // Assign skin to local player
                    {
                        if (InLobby(GameType.Online))
                        {
                            gameStatesOnlineLobby.JPNNBHNHHJC();
                            gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, false); //Set Ready
                            gameStatesOnlineLobby.BFIGLDLHKPO();
                            gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer);
                        }

                        if (localLobbyPlayer.DOFCCEDJODB == Character.RANDOM) // Randomize skin and char
                        {
                            var n = rng.Next(0, 11);
                            localLobbyPlayer.DOFCCEDJODB = (Character)n;
                            localLobbyPlayer.LALEEFJMMLH = (Character)n;

                            localTex = GetLoadedTexture((Character)n, localTex, true);
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
                        else localTex = GetLoadedTexture(localLobbyPlayer.DOFCCEDJODB, localTex, false);

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
                                if (localGamePlayerEntity.character == Character.GRAF)
                                {
                                    AssignToxicEffectColors(localGamePlayerEntity.player.CJFLMDNNMIE, localTex, localGamePlayerEntity.variant);
                                } else if (localGamePlayerEntity.character == Character.BOSS && (localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT3 || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT4))
                                {
                                    AssignDoomBoxVisualizerColorIngame(localGamePlayerEntity.player.CJFLMDNNMIE, localTex);
                                }
                            }
                        }
                    }
                    if (gameState == (JOFJHDJHJGI)21) { initLocalPlayer = true; }
                    break;
                #endregion
                case GameMode._1v1:
                case GameMode.FREE_FOR_ALL:
                //case GameMode.COMPETITIVE:
                    #region In ranked and online lobby
                    if (Input.GetKeyDown(cancelKey))
                    {
                        cancelOpponentSkin = !cancelOpponentSkin;
                        if (opponentLobbyCharacterModel != null)
                        {
                            opponentCustomTexture = null;
                            opponentCustomSkinCharacter = opponentPlayer.DOFCCEDJODB;
                            opponentCustomSkinCharacterVariant = opponentPlayer.AIINAIDBHJI;
                            opponentLobbyCharacterModel.SetCharacterLobby(opponentPlayer.CJFLMDNNMIE, opponentCustomSkinCharacter, CharacterVariant.CORPSE, false);
                            initOpponentPlayer = true;
                        }
                    }
                    if (InLobby(GameType.Any))
                    {
                        if (initLocalPlayer) { InitLocalPlayer(); }
                        if (initOpponentPlayer) { InitOpponentPlayer(); }
                        if (localLobbyPlayer == null)
                        {
                            if (InLobby(GameType.Online))
                            {
                                localLobbyPlayer = GetLocalPlayerInLobby(GameType.Online);
                            } else
                            {
                                localLobbyPlayer = GetLocalPlayerInLobby(GameType.Offline);
                            }
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
                            } catch { }
                            if (localLobbyPlayer.CJFLMDNNMIE == 3)
                            {
                                initLocalPlayer = true;
                            }

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
                            } catch { }
                            if (localTex != null)
                            {
                                AssignTextureToIngameCharacter(localGamePlayerEntity, localTex);
                                localPlayerName = AssignTextureToHud(localGamePlayerEntity, localTex);
                                if (localGamePlayerEntity.character == Character.GRAF)
                                {
                                    AssignToxicEffectColors(localGamePlayerEntity.player.CJFLMDNNMIE, localTex, localGamePlayerEntity.variant);
                                }
                                else if (localGamePlayerEntity.character == Character.BOSS && (localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT3 || localGamePlayerEntity.variant == CharacterVariant.MODEL_ALT4))
                                {
                                    AssignDoomBoxVisualizerColorIngame(localGamePlayerEntity.player.CJFLMDNNMIE, localTex);
                                }
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
                                    if (opponentPlayerEntity.character == Character.GRAF)
                                    {
                                        AssignToxicEffectColors(opponentPlayerEntity.player.CJFLMDNNMIE, opponentCustomTexture, opponentPlayerEntity.variant);
                                    }
                                    else if (opponentPlayerEntity.character == Character.BOSS && (opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT3 || opponentPlayerEntity.variant == CharacterVariant.MODEL_ALT4))
                                    {
                                        AssignDoomBoxVisualizerColorIngame(opponentPlayerEntity.player.CJFLMDNNMIE, opponentCustomTexture);
                                    }
                                }
                            }
                        }
                    }
                    else if (InPostGame())
                    {
                        AssignSkinToWinnerModel();

                        if (localTex != null) { AssignTextureToPostGameHud(localPlayerName, localTex); }
                        initLocalPlayer = true;

                        if (opponentCustomTexture != null)
                        {
                            AssignTextureToPostGameHud(opponentPlayerName, opponentCustomTexture);
                            initOpponentPlayer = true;
                        }
                    }
                    else initLocalPlayer = true;
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
                        ResetAllAshesOutlineColors();
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

                    if ((Input.GetKey(holdKey1) || Input.GetKey(holdKey2)) && Input.GetKeyDown(setSkinKey))
                    {
                        try
                        {
                            localTex = GetLoadedTextureForUnlocksModel(localTex);
                            SetUnlocksCharacterModel(localTex);
                        } catch { }
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
                            } else intervalMode = false;

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

            if (Input.GetKeyDown(reloadEntireSkinLibrary))
            {
                TextureMod.Instance.tl.LoadLibrary(); //Reloads the entire texture folder
            }
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
            switch(gt)
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
                    HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();
                    if (gameStatesOnlineLobby != false)
                    {
                        return ALDOKEMAOMB.BJDPHEHJJJK(gameStatesOnlineLobby.HCCGAFLIABM);
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
            HDLIJDBFGKN gameStatesOnlineLobby = FindObjectOfType<HDLIJDBFGKN>();
            if (gameStatesOnlineLobby != false)
            {
                if (gameStatesOnlineLobby.HCCGAFLIABM == 0) { player = ALDOKEMAOMB.BJDPHEHJJJK(1); }
                else { player = ALDOKEMAOMB.BJDPHEHJJJK(0); }
            }
            return player;
        }

        public CharacterModel GetLobbyCharacterModel(int playerNr)
        {
            CharacterModel retmodel = null;
            ScreenPlayers screenPlayers = FindObjectOfType<ScreenPlayers>();
            foreach(PlayersSelection playersSelection in screenPlayers.playerSelections)
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
           
            Renderer[] rs = model.curModel.transform.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                try
                {
                    foreach (Renderer r in rs)
                    {
                        if (model.character == Character.BOSS && (model.characterVariant == CharacterVariant.MODEL_ALT3 || model.characterVariant == CharacterVariant.MODEL_ALT4))
                        {
                            AssignDoomBoxVisualizerColorToRenderer(r, tex);
                        }

                        r.material.SetTexture("_MainTex", tex);
                    }
                } catch { Debug.Log("Failed assigning custom skin to CharacterModels"); }
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


        private void AssignTextureToIngameCharacter(PlayerEntity playerEntity, Texture tex)
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
                    if (ps.winnerCharacter == localPlayerChar && ps.winnerCharacterVariant == localPlayerCharVar) AssignTextureToCharacterModelRenderers(ps.winnerModel, localTex);
                }

                if (opponentCustomTexture != null)
                {
                    if (ps.winnerCharacter == opponentCustomSkinCharacter && ps.winnerCharacterVariant == opponentCustomSkinCharacterVariant) AssignTextureToCharacterModelRenderers(ps.winnerModel, opponentCustomTexture);
                }
            }
        }

        private void SetUnlocksCharacterModel(Texture tex)
        {
            CharacterModel[] cms = FindObjectsOfType<CharacterModel>();
            if (cms.Length > 0)
            {
                foreach(CharacterModel cm in cms)
                {
                    cm.SetSilhouette(false);
                    Renderer[] rs = cm.curModel.transform.GetComponentsInChildren<Renderer>();
                    if (rs.Length > 0)
                    {
                        foreach (Renderer r in rs) { r.material.SetTexture("_MainTex", tex); }
                    }
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
            initLocalPlayer = false;
            randomizedChar = false;
            ResetAllAshesOutlineColors();
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

        private Texture2D GetLoadedTexture(Character c, Texture2D currentTexture, bool random)
        {
            Texture2D ret = null;
            var texname = "";
            if (random)
            {
                var n = rng.Next(0, TextureMod.Instance.tl.characterTextures[c].Count());
                localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key);
                if (c == Character.BAG)
                {
                    localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, Character.NONE, CharacterVariant.DEFAULT, false);
                    AssignAshesOutlineColor(TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Value, localLobbyPlayer.AIINAIDBHJI);
                }
                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key), false);
                localPlayerCharVar = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key);
                texname = TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Key;
                ret = TextureMod.Instance.tl.characterTextures[c].ElementAt(n).Value;
            }
            else
            {
                if (currentTexture == null && TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value != null)
                {
                    localLobbyPlayer.AIINAIDBHJI = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                    if (c == Character.BAG)
                    {
                        localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, Character.NONE, CharacterVariant.DEFAULT, false);
                        AssignAshesOutlineColor(TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value, localLobbyPlayer.AIINAIDBHJI);
                    }
                    localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key), false);
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
                            if (c == Character.BAG)
                            {
                                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, Character.NONE, CharacterVariant.DEFAULT, false);
                                AssignAshesOutlineColor(pair.Value, localLobbyPlayer.AIINAIDBHJI);
                            }
                            localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, pair.Key), false);
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
                                if (c == Character.BAG)
                                {
                                    localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, Character.NONE, CharacterVariant.DEFAULT, false);
                                    AssignAshesOutlineColor(TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value, localLobbyPlayer.AIINAIDBHJI);
                                }
                                localLobbyPlayerModel.SetCharacterLobby(localLobbyPlayer.CJFLMDNNMIE, c, GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key), false);
                                localPlayerCharVar = GetVariantFromFileName(c, TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key);
                                texname = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Key;
                                ret = TextureMod.Instance.tl.characterTextures[c].ElementAt(0).Value;
                            }
                        }
                    }
                }
            }
            localPlayerChar = c;
            if (texname.Contains(".png")) texname = texname.Replace(".png", "");
            if (texname.Contains(".PNG")) texname = texname.Replace(".PNG", "");
            if (texname.Contains("_ALT2")) texname = texname.Replace("_ALT2", "");
            else if (texname.Contains("_ALT")) texname = texname.Replace("_ALT", "");

            localTexName = texname;
            return ret;
        }

        private Texture2D GetLoadedTextureForUnlocksModel(Texture2D currentTexture)
        {
            Texture2D ret = null;
            var texname = "";
            ScreenUnlocksSkins sus = FindObjectOfType<ScreenUnlocksSkins>();
            if (sus != null)
            {
                if (currentTexture == null && TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value != null)
                {
                    if (sus.character == Character.BAG) AssignAshesOutlineColor(TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key));
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
                            if (sus.character == Character.BAG) AssignAshesOutlineColor(pair.Value, GetVariantFromFileName(sus.character, pair.Key));
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
                                if (sus.character == Character.BAG) AssignAshesOutlineColor(TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key));
                                sus.ShowCharacter(sus.character, GetVariantFromFileName(sus.character, TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key), false);
                                texname = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Key;
                                ret = TextureMod.Instance.tl.characterTextures[sus.character].ElementAt(0).Value;
                            }
                        }
                    }
                }
            }
            if (texname.Contains(".png")) texname = texname.Replace(".png", "");
            if (texname.Contains(".PNG")) texname = texname.Replace(".PNG", "");
            if (texname.Contains("_ALT2")) texname = texname.Replace("_ALT2", "");
            else if (texname.Contains("_ALT")) texname = texname.Replace("_ALT", "");
            siluetteTimer = 5;

            localTexName = texname;
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

                    switch(localPlayerC)
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
                    } else
                    {
                        if (localPlayerC != c) return CharacterVariant.DEFAULT;
                        else
                        {
                            if (cv == CharacterVariant.DEFAULT) return CharacterVariant.ALT0;
                            else return CharacterVariant.DEFAULT;
                        }
                    }
                } else
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
                    switch (cv)
                    {
                        case CharacterVariant.DEFAULT:      toxic.outfitEffectColors[0] = c;    break;
                        case CharacterVariant.ALT0:         toxic.outfitEffectColors[1] = c;    break;
                        case CharacterVariant.MODEL_ALT:    toxic.outfitEffectColors[9] = c;    break;
                        case CharacterVariant.MODEL_ALT2:   toxic.outfitEffectColors[10] = c;   break;
                    }
                }
            }
        }

        public void AssignDoomBoxVisualizerColorIngame(int playerId, Texture2D tex)
        {
            BossPlayerModel[] dbs = FindObjectsOfType<BossPlayerModel>();
            foreach (BossPlayerModel db in dbs)
            {
                Color c1 = tex.GetPixel(493, 510);
                Color c2 = tex.GetPixel(508, 510);
                if (db.player.CJFLMDNNMIE == playerId)
                {
                    FNDGCLEDHAD visualizer = db.GetVisual("main").mainRenderer.gameObject.GetComponent<FNDGCLEDHAD>();
                    Material vismat = visualizer.FHAMOPAJHNJ;
                    vismat.SetColor("_AmpColor0", c1);
                    vismat.SetColor("_AmpColor1", c2);
                }
            }
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

        #region Ashes Outline stuff
        public void AssignAshesOutlineColor(Texture2D tex, CharacterVariant cv)
        {
            Color c;
            c.a = byte.MaxValue;
            switch (cv)
            {
                case CharacterVariant.DEFAULT:
                    c = tex.GetPixel(58, 438);
                    AOIOFOIHOCJ.outfitOutlineColors[0] = c;
                    break;
                case CharacterVariant.ALT0:
                    c = tex.GetPixel(58, 438);
                    AOIOFOIHOCJ.outfitOutlineColors[1] = c;
                    break;
                case CharacterVariant.MODEL_ALT3:
                    c = tex.GetPixel(113, 334);
                    AOIOFOIHOCJ.outfitOutlineColors[11] = c;
                    break;
                case CharacterVariant.MODEL_ALT4:
                    c = tex.GetPixel(113, 334);
                    AOIOFOIHOCJ.outfitOutlineColors[12] = c;
                    break;
            }
        }

        public void SaveOriginalAshesColors()
        {
            for (var i = 0; i < AOIOFOIHOCJ.outfitOutlineColors.Length; i++) originalDNAColors[i] = AOIOFOIHOCJ.outfitOutlineColors[i];
        }

        public void ResetAllAshesOutlineColors()
        {
            for (var i = 0; i < AOIOFOIHOCJ.outfitOutlineColors.Length; i++) AOIOFOIHOCJ.outfitOutlineColors[i] = originalDNAColors[i];
        }
        #endregion
        #endregion
    }
}

