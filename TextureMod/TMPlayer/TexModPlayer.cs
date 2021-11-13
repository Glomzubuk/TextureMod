using System;
using UnityEngine;
using GameplayEntities;
using LLScreen;
using HarmonyLib;
using LLBML;
using LLBML.Players;

namespace TextureMod.TMPlayer
{
    public class TexModPlayer
    {

        public TexModPlayer()
        {
        }
        public TexModPlayer(Player player, CustomSkin skin = null, CharacterModel model = null)
        {
            this.Player = player;
            this.customSkin = skin;
            this.characterModel = model ?? GetCurrentCharacterModelFor(this.Player.nr);

        }
        public Player Player { get; protected set; }
        public PlayerEntity PlayerEntity => Player?.playerEntity;

        public CharacterModel characterModel = null;

        public CustomSkin customSkin = null;
        public Texture2D Texture
        {
            get
            {
                if (SkinColorOverride == SkinColorFilter.NONE)
                    return customSkin?.Texture;
                else
                    return Texture;
            }
            private set
            {
                Texture = value;
            }
        }
        public Character CustomSkinCharacter => customSkin?.Character ?? Character.NONE;
        public CharacterVariant CustomSkinCharacterVariant => customSkin?.CharacterVariant ?? CharacterVariant.CORPSE;


        public bool ShouldRefreshSkin { get; set; }
        public SkinColorFilter SkinColorOverride { get; private set; } = SkinColorFilter.NONE;

        public void SetPlayer(Player player)
        {
            this.Player = player;
        }

        public virtual void Update()
        {
            if (ShouldRefreshSkin)
            {

            }

            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            if (screenZero?.screenType == ScreenType.GAME_RESULTS)
            {
                PostScreen postScreen = screenZero as PostScreen;
                if (customSkin != null) // postScreen.winner.nr
                {

                    if (postScreen.winner.CJFLMDNNMIE == this.Player.nr) // postScreen.winner.nr
                    {
                        AssignSkinToWinnerModel(postScreen);
                    }
                    AssignTextureToPostGameHud(postScreen);
                }
            }

            if (InLobby(GameType.Any))
            {
                this.characterModel?.SetSilhouette(false);
                if (customSkin != null) RendererHelper.AssignTextureToCharacterModelRenderers(this.characterModel, this.PlayerEntity, Texture);
            }
            else if (StateApi.InGame)
            {
                AssignTextureToIngameCharacter();
                AssignTextureToHud();
                switch (this.Player.CharacterSelected)
                {
                    case Character.CANDY: EffectsHandler.CandymanIngameEffects(this); break;
                    case Character.BAG: EffectsHandler.AshesIngameEffects(this); break;
                    case Character.GRAF: EffectsHandler.ToxicIngameEffects(this); break;
                    case Character.ELECTRO: EffectsHandler.GridIngameEffects(this); break;
                    case Character.SKATE: EffectsHandler.JetIngameEffects(this); break;
                }
            }

            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (screenOne != null)
            {
                if (screenOne?.screenType == ScreenType.UNLOCKS_SKINS)
                {
                    var screenUnlocksSkins = screenOne as ScreenUnlocksSkins;

                    CharacterModel previewModel = screenUnlocksSkins.previewModel;
                    if (silouetteTimer > 0)
                    {
                        if (customSkin != null)
                        {
                            characterModel.SetSilhouette(false);
                            AssignTextureToCharacterModelRenderers(characterModel, localPlayer.customSkin.Texture);
                        }
                    }

                    if (Input.GetKey(holdKey1.Value))
                    {
                        if (OnSkinChangeButtonDown())
                        {
                            SetSkinForUnlocksModel(screenUnlocksSkins);
                        }
                    }

                    if (localCustomSkin != null) // Reload a skin from its file
                    {
                        if (Input.GetKeyDown(reloadCustomSkin.Value))
                        {
                            if (!intervalMode)
                            {
                                if (reloadCustomSkinOnInterval.Value)
                                {
                                    intervalMode = true;
                                    reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                                }
                            }
                            else intervalMode = false;

                            try
                            {
                                localCustomSkin.ReloadSkin();
                                //localTex = TextureHelper.ReloadSkin(screenUnlocksSkins.character, localTex);
                                SetUnlocksCharacterModel(localCustomSkin.Texture);
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
                                    localCustomSkin.ReloadSkin();
                                    //localTex = TextureHelper.ReloadSkin(screenUnlocksSkins.character, localTex);
                                    SetUnlocksCharacterModel(localCustomSkin.Texture);
                                }
                                catch { LLHandlers.AudioHandler.PlaySfx(LLHandlers.Sfx.MENU_BACK); }
                                reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                            }
                        }
                    }
                }
                else if (screenOne?.screenType == ScreenType.UNLOCKS_CHARACTERS)
                {
                    localCustomSkin = null;
                    intervalMode = false;
                    reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                }
            }

        }
        public void FixedUpdate()
        {
        }

        public void CheckMirrors()
        {
            throw new NotImplementedException();
        }

        public void UpdateModel()
        {
            this.characterModel = GetCurrentCharacterModelFor(this.Player.nr);


            this.characterModel?.SetSilhouette(false);
            if (this.customSkin != null)
            {
                AssignTextureToCharacterModelRenderers();
            }
        }
        private static CharacterModel GetCurrentCharacterModelFor(int playerNr)
        {
            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];

            if (screenZero?.screenType == ScreenType.PLAYERS)
            {
                var screenPlayers = screenZero as ScreenPlayers;
                return screenPlayers.playerSelections[playerNr].characterModel;
            }
            else if (screenOne?.screenType == ScreenType.UNLOCKS_SKINS)
            {
                var screenUnlocksSkins = screenOne as ScreenUnlocksSkins;
                return screenUnlocksSkins.previewModel;
            }
            else if (screenOne?.screenType == ScreenType.UNLOCKS_CHARACTERS)
            {
                var screenUnlocksCharacters = screenOne as ScreenUnlocksCharacters;
                return screenUnlocksCharacters.previewModel;
            }
            else
            {
                TextureMod.Log.LogWarning("Got request for character model in an unsupported screen.\n - Screen[0].type: " + screenZero?.screenType.ToString() +
                    "\n - CurrentScreens[1].type: " + screenOne?.screenType.ToString());
                return null;
            }
        }

        public void SetColorFilter(SkinColorFilter filter)
        {
            if (SkinColorOverride != filter)
            {
                switch (filter)
                {
                    case SkinColorFilter.NONE:
                        Texture = null; break;
                    case SkinColorFilter.GRAY:
                        Texture = EffectsHandler.GetGrayscaledCopy(customSkin.Texture); break;
                    case SkinColorFilter.BLUE:
                        Texture = EffectsHandler.GetColoredCopy(customSkin.Texture, new Color(0,0,255)); break;
                    case SkinColorFilter.GREEN:
                        Texture = EffectsHandler.GetColoredCopy(customSkin.Texture, new Color(0, 255, 0)); break;
                    case SkinColorFilter.RED:
                        Texture = EffectsHandler.GetColoredCopy(customSkin.Texture, new Color(255, 0, 0)); break;
                    case SkinColorFilter.YELLOW:
                        Texture = EffectsHandler.GetColoredCopy(customSkin.Texture, new Color(0, 255, 255)); break;
                }
                SkinColorOverride = filter;
                this.ShouldRefreshSkin = true;
            }
        }



        private void SetUnlocksCharacterModel(Texture2D tex)
        {
            CharacterModel[] cms = FindObjectsOfType<CharacterModel>();
            if (cms.Length > 0)
            {
                foreach (CharacterModel cm in cms)
                {
                    cm.SetSilhouette(false);
                    AssignTextureToCharacterModelRenderers(cm, tex);
                }
            }
        }

        private void SetCharacterModelTex()
        {
            this.characterModel.SetSilhouette(false);
            this.AssignTextureToCharacterModelRenderers();
        }


        private void AssignTextureToIngameCharacter()
        {
            RendererHelper.AssignTextureToIngameCharacter(this.PlayerEntity, this.Texture);
        }
        private void AssignSkinToWinnerModel(PostScreen postScreen)
        {
            RendererHelper.AssignSkinToWinnerModel(postScreen, this.PlayerEntity, this.Texture);
        }
        private void AssignTextureToPostGameHud(PostScreen postScreen)
        {
            RendererHelper.AssignTextureToPostGameHud(postScreen, this.Texture, this.Player.nr);
        }
        private void AssignTextureToHud()
        {
            RendererHelper.AssignTextureToHud(this.Texture, this.PlayerEntity);
        }
        private void AssignTextureToCharacterModelRenderers()
        {
            RendererHelper.AssignTextureToCharacterModelRenderers(this.characterModel, this.PlayerEntity, this.Texture);
        }

        public void SetCustomSkin(CustomSkin customSkin)
        {
            this.customSkin = customSkin;
            if (this.customSkin == null) { return; }
            if (this.Player.Character != customSkin.Character || this.Player.CharacterVariant != customSkin.CharacterVariant)
            {
                SetCharacter(customSkin.Character, customSkin.CharacterVariant);
            }
        }

        public void SetCharacter(Character character, CharacterVariant characterVariant = CharacterVariant.DEFAULT)
        {
            if (StateApi.InLobby)
            {
                this.Player.Character = customSkin.Character;
                this.Player.CharacterVariant = customSkin.CharacterVariant;
                bool flipped = StateApi.CurrentGameMode == GameMode._1v1 && this.Player.nr == 1;
                this.UpdateModel();
                this.characterModel.SetCharacterLobby(this.Player.nr, this.Player.Character, this.Player.CharacterVariant, flipped);
                this.characterModel.PlayCamAnim();
            }
        }
    }
    public enum SkinColorFilter
    {
        RED = Team.Team_Enum.RED,
        BLUE = Team.Team_Enum.BLUE,
        YELLOW = Team.Team_Enum.YELLOW,
        GREEN = Team.Team_Enum.GREEN,
        NONE = Team.Team_Enum.NONE,
        GRAY,
    }
}
