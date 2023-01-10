using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using TextureMod.TMPlayer;
using TextureMod.CustomSkins;
using TextureMod.Showcase;


namespace TextureMod
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInProcess("LLBlaze.exe")]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    public class TextureMod : BaseUnityPlugin
    {
        #region legacystrings
        private const string modVersion = PluginInfos.PLUGIN_VERSION;
        private const string repositoryOwner = "Daioutzu";
        private const string repositoryName = "LLBMM-TextureMod";
        #endregion

        //public string debug = "";

        #region instances
        public static TextureMod Instance { get; private set; } = null;
        public static ManualLogSource Log { get; private set; } = null;
        public TexModPlayerManager tmpl = null;
        public SkinsManager sm = null;
        public ModDebugging md = null;
        public EffectChanger effectChanger = null;
        public ShowcaseStudio showcaseStudio = null;
        #endregion

        #region pathes
        public static string ResourceFolder { get; private set; }
        public static DirectoryInfo ModdingFolder { get; private set; }
        #endregion

        internal static bool IsSkinKeyDown() => Input.GetKey(holdKey1.Value);
        public static string loadingText = $"TextureMod is loading External Textures...";

        public void Awake()
        {
            Logger.LogInfo("Hello, World!");
            ResourceFolder = Utility.CombinePaths(Path.GetDirectoryName(this.Info.Location), "TextureModResources");
            ModdingFolder = LLBML.Utils.ModdingFolder.GetModSubFolder(this.Info);
            Instance = this;
            Log = this.Logger;
            InitConfig();


            var harmoInstance = new Harmony(PluginInfos.PLUGIN_ID);
            Logger.LogInfo("Patching SkinSelect_Patches");
            harmoInstance.PatchAll(typeof(SkinSelect_Patches));
        }

        private void Start()
        {
            SkinsManager.LoadLibrary();

            // TODO Loading screen
            // UIScreen.SetLoadingScreen(true, false, false, Stage.NONE);
            EffectsHandler.Init();
            ExchangeClient.Init();


            LLBML.Utils.ModDependenciesUtils.RegisterToModMenu(this.Info, new List<string> {
                "Wondering how to assign skins and in what part of the game you can do so?",
                "Simply hold the 'Enable Skin Changer' button and press either the `Next skin` or the `Previous Skin` buttons to cycle skins",
                "Skins can be assigned in Ranked Lobbies, 1v1 Lobbies, FFA Lobbies(Only for player 1 and 2) and in the skin unlock screen for a character or in ShowcaseStudio",
                "If you select random in the lobby and try to assign a custom skin you will be given a random character and random skin. In online lobbies you will be set to ready, and your buttons will become unavailable unless you've deactivated 'Lock Buttons On Random'",
                " ",
                "If you wish to real time edit your skins, use the F5 button to reload your skin whenever you're in training mode or in the character skin unlock screen",
                "You can also enable the interval mode and have it automatically reload the current custom skin every so often. Great for dual screen, or windowed mode setups (Does not work in training mode)",
                "This mod was written by MrGentle"
            });

            tmpl = gameObject.AddComponent<TexModPlayerManager>();
            sm = gameObject.AddComponent<SkinsManager>();
        }

        private void Update()
        {
            if (md == null) { md = gameObject.AddComponent<ModDebugging>(); }
            if (effectChanger == null) { effectChanger = gameObject.AddComponent<EffectChanger>(); }
            if (showcaseStudio == null) showcaseStudio = gameObject.AddComponent<ShowcaseStudio>();
        }

        private void OnGUI()
        {
            /*
            var OriginalColor = GUI.contentColor;
            var OriginalLabelFontSize = GUI.skin.label.fontSize;
            var OriginalLabelAlignment = GUI.skin.label.alignment;

            GUI.contentColor = Color.white;
            GUI.skin.label.fontSize = 50;
            if ((tl == null) || UIScreen.loadingScreenActive && tl.loadingExternalFiles == true)
            {
                GUIStyle label = new GUIStyle(GUI.skin.label);
                var sX = Screen.width / 2;
                var sY = UIScreen.GetResolutionFromConfig().height / 3;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, sY + 50, Screen.width, sY), loadingText);
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            GUI.contentColor = OriginalColor;
            GUI.skin.label.fontSize = OriginalLabelFontSize;
            GUI.skin.label.alignment = OriginalLabelAlignment;
            GUI.Label(new Rect(5f, 5f, 1920f, 25f), debug);
            */
        }

        #region Configuration
        public static ConfigEntry<KeyCode> holdKey1;
        public static ConfigEntry<KeyCode> nextSkin;
        public static ConfigEntry<KeyCode> previousSkin;
        public static ConfigEntry<KeyCode> cancelKey;
        public static ConfigEntry<KeyCode> reloadCustomSkin;
        public static ConfigEntry<KeyCode> reloadEntireSkinLibrary;
        public static ConfigEntry<bool> useOnlySetKey;
        public static ConfigEntry<bool> sendSkinsToOpponents;
        public static ConfigEntry<bool> receiveSkinsFromOpponents;
        public static ConfigEntry<bool> showDebugInfo;
        public static ConfigEntry<bool> randomSkinOnRandomSelect;
        public static ConfigEntry<bool> reloadCustomSkinOnInterval;
        public static ConfigEntry<int> skinReloadIntervalInFrames;
        public static ConfigEntry<bool> assignFirstSkinOnCharacterSelection;

        private void InitConfig()
        {

            ConfigFile config = TextureMod.Instance.Config;

            config.Bind("TextureChanger", "lobby_settings_header", "Lobby Settings:", new ConfigDescription("", null, "modmenu_header"));
            holdKey1 = config.Bind<KeyCode>("TextureChanger", "holdKey1", KeyCode.LeftShift);
            nextSkin = config.Bind<KeyCode>("TextureChanger", "nextSkin", KeyCode.Mouse0);
            previousSkin = config.Bind<KeyCode>("TextureChanger", "previousSkin", KeyCode.Mouse1);
            cancelKey = config.Bind<KeyCode>("TextureChanger", "cancelKey", KeyCode.A);
            useOnlySetKey = config.Bind<bool>("TextureChanger", "useOnlySetKey", false);
            sendSkinsToOpponents = config.Bind<bool>("TextureChanger", "sendSkinsToOpponents", true);
            receiveSkinsFromOpponents = config.Bind<bool>("TextureChanger", "receiveSkinsFromOpponents", true);
            randomSkinOnRandomSelect = config.Bind<bool>("TextureChanger", "randomSkinOnRandomSelect", true);
            assignFirstSkinOnCharacterSelection = config.Bind<bool>("TextureChanger", "assignFirstSkinOnCharacterSelection", false);
            config.Bind("TextureChanger", "gap1", 20, new ConfigDescription("",null,"modmenu_gap"));

            config.Bind("TextureChanger", "rt_skin_edit_header", "Real-time Skin editing:", new ConfigDescription("", null, "modmenu_header"));
            reloadCustomSkin = config.Bind<KeyCode>("TextureChanger", "reloadCustomSkin", KeyCode.F5);
            reloadEntireSkinLibrary = config.Bind<KeyCode>("TextureChanger", "reloadEntireSkinLibrary", KeyCode.F9);
            reloadCustomSkinOnInterval = config.Bind<bool>("TextureChanger", "reloadCustomSkinOnInterval", false);
            skinReloadIntervalInFrames = config.Bind<int>("TextureChanger", "skinReloadIntervalInFrames", 60);
            config.Bind("TextureChanger", "gap2", 20, new ConfigDescription("", null, "modmenu_gap"));

            config.Bind("TextureChanger", "general_header", "General:", new ConfigDescription("", null, "modmenu_header"));
            showDebugInfo = config.Bind<bool>("TextureChanger", "showDebugInfo", false);
            config.Bind("TextureChanger", "gap3", 20, new ConfigDescription("", null, "modmenu_gap"));
        }
        #endregion
    }
}
