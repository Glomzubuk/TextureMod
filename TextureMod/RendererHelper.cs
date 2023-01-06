using System;
using UnityEngine;
using GameplayEntities;
using LLScreen;
using LLBML;
using LLBML.Players;
using LLBML.Utils;
using HarmonyLib;

namespace TextureMod
{
    public static class RendererHelper
    {
        public static void AssignTextureToHud(Texture2D texture, PlayerEntity playerEntity)
        {
            if (ScreenApi.CurrentScreens[0] is ScreenGameHud sgh)
            {
                Player p = playerEntity.player;
                GameHudPlayerInfo ghpi = sgh.playerInfos[p.nr];

                Renderer[] rs = ghpi.gameObject.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rs)
                {
                    RendererHelper.AssignTextureToRenderer(r, texture, playerEntity.character, playerEntity.variant);
                }
            }
        }

        public static void AssignTextureToIngameCharacter(PlayerEntity playerEntity, Texture2D texture)
        {
            foreach (Renderer r in playerEntity.skinRenderers)
            {
                AssignTextureToRenderer(r, texture, playerEntity.character, playerEntity.variant);
            }

            if (playerEntity.character == Character.GRAF)
            {
                EffectsHandler.AssignToxicEffectColors(playerEntity.player.CJFLMDNNMIE, texture, playerEntity.variant);
            }
        }

        public static void AssignSkinToWinnerModel(PostScreen postScreen, PlayerEntity playerEntity, Texture2D texture)
        {
            AssignTextureToCharacterModelRenderers(postScreen.winnerModel, playerEntity.character, playerEntity.variant, texture);
        }


        public static void AssignTextureToPostGameHud(PostScreen postScreen, Texture2D texture, int playerNr)
        {
            Player player = Player.GetPlayer(playerNr);
            PostSceenPlayerBar playerBar = postScreen.playerBarsByPlayer[playerNr];

            Renderer[] rs = playerBar.gameObject.transform.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in rs)
            {
                RendererHelper.AssignTextureToRenderer(r, texture, player.Character, player.CharacterVariant);
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
