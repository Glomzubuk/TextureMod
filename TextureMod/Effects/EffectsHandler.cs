using System;
using System.IO;
using System.Linq;
using UnityEngine;
using GameplayEntities;
using LLHandlers;
using BepInEx;
using HarmonyLib;
using LLBML;
using LLBML.Math;
using LLBML.Texture;
using LLBML.States;
using TextureMod.TMPlayer;

namespace TextureMod
{
    public static class EffectsHandler
    {
        public static string EffectsFolder => Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "Effects");

        #region Effects
        private static Texture2D candySplashWhite;

        private static Texture2D gridStartBG;
        private static Texture2D gridStartFG;
        private static Texture2D gridTrail;
        private static Texture2D gridArrive;

        private static Texture2D bubbleBG;
        private static Texture2D bubbleFG;
        private static Texture2D bubblePopBG;
        private static Texture2D bubblePopFG;
        #endregion

        public static void Init()
        {
            LoadEffects();
        }

        private static void LoadEffects()
        {
            candySplashWhite = TextureUtils.LoadPNG(Path.Combine(EffectsFolder, "candySplashWhite.png"));

            gridStartBG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "GridSpecial", "gridStartBG.png"));
            gridStartFG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "GridSpecial", "gridStartFG.png"));
            gridTrail = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "GridSpecial", "gridTrail.png"));
            gridArrive = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "GridSpecial", "gridArrive.png"));

            bubbleBG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "JetSpecial", "bubbleBG.png"));
            bubbleFG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "JetSpecial", "bubbleFG.png"));
            bubblePopBG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "JetSpecial", "bubblePopBG.png"));
            bubblePopFG = TextureUtils.LoadPNG(Utility.CombinePaths(EffectsFolder, "JetSpecial", "bubblePopFG.png"));
        }

        #region Texture Manipulation [Color Replacers and effect colors, etc]

        public static Texture2D GetGrayscaledCopy(Texture2D texture)
        {
            Texture2D gray = TextureUtils.DefaultTexture(texture.width, texture.height);
            Color[] texColors = gray.GetPixels();
            for (var i = 0; i < texColors.Length; i++)
            {
                var grayValue = texColors[i].grayscale;
                texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
            }
            gray.SetPixels(texColors);
            gray.Apply();
            return gray;
        }

        public static Texture2D GetColoredCopy(Texture2D texture, Color color)
        {
            Texture2D gray = TextureUtils.DefaultTexture(texture.width, texture.height);
            Color[] texColors = gray.GetPixels();
            for (var i = 0; i < texColors.Length; i++)
            {
                texColors[i] *= color;
            }
            gray.SetPixels(texColors);
            gray.Apply();
            return gray;
        }
        #endregion

        #region Ball Effects
        public static void SetBallColors()
        {
            if (GameStates.IsInMatch())
            {
                SetBallColors(BallHandler.instance.GetBall(0));
            }
        }
        public static void SetBallColors(BallEntity ballEntity)
        {
            Color color = Color.grey;
            ballEntity.GetVisual("main").renderers[2].materials[0].color = color;
            ballEntity.SetColorOutlinesColor(color);
            for (int i = 0; i < ballEntity.GetVisual("main2D").meshRendererTrail.Length; i++)
            {
                color.a = ballEntity.GetVisual("main2D").meshRendererTrail[i].material.color.a;
                ballEntity.GetVisual("main2D").meshRendererTrail[i].material.color = color;
            }
        }
        #endregion

        #region Dust&Ashes Effects
        public static void AshesIngameEffects(TexModPlayer tmPlayer)
        {
            if (tmPlayer.PlayerEntity != null && tmPlayer.CustomSkin?.Texture != null)
            {
                if (tmPlayer.PlayerEntity.character == Character.BAG)
                {
                    AssignAshesOutlineColor(tmPlayer.PlayerEntity, tmPlayer.Texture);
                }
            }
        }

        public static void AssignAshesOutlineColor(PlayerEntity pe, Texture2D tex)
        {
            foreach (Renderer r in pe.skinRenderers)
            {
                AssignAshesOutlineColor(r, pe.variant, tex);
            }
        }

        public static void AssignAshesOutlineColor(Renderer r, CharacterVariant variant, Texture2D tex)
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
        public static void CandymanIngameEffects(TexModPlayer tmPlayer)
        {
            var mainBall = BallApi.GetBall(0);
            if (mainBall.ballData.hitstunState == HitstunState.CANDY_STUN || mainBall.ballData.ballState == BallState.CANDYBALL)
            {
                if (mainBall.GetLastPlayerHitter().character == Character.CANDY)
                {
                    if (mainBall.GetLastPlayerHitter() == tmPlayer.PlayerEntity && tmPlayer.CustomSkin != null)
                    {
                        AssignSkinToCandyball(mainBall, tmPlayer.PlayerEntity, tmPlayer.Texture);
                    }
                    else AssignSkinToCandyball(mainBall, mainBall.GetLastPlayerHitter(), null);
                }
            }
            AssignSkinColorToCandySplash(tmPlayer);
        }


        public static void AssignSkinToCandyball(BallEntity ball, PlayerEntity pe, Texture2D tex)
        {
            if (tex == null)
            {
                tex = (Texture2D)pe.skinRenderers.First().material.mainTexture;
            }

            ball.GetVisual($"candyBall{(int)pe.variant}Visual").mainRenderer.material.mainTexture = tex;
        }

        private static int lastCandyBallPlayerIndex = -1;
        public static void AssignSkinColorToCandySplash(TexModPlayer tmPlayer)
        {
            var mainBall = BallApi.GetBall(0);
            EffectEntity[] effects = UnityEngine.Object.FindObjectsOfType<EffectEntity>();
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
                        Texture2D tex = CheckPlayerOrMainBallSplashEffect(tmPlayer.PlayerEntity, effect, tmPlayer.CustomSkin.Texture) ?? null;

                        if (tex != null)
                        {
                            SetCandySplashEffect(tmPlayer.PlayerEntity.variant, effects[i], tex);
                            for (int j = 0; j < effects.Length; j++)
                            {
                                //Searches for the intiial candySplash and sets it to custom texture
                                if (effects[j].effectData.graphicName == "candySplash" && effects[j].effectData.active)
                                {
                                    SetCandySplashEffect(tmPlayer.PlayerEntity.variant, effects[j], tex);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Texture2D CheckPlayerOrMainBallSplashEffect(PlayerEntity playerEntity, EffectEntity effect, Texture2D characterTex)
        {
            if (playerEntity?.character == Character.CANDY && characterTex != null)
            {

                var mainBall = BallApi.GetBall(0);
                //bool playerNearEffect = HHBCPNCDNDH.HPLPMEAOJPM(IBGCBLLKIHA.FCKBPDNEAOG(playerEntity.GetPosition(), effect.GetPosition()).KEMFCABCHLO, HHBCPNCDNDH.NKKIFJJEPOL(0.2m));
                bool playerNearEffect = ((Vector2f)IBGCBLLKIHA.FCKBPDNEAOG(playerEntity.GetPosition(), effect.GetPosition())).magnitude < 0.2m;
                bool ballNearEffect = ((Vector2f)IBGCBLLKIHA.FCKBPDNEAOG(mainBall.GetPosition(), effect.GetPosition())).magnitude < 0.6m;
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

        public static void SetCandySplashEffect(CharacterVariant variant, EffectEntity effect, Texture2D tex)
        {
            Renderer r = effect.GetVisual("main").mainRenderer;
            r.material.mainTexture = candySplashWhite;
            r.material.color = (variant == CharacterVariant.MODEL_ALT3 || variant == CharacterVariant.MODEL_ALT4) ? tex.GetPixel(130, 92) : tex.GetPixel(103, 473);
            effect.name = "candySplashModified";
        }
        #endregion


        #region Grid effects
        public static void GridIngameEffects(TexModPlayer tmPlayer)
        {
            var mainBall = BallApi.GetBall(0);

            if (tmPlayer.PlayerEntity != null && tmPlayer.CustomSkin != null)
            {
                if (mainBall?.ballData.hitstunState == HitstunState.TELEPORT_STUN || mainBall?.ballData.hitstunState == HitstunState.BUNT_STUN)
                {
                    if (mainBall.GetLastPlayerHitter() == tmPlayer.PlayerEntity)
                    {
                        AssignGridSpecialColor(tmPlayer.PlayerEntity, tmPlayer.Texture);
                    }
                }
                else if (tmPlayer.PlayerEntity.GetCurrentAbility()?.name.Contains("ELECTROCHARGE") != null)
                {
                    AssignGridSpecialColor(tmPlayer.PlayerEntity, tmPlayer.Texture);
                }
            }
        }

        public static void AssignGridSpecialColor(PlayerEntity pe, Texture2D tex)
        {
            Color pixelColor = new Color(1, 1, 1, 1);

            //Gets the pixel color from the skin texture at a certain location based on if it's a DLC skin or Normal/Model_Alt
            pixelColor = (pe.variant <= CharacterVariant.MODEL_ALT2) ? tex.GetPixel(451, 454) : tex.GetPixel(442, 484);


            VisualEntity[] ves = UnityEngine.Object.FindObjectsOfType<VisualEntity>();
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
        public static void DBEffects(TexModPlayer tmPlayer)
        {
            if (tmPlayer.PlayerEntity != null && tmPlayer.CustomSkin != null)
            {
                AssignVisualizer(tmPlayer.PlayerEntity, tmPlayer.Texture);
            }
        }

        public static void AssignVisualizer(PlayerEntity pe, Texture2D tex)
        {
            AssignVisualizer(pe.character, pe.GetVisual("main").mainRenderer, tex);
        }

        public static void AssignVisualizer(Character character, Renderer r, Texture2D tex)
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

        public static void AssignOmegaDoomboxSmearsAndArms(PlayerEntity pe, Texture2D tex)
        {
            AssignOmegaDoomboxSmearsAndArms(pe.GetVisual("main").mainRenderer, tex);
        }

        public static void AssignOmegaDoomboxSmearsAndArms(Renderer r, Texture2D tex)
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

        public static void ToxicIngameEffects(TexModPlayer tmPlayer)
        {
            if (tmPlayer.PlayerEntity != null && tmPlayer.CustomSkin != null && tmPlayer.PlayerEntity.character == Character.GRAF)
            {
                if (tmPlayer.PlayerEntity.variant == CharacterVariant.MODEL_ALT3 || tmPlayer.PlayerEntity.variant == CharacterVariant.MODEL_ALT4)
                {
                    AssignNurseToxicCanisters(tmPlayer.PlayerEntity, tmPlayer.Texture);
                    AssignToxicEffectColors(tmPlayer.Player.nr, tmPlayer.Texture, tmPlayer.Player.CharacterVariant);
                }
            }
        }

        public static void AssignNurseToxicCanisters(PlayerEntity pe, Texture2D tex)
        {
            AssignNurseToxicCanisters(pe.GetVisual("main").mainRenderer, tex);
        }

        public static void AssignNurseToxicCanisters(Renderer r, Texture2D tex)
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

        public static void AssignToxicEffectColors(int playerId, Texture2D tex, CharacterVariant cv)
        {
            GrafPlayer[] toxics = UnityEngine.Object.FindObjectsOfType<GrafPlayer>();
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
        public static void JetIngameEffects(TexModPlayer tmPlayer)
        {
            var mainBall = BallApi.GetBall(0);
            if (mainBall.GetLastPlayerHitter() == tmPlayer.PlayerEntity && tmPlayer.CustomSkin != null)
            {
                AssignBubbleVisual(tmPlayer.PlayerEntity.variant, tmPlayer.Texture);
            }

            if (tmPlayer.PlayerEntity && tmPlayer.CustomSkin != null)
            {
                if (tmPlayer.PlayerEntity.variant == CharacterVariant.MODEL_ALT || tmPlayer.PlayerEntity.variant == CharacterVariant.MODEL_ALT2)
                {
                    AssignJetScubaVisor(tmPlayer.PlayerEntity, tmPlayer.Texture);
                }
            }
        }
        public static void AssignJetScubaVisor(PlayerEntity pe, Texture2D tex)
        {
            AssignJetScubaVisor(pe.GetVisual("main").mainRenderer, tex);
        }

        public static void AssignJetScubaVisor(Renderer r, Texture2D tex)
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

        public static void AssignBubbleVisual(CharacterVariant variant, Texture2D tex)
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

            MeshRenderer[] mrs = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
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

            VisualEntity[] ves = UnityEngine.Object.FindObjectsOfType<VisualEntity>();
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
    }
}
