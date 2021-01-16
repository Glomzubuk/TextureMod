using System.IO;
using UnityEngine;
using LLScreen;
using LLHandlers;
using LLModMenu;
using Multiplayer;
using System.Collections.Generic;
using Steamworks;

namespace TextureMod
{
    public class TextureMod : MonoBehaviour
    {
        private static TextureMod instance = null;
        public static TextureMod Instance { get { return instance; } }
        public static readonly string resourceFolder = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Managed"), "TextureModResources"), "Images");

        public static void Initialize() { GameObject gameObject = new GameObject("TextureMod"); TextureMod modscript = gameObject.AddComponent<TextureMod>(); DontDestroyOnLoad(gameObject); instance = modscript; }
        private const string modVersion = "1.4.5";
        private const string repositoryOwner = "Daioutzu";
        private const string repositoryName = "LLBMM-TextureMod";

        public string debug = "";

        public TextureChanger tc = null;
        public ModMenuIntegration MMI = null;
        public TextureLoader tl = null;
        public ExchangeClient ec = null;
        public ModDebugging md = null;
        public string retSkin = "";
        public EffectChanger effectChanger = null;
        public ShowcaseStudio showcaseStudio = null;

        public static List<string> ownedDLCs = new List<string>();
        public static bool hasDLC = false;

        private void Start()
        {
            UIScreen.SetLoadingScreen(true, false, false, Stage.NONE);
            CheckIfPLayerHasDLC();
            if (ownedDLCs.Count > 0) hasDLC = true;
        }

        private void Update()
        {
            if (MMI == null) { MMI = gameObject.AddComponent<ModMenuIntegration>(); }
            if (tl == null) { tl = gameObject.AddComponent<TextureLoader>(); }
            if (tc == null) { tc = gameObject.AddComponent<TextureChanger>(); }
            if (ec == null) { ec = gameObject.AddComponent<ExchangeClient>(); }
            if (md == null) { md = gameObject.AddComponent<ModDebugging>(); }
            if (effectChanger == null) {effectChanger = gameObject.AddComponent<EffectChanger>(); }
            if (showcaseStudio == null) showcaseStudio = gameObject.AddComponent<ShowcaseStudio>();
        }


        private void OnGUI()
        {
            var OriginalColor = GUI.contentColor;
            var OriginalLabelFontSize = GUI.skin.label.fontSize;
            var OriginalLabelAlignment = GUI.skin.label.alignment;

            GUI.contentColor = Color.white;
            GUI.skin.label.fontSize = 50;
            if ((tl == null) || UIScreen.loadingScreenActive && tl.loadingExternalFiles == true)
            {
                var sX = Screen.width / 2;
                var sY = UIScreen.GetResolutionFromConfig().height / 3;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, sY+50, Screen.width, sY), "TextureMod is loading External Textures");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            GUI.contentColor = OriginalColor;
            GUI.skin.label.fontSize = OriginalLabelFontSize;
            GUI.skin.label.alignment = OriginalLabelAlignment;

            GUI.Label(new Rect(5f, 5f, 1920f, 25f), debug);
        }

        private void CheckIfPLayerHasDLC()
        {
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1244880))) ownedDLCs.Add("Dice");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1204070))) ownedDLCs.Add("Raptor");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1174410))) ownedDLCs.Add("Dust&Ashes");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(991870)) || AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1399791))) ownedDLCs.Add("Doombox");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1269880))) ownedDLCs.Add("Toxic");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431710))) ownedDLCs.Add("Latch");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431701))) ownedDLCs.Add("Grid");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431702))) ownedDLCs.Add("Switch");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1399790))) ownedDLCs.Add("Sonata");
            if (AALLGKBNLBO.OEBMADMCBAE(new AppId_t(1431711))) ownedDLCs.Add("Jet");
        }
    }
}
