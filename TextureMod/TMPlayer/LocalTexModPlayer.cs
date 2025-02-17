using System;
using System.Collections.Generic;
using GameplayEntities;
using Multiplayer;
using LLBML.Players;
using LLBML.States;
using TextureMod.CustomSkins;

namespace TextureMod.TMPlayer
{
    public class LocalTexModPlayer : TexModPlayer
    {
        public static int localPlayerNr => P2P.localPeer?.playerNr ?? 0;
        public static Player LocalLobbyPlayer => Player.GetPlayer(localPlayerNr);
        public static PlayerEntity LocalGamePlayerEntity => LocalLobbyPlayer?.playerEntity;

        public LocalTexModPlayer(Player player, CustomSkin skin = null)
            : base(player, skin)
        {
        }

        private int skinCounter = 0;
        public void NextSkin(Character character = Character.NONE, bool random = false)
        {
            if (character == Character.NONE) character = Player.CharacterSelected;
            if (CustomSkin?.Character != Player.CharacterSelected || character != Player.CharacterSelected) skinCounter = 0;
            else skinCounter++;
            this.ChangeSkin(character, skinCounter, random);
        }

        public void PreviousSkin(Character character = Character.NONE, bool random = false)
        {
            if (character == Character.NONE) character = Player.CharacterSelected;
            if (CustomSkin?.Character != Player.CharacterSelected || character != Player.CharacterSelected) skinCounter = -1;
            else skinCounter--;
            this.ChangeSkin(character, skinCounter, random);
        }

        public void ChangeSkin(Character character, int index, bool random = false)
        {
            // TODO Improve that
            List<CustomSkinHandler> skins = SkinsManager.skinCache.GetUsableHandlers(character);
            if (skins == null) return;

            Logger.LogDebug($"Counter: {skinCounter}, skin length: {skins.Count}");
            if (skins.Count > 0)
            {
                this.SetCustomSkin(skins?[mod(index, skins.Count)]);
                if (GameStates.IsInOnlineLobby())
                {
                    GameStatesLobbyUtils.SendPlayerState(this.Player);
                }
            }
        }
        private int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
