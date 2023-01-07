using System;
using UnityEngine;
using LLScreen;
using LLHandlers;
using HarmonyLib;
using LLBML.Players;
using LLBML.States;
using TextureMod.TMPlayer;
using TextureMod.Showcase;

namespace TextureMod
{
    public static class SkinSelect_Patches
    {
        /*
        [HarmonyPatch(typeof(HPNLMFHPHFD), nameof(HPNLMFHPHFD.ProcessMsg))]
        [HarmonyPrefix]
        public static bool ProcessMsg_Prefix(JOFJHDJHJGI __0, Message __1, HPNLMFHPHFD __instance)
        {
            if (!Input.GetKey(TextureMod.holdKey1.Value) || __1.msg != Msg.SEL_SKIN ||
                __instance.CFKCIJCEILI != null || __instance.EFOEMGLHOPL != null)
            {
                return true;
            }
            int playerNr = __1.playerNr;
            int selector = (playerNr != -1) ? playerNr : Player.GetLocalPlayer().nr;
            int index = __1.index;
            int prevNext = 1;
            if (index >= 100)
            {
                index -= 100;
                prevNext = -1;
            }
            if (selector == index)
            {
                TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(selector);
                TextureMod.Log.LogDebug($"Selecting new skin for player {selector}, {(prevNext > 0 ? "next" : "previous")}");
                if (tmPlayer.Player.selected && tmPlayer is LocalTexModPlayer ltmp)
                {
                    if (prevNext > 0)
                    {
                        ltmp.NextSkin();
                    }
                    else
                    {
                        ltmp.PreviousSkin();
                    }
                }
            }
            return false;
        }
        */

        [HarmonyPatch(typeof(OGKPCMDOMPF), nameof(OGKPCMDOMPF.ProcessMsg))]
        [HarmonyPrefix]
        public static bool GameStatesUnlocks_ProcessMsh_Prefix(JOFJHDJHJGI OHBPPCEFBHI, Message EIMJOIEPMNA)
        {
            if (!TextureMod.IsSkinKeyDown())
            {
                ShowcaseStudio.Instance.SetCustomSkin(null);
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayersSelection), "<Init>m__4")] // btSkin.onClick delegate in PlayersSelection.Init
        [HarmonyPrefix]
        public static bool BtSkin_OnClick_Prefix(int pNr)
        {
            if (!TextureMod.IsSkinKeyDown())
            {
                ClearSkinFor(pNr);
                return true;
            }
            NextSkinFor(pNr);
            return false;
        }

        [HarmonyPatch(typeof(PlayersCharacterButton), "<Init>m__0")] // btCharacter.onClick delegate in PlayersCharacterButton.Init
        [HarmonyPrefix]
        public static bool BtCharacter_OnClick_Prefix(int pNr, PlayersCharacterButton __instance)
        {
            if (!TextureMod.IsSkinKeyDown())
            {
                ClearSkinFor(pNr);
                return true;
            }
            NextSkinFor(pNr, __instance.character);
            return false;
        }

        [HarmonyPatch(typeof(ScreenPlayers), "<DoUpdate>m__5")] // Player.ForEach delegate in ScreenPlayers.DoUpdate
        [HarmonyPrefix]
        public static bool Player_ForEach_Prefix(ALDOKEMAOMB p, ScreenPlayers __instance)
        {
            Player player = p;
            int nr = player.nr;

            if (!TextureMod.IsSkinKeyDown())
            {
                /*if (player.controller.GetButtonDown(InputAction.SHLEFT) || player.controller.GetButtonDown(InputAction.SHLEFT))
                {
                    ClearSkinFor(nr);
                }

                if (CGLLJHHAJAK.GIGAKBJGFDI.hasMouseKeyboard && player.controller.IncludesMouse())
                {
                    if (Input.mouseScrollDelta.y > 0.8f)
                    {
                        ClearSkinFor(nr);
                    }
                    if (Input.mouseScrollDelta.y < -0.8f)
                    {
                        ClearSkinFor(nr);
                    }
                }*/
                return true;
            }

            if (player.controller.GetButtonDown(InputAction.SHLEFT))
            {
                PreviousSkinFor(nr);
            }
            if (player.controller.GetButtonDown(InputAction.SHRIGHT))
            {
                NextSkinFor(nr);
            }
            if (CGLLJHHAJAK.GIGAKBJGFDI.hasMouseKeyboard && player.controller.IncludesMouse())
            {
                if (Input.mouseScrollDelta.y > 0.8f)
                {
                    PreviousSkinFor(nr);
                }
                if (Input.mouseScrollDelta.y < -0.8f)
                {
                    NextSkinFor(nr);
                }
            }
            return false;
        }

        public static void ClearSkinFor(int nr)
        {
            int selector = (nr != -1) ? nr : Player.GetLocalPlayer().nr;
            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(selector);
            if (tmPlayer?.Player != null && tmPlayer.Player.selected && tmPlayer is LocalTexModPlayer ltmp)
            {
                TextureMod.Log.LogDebug($"Removing skin from player {selector}");
                ltmp.RemoveCustomSkin();
            }
        }
        public static void NextSkinFor(int nr, Character character = Character.NONE)
        {
            int selector = (nr != -1) ? nr : Player.GetLocalPlayer().nr;
            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(selector);
            if (tmPlayer?.Player != null && tmPlayer.Player.selected && tmPlayer is LocalTexModPlayer ltmp)
            {
                TextureMod.Log.LogDebug($"Selecting new skin for player {selector}, next");
                ltmp.NextSkin(character);
            }
        }

        public static void PreviousSkinFor(int nr, Character character = Character.NONE)
        {
            int selector = (nr != -1) ? nr : Player.GetLocalPlayer().nr;
            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(selector);
            if (tmPlayer?.Player != null && tmPlayer.Player.selected && tmPlayer is LocalTexModPlayer ltmp)
            {
                TextureMod.Log.LogDebug($"Selecting new skin for player {selector}, previous");
                ltmp.PreviousSkin(character);
            }
        }
    }

    public static class RandomSkinSelect_Patches
    {

    }
}

