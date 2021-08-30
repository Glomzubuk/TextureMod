using System.IO;
using System.Linq;
using UnityEngine;
using GameplayEntities;
using BepInEx;
using BepInEx.Configuration;


namespace TextureMod
{
    public class EffectChanger : MonoBehaviour
    {
        public static string effectsFolder = Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "Effects");

        #region Parry and clash effects
        Texture2D parryActiveBG;
        Texture2D parryActiveMG;
        Texture2D parryActiveFG;

        Texture2D parryEndBG;
        Texture2D parryEndMG;
        Texture2D parryEndFG;

        Texture2D parrySuccessBG;
        Texture2D parrySuccessMG;
        Texture2D parrySuccessFG;

        Texture2D clashBG;
        Texture2D clashFG;

        ConfigEntry<int> parryFirstColorR;
        ConfigEntry<int> parryFirstColorG;
        ConfigEntry<int> parryFirstColorB;

        ConfigEntry<int> parrySecondColorR;
        ConfigEntry<int> parrySecondColorG;
        ConfigEntry<int> parrySecondColorB;

        ConfigEntry<int> parryThirdColorR;
        ConfigEntry<int> parryThirdColorG;
        ConfigEntry<int> parryThirdColorB;

        ConfigEntry<bool> enableCustomParry;
        #endregion



        private void Start()
        {
            string parryFolder = Path.Combine(effectsFolder, "parry");
            parryActiveBG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryActiveBG.png"));
            parryActiveMG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryActiveMG.png"));
            parryActiveFG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryActiveFG.png"));

            parryEndBG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryEndBG.png"));
            parryEndMG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryEndMG.png"));
            parryEndFG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parryEndFG.png"));

            parrySuccessBG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parrySuccessBG.png"));
            parrySuccessMG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parrySuccessMG.png"));
            parrySuccessFG = TextureHelper.LoadPNG(Path.Combine(parryFolder, "parrySuccessFG.png"));

            string clashFolder = Path.Combine(effectsFolder, "Clash");
            clashBG = TextureHelper.LoadPNG(Path.Combine(clashFolder, "clashEffectBG.png"));
            clashFG = TextureHelper.LoadPNG(Path.Combine(clashFolder, "clashEffectFG.png"));


            ConfigFile config = TextureMod.Instance.Config;

            config.Bind("EffectChanger", "parry_header", "Parry Settings:", "modmenu_header");
            enableCustomParry = config.Bind("EffectChanger", "enableCustomParryAndClash", false);

            parryFirstColorR = config.Bind("EffectChanger", "parryFirstColorR", 0,
                new ConfigDescription("Parry's first color red amount", new AcceptableValueRange<int>(0, 255))
            );
            parryFirstColorG = config.Bind("EffectChanger", "parryFirstColorG", 0,
                new ConfigDescription("Parry's first color green amount", new AcceptableValueRange<int>(0, 255))
            );
            parryFirstColorB = config.Bind("EffectChanger", "parryFirstColorR", 0,
                new ConfigDescription("Parry's first color blue amount", new AcceptableValueRange<int>(0, 255))
            );
            config.Bind("EffectChanger", "parry_gap1", "20", "modmenu_gap");

            parrySecondColorR = config.Bind("EffectChanger", "parrySecondColorR", 0,
                new ConfigDescription("Parry's second color red amount", new AcceptableValueRange<int>(0, 255))
            );
            parrySecondColorG = config.Bind("EffectChanger", "parrySecondColorG", 0,
                new ConfigDescription("Parry's second color green amount", new AcceptableValueRange<int>(0, 255))
            );
            parrySecondColorB = config.Bind("EffectChanger", "parrySecondColorB", 0,
                new ConfigDescription("Parry's second color blue amount", new AcceptableValueRange<int>(0, 255))
            );
            config.Bind("EffectChanger", "parry_gap2", "20", "modmenu_gap");

            parryThirdColorR = config.Bind("EffectChanger", "parryThirdColorR", 0,
                new ConfigDescription("Parry's third color red amount", new AcceptableValueRange<int>(0, 255))
            );
            parryThirdColorG = config.Bind("EffectChanger", "parryThirdColorG", 0,
                new ConfigDescription("Parry's third color green amount", new AcceptableValueRange<int>(0, 255))
            );
            parryThirdColorB = config.Bind("EffectChanger", "parryThirdColorB", 0,
                new ConfigDescription("Parry's third color blue amount", new AcceptableValueRange<int>(0, 255))
            );
            config.Bind("EffectChanger", "parry_gap3", "20", "modmenu_gap");
        }

        private void Update()
        {
            if (enableCustomParry.Value == true)
            {
                Color32 parryFirstColor = new Color32((byte)parryFirstColorR.Value, (byte)parryFirstColorG.Value, (byte)parryFirstColorB.Value, 255);
                Color32 parrySecondColor = new Color32((byte)parrySecondColorR.Value, (byte)parrySecondColorG.Value, (byte)parrySecondColorB.Value, 255);
                Color32 parryThirdColor = new Color32((byte)parryThirdColorR.Value, (byte)parryThirdColorG.Value, (byte)parryThirdColorB.Value, 255);

                MeshRenderer[] mrs = FindObjectsOfType<MeshRenderer>();
                foreach (MeshRenderer mr in mrs)
                {
                    if (mr.name == "parryVisual")
                    {
                        mr.material.mainTexture = parryActiveBG;
                        Material m1 = mr.material;
                        Material m2 = new Material(mr.material.shader);
                        Material m3 = new Material(mr.material.shader);
                        m2.mainTexture = parryActiveMG;
                        m3.mainTexture = parryActiveFG;
                        Material[] mArray = new Material[] { m1, m2, m3 };
                        mr.materials = mArray;
                        mr.materials[0].color = parryFirstColor;
                        mr.materials[1].color = parrySecondColor;
                        mr.materials[2].color = parryThirdColor;
                    }
                }

                VisualEntity[] ves = FindObjectsOfType<VisualEntity>();
                foreach (VisualEntity ve in ves)
                {
                    if (ve.name == "parryEnd")
                    {
                        foreach (Renderer mr in ve.GetComponentsInChildren<Renderer>())
                        {
                            mr.material.mainTexture = parryEndBG;
                            Material m1 = mr.material;
                            Material m2 = new Material(mr.material.shader);
                            Material m3 = new Material(mr.material.shader);
                            m2.mainTexture = parryEndMG;
                            m3.mainTexture = parryEndFG;
                            Material[] mArray = new Material[] { m1, m2, m3 };
                            mr.materials = mArray;
                            mr.materials[0].color = parryFirstColor;
                            mr.materials[1].color = parrySecondColor;
                            mr.materials[2].color = parryThirdColor;
                        }
                    }


                    if (ve.name == "parrySuccess")
                    {
                        Renderer mr = ve.GetComponentsInChildren<Renderer>().First();
                        mr.material.mainTexture = parrySuccessBG;
                        Material m1 = mr.material;
                        Material m2 = new Material(mr.material.shader);
                        Material m3 = new Material(mr.material.shader);
                        m2.CopyPropertiesFromMaterial(m1);
                        m3.CopyPropertiesFromMaterial(m1);
                        m2.mainTexture = parrySuccessMG;
                        m3.mainTexture = parrySuccessFG;
                        Material[] mArray = new Material[] { m1, m2, m3 };
                        mr.materials = mArray;
                        mr.materials[0].color = parryFirstColor;
                        mr.materials[1].color = parrySecondColor;
                        mr.materials[2].color = parryThirdColor;
                    }

                    if (ve.name == "clashEffect")
                    {
                        Renderer mr = ve.GetComponentsInChildren<Renderer>().First();
                        mr.material.mainTexture = clashBG;
                        Material m1 = mr.material;
                        Material m2 = new Material(mr.material.shader);
                        m2.CopyPropertiesFromMaterial(m1);
                        m2.mainTexture = clashFG;
                        Material[] mArray = new Material[] { m1, m2 };
                        mr.materials = mArray;
                        mr.materials[0].color = parryFirstColor;
                        mr.materials[1].color = parryThirdColor;
                    }
                }
            }
        }

        public Texture2D Combine(Texture2D background, Texture2D overlay)
        {

            int startX = 0;
            int startY = background.height - overlay.height;

            for (int x = startX; x < background.width; x++)
            {

                for (int y = startY; y < background.height; y++)
                {
                    Color bgColor = background.GetPixel(x, y);
                    Color wmColor = overlay.GetPixel(x - startX, y - startY);

                    Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
        }
    }
}
