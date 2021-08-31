using System;
using System.Collections.Generic;
using UnityEngine;
using LLHandlers;
using LLScreen;
using Multiplayer;
using Steamworks;
using BepInEx;
using LLBML;


namespace TextureMod
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInProcess("LLBlaze.exe")]
    [BepInDependency("fr.glomzubuk.plugins.llb.llbml", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    public class TextureMod : BaseUnityPlugin
    {
        #region legacystrings
        private const string modVersion = PluginInfos.PLUGIN_VERSION;
        private const string repositoryOwner = "Daioutzu";
        private const string repositoryName = "LLBMM-TextureMod";
        #endregion

        public string debug = "";

        #region instances
        public static TextureMod Instance { get; private set; }
        public TextureChanger tc = null;
        public TextureLoader tl = null;
        public ExchangeClient ec = null;
        public ModDebugging md = null;
        #endregion

        public static string ResourceFolder { get { return BepInEx.Utility.CombinePaths(Paths.ManagedPath, "TextureModResources"); } }

        public string retSkin = "";
        public EffectChanger effectChanger = null;
        public ShowcaseStudio showcaseStudio = null;

        public static List<Character> ownedDLCs = new List<Character>();
        public static bool hasDLC = false;
        public static string loadingText = $"TextureMod is loading External Textures...";

        public void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UIScreen.SetLoadingScreen(true, false, false, Stage.NONE);
            CheckIfPLayerHasDLC();
            if (ownedDLCs.Count > 0) hasDLC = true;


            Logger.LogInfo("Searching ModMenu");
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("no.mrgentle.plugins.llb.modmenu"))
            {
                Logger.LogInfo("Registering to ModMenu");
                LLModMenu.LLModMenu.RegisterMod(this.Info);
            }

            Config.Bind("General", "text3", "Wondering how to assign skins and in what part of the game you can do so?", "modmenu_text");
            Config.Bind("General", "text4", "Simply hold the 'Enable Skin Changer' button and press either the `Next skin` or the `Previous Skin` buttons to cycle skins", "modmenu_text");
            Config.Bind("General", "text5", "Skins can be assigned in Ranked Lobbies, 1v1 Lobbies, FFA Lobbies(Only for player 1 and 2) and in the skin unlock screen for a character or in ShowcaseStudio", "modmenu_text");
            Config.Bind("General", "text6", "If you select random in the lobby and try to assign a custom skin you will be given a random character and random skin. In online lobbies you will be set to ready, and your buttons will become unavailable unless you've deactivated 'Lock Buttons On Random'", "modmenu_text");
            Config.Bind("General", "text7", " ", "modmenu_text");
            Config.Bind("General", "text8", "If you wish to real time edit your skins, use the F5 button to reload your skin whenever you're in training mode or in the character skin unlock screen", "modmenu_text");
            Config.Bind("General", "text9", "You can also enable the interval mode and have it automatically reload the current custom skin every so often. Great for dual screen, or windowed mode setups (Does not work in training mode)", "modmenu_text");
            Config.Bind("General", "text1", "This mod was written by MrGentle", "modmenu_text");
        }

        private void Update()
        {
            if (tl == null) { 
                tl = gameObject.AddComponent<TextureLoader>(); 
            } else if(tl.loadingExternalFiles == false) {
                LoadingScreen.SetLoading(this.Info, false);
            }
            if (tc == null) { tc = gameObject.AddComponent<TextureChanger>(); }
            if (ec == null) { ec = gameObject.AddComponent<ExchangeClient>(); }
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
            */
            GUI.Label(new Rect(5f, 5f, 1920f, 25f), debug);
        }

        private void CheckIfPLayerHasDLC()
        {
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1244880))) ownedDLCs.Add(Character.PONG);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1204070))) ownedDLCs.Add(Character.KID);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1174410))) ownedDLCs.Add(Character.BAG);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(991870)) || AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1399791))) ownedDLCs.Add(Character.BOSS);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1269880))) ownedDLCs.Add(Character.GRAF);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431710))) ownedDLCs.Add(Character.CROC);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431701))) ownedDLCs.Add(Character.ELECTRO);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431702))) ownedDLCs.Add(Character.ROBOT);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1399790))) ownedDLCs.Add(Character.BOOM);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431711))) ownedDLCs.Add(Character.SKATE);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431712))) ownedDLCs.Add(Character.CANDY);
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431700))) ownedDLCs.Add(Character.COP);
        }
    }
}
