using System;
using UnityEngine;
using GameplayEntities;
using LLBML.Players;
using LLBML.Utils;
using HarmonyLib;

namespace TextureMod
{
    public static class RendererHelper
    {
        private static AccessTools.FieldRef<GameHudPlayerInfo, ALDOKEMAOMB> shownPlayerField = AccessTools.FieldRefAccess<GameHudPlayerInfo, ALDOKEMAOMB>("shownPlayer");


        public static void AssignTextureToHud(Texture2D texture, PlayerEntity playerEntity)
        {
            GameHudPlayerInfo[] ghpis = UnityEngine.Object.FindObjectsOfType<GameHudPlayerInfo>();
            if (ghpis.Length > 0)
            {
                foreach (GameHudPlayerInfo ghpi in ghpis)
                {
                    //ALDOKEMAOMB shownPlayer = Traverse.Create(ghpi).Field<ALDOKEMAOMB>("shownPlayer").Value;
                    ALDOKEMAOMB shownPlayer = shownPlayerField.Invoke(ghpi);
                    if (shownPlayer == playerEntity.player)
                    {
                        Renderer[] rs = ghpi.gameObject.transform.GetComponentsInChildren<Renderer>();
                        if (rs.Length > 0)
                        {
                            foreach (Renderer r in rs)
                            {
                                RendererHelper.AssignTextureToRenderer(r, texture, playerEntity.character, playerEntity.variant);
                            }
                        }
                    }
                }
            }
        }

        public static void AssignTextureToIngameCharacter(PlayerEntity playerEntity, Texture2D texture)
        {
            VisualEntity ve = playerEntity?.gameObject?.GetComponent<VisualEntity>();
            if (ve != null)
            {
                if (ve.skinRenderers.Count > 0)
                {
                    foreach (Renderer r in ve.skinRenderers)
                    {
                        AssignTextureToRenderer(r, texture, playerEntity);
                    }

                    if (playerEntity.character == Character.GRAF)
                    {
                        EffectsHandler.AssignToxicEffectColors(playerEntity.player.CJFLMDNNMIE, texture, playerEntity.variant);
                    }
                }
            }
            else
            {
                TextureMod.Log.LogError($"Null ref test: pe {playerEntity == null}, tex {texture == null}, ve {ve == null}");
                DebugUtils.PrintStacktrace();
            }
        }

        public static void AssignSkinToWinnerModel(PostScreen postScreen, PlayerEntity playerEntity, Texture2D texture)
        {
            AssignTextureToCharacterModelRenderers(postScreen.winnerModel, playerEntity, texture);
        }


        public static void AssignTextureToPostGameHud(PostScreen postScreen, Texture2D texture, int playerNr)
        {
            Renderer[] rs = postScreen.playerBarsByPlayer[playerNr].gameObject.transform.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < rs.Length; i++)
            {
                RendererHelper.AssignTextureToRenderer(rs[i], texture, playerNr);
            }
        }

        public static void AssignTextureToCharacterModelRenderers(CharacterModel model, PlayerEntity playerEntity, Texture2D texture)
        {
            
            Renderer[] rs = model.curModel?.transform.GetComponentsInChildren<Renderer>() ?? new Renderer[0];
            for (int i = 0; i < rs.Length; i++)
            {
                RendererHelper.AssignTextureToRenderer(rs[i], texture, playerEntity);
            }
        }

        public static void AssignTextureToCharacterModelRenderers(CharacterModel model, Character character, CharacterVariant variant, Texture2D texture)
        {

            Renderer[] rs = model.curModel?.transform.GetComponentsInChildren<Renderer>() ?? new Renderer[0];
            for (int i = 0; i < rs.Length; i++)
            {
                RendererHelper.AssignTextureToRenderer(rs[i], texture, character, variant);
            }
        }

        public static void AssignTextureToRenderer(Renderer r, Texture2D texture, int playerNr = -1)
        {
            Character character = Character.NONE;
            CharacterVariant characterVariant = CharacterVariant.DEFAULT;
            PlayerEntity playerEntity = r.transform.GetComponentInParent<PlayerEntity>();

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
                    Player shownPlayer = Traverse.Create(playerHud).Field<ALDOKEMAOMB>("shownPlayer").Value;
                    character = shownPlayer.CharacterSelected;
                    characterVariant = shownPlayer.CharacterVariant;
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
                        if (playerNr > -1)
                        {
                            Player player = Player.GetPlayer(playerNr);
                            character = player.Character;
                            characterVariant = player.CharacterVariant;
                        }
                        else
                        {
                            throw new MemberNotFoundException("Unable to determine character and variant");
                        }
                    }
                }
            }
            AssignTextureToRenderer(r, texture, character, characterVariant);
        }

        public static void AssignTextureToRenderer(Renderer r, Texture2D texture, PlayerEntity playerEntity)
        {
            AssignTextureToRenderer(r, texture, playerEntity.character, playerEntity.variant);
        }

        public static void AssignTextureToRenderer(Renderer r, Texture2D texture, Character character, CharacterVariant characterVariant)
        {
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
                    r.material.SetTexture("_MainTex", texture);
                }
            }

            if (character == Character.GRAF)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    EffectsHandler.AssignNurseToxicCanisters(r, texture);
                }
            }
            else if (character == Character.SKATE)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                {
                    EffectsHandler.AssignJetScubaVisor(r, texture);
                }
            }
            else if (character == Character.BOSS)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                {
                    EffectsHandler.AssignOmegaDoomboxSmearsAndArms(r, texture);
                }
                else if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    EffectsHandler.AssignVisualizer(character, r, texture);
                }
            }
            else if (character == Character.BAG)
            {
                EffectsHandler.AssignAshesOutlineColor(r, characterVariant, texture);
            }
            else if (character == Character.BOOM)
            {
                if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    EffectsHandler.AssignVisualizer(character, r, texture);
                }
            }
        }
    }
}
