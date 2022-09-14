using System;
using UnityEngine;
using GameplayEntities;
using LLScreen;
using HarmonyLib;
using LLBML;
using LLBML.Players;
using LLBML.States;
using TextureMod.CustomSkins;

namespace TextureMod.TMPlayer
{
    public class TexModPlayer
    {
        protected static BepInEx.Logging.ManualLogSource Logger => TextureMod.Log;

        public TexModPlayer(Player player, CustomSkin skin = null, CharacterModel model = null)
        {
            this.Player = player;
            this.skinHandler = new CustomSkinHandler(skin);
            this.characterModel = model ?? GetCurrentCharacterModelFor(this.Player.nr);

        }
        public Player Player { get; protected set; }
        public PlayerEntity PlayerEntity => Player?.playerEntity;

        public CharacterModel characterModel = null;

        public CustomSkinHandler skinHandler = null;
        public bool HasCustomSkin() => skinHandler?.CustomSkin != null;
        public CustomSkin CustomSkin => skinHandler?.CustomSkin;
        public Texture2D Texture
        {
            get
            {
                if (SkinColorOverride == SkinColorFilter.NONE)
                    return CustomSkin.Texture;
                else
                    return Texture;
            }
            private set
            {
                Texture = value;
            }
        }
        public Character CustomSkinCharacter => CustomSkin?.Character ?? Character.NONE;
        public CharacterVariant CustomSkinCharacterVariant => CustomSkin?.CharacterVariant ?? CharacterVariant.CORPSE;


        public bool ShouldRefreshSkin { get; set; }
        public SkinColorFilter SkinColorOverride { get; private set; } = SkinColorFilter.NONE;

        public void SetPlayer(Player player)
        {
            this.Player = player;
        }

        public virtual void Update()
        {
            if (this.CustomSkin != null && (this.Player.CharacterSelected != this.CustomSkinCharacter || this.Player.CharacterVariant != this.CustomSkinCharacterVariant))
            {
                if (ShouldRefreshSkin)
                {
                    this.Player.CharacterSelected = this.CustomSkinCharacter;
                    this.Player.CharacterVariant = this.CustomSkinCharacterVariant;
                    GameStatesLobbyUtils.RefreshLocalPlayerState();
               
                }
                else
                {
                    RemoveCustomSkin();
                }
            }

            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            if (screenZero?.screenType == ScreenType.GAME_RESULTS)
            {
                PostScreen postScreen = screenZero as PostScreen;
                if (CustomSkin != null) // postScreen.winner.nr
                {

                    if (postScreen.winner.CJFLMDNNMIE == this.Player.nr) // postScreen.winner.nr
                    {
                        AssignSkinToWinnerModel(postScreen);
                    }
                    AssignTextureToPostGameHud(postScreen);
                }
            }

            if (GameStates.IsInLobby())
            {
                this.characterModel?.SetSilhouette(false);
                if (HasCustomSkin())
                {
                    AssignTextureToCharacterModelRenderers();
                }
            }
            else if (GameStates.IsInMatch() && HasCustomSkin() && PlayerEntity != null && Player.IsInMatch)
            {
                AssignTextureToIngameCharacter();
                AssignTextureToHud();
                switch (this.Player.Character)
                {
                    case Character.CANDY: EffectsHandler.CandymanIngameEffects(this); break;
                    case Character.BAG: EffectsHandler.AshesIngameEffects(this); break;
                    case Character.GRAF: EffectsHandler.ToxicIngameEffects(this); break;
                    case Character.ELECTRO: EffectsHandler.GridIngameEffects(this); break;
                    case Character.SKATE: EffectsHandler.JetIngameEffects(this); break;
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
            if (this.CustomSkin != null)
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
                        Texture = EffectsHandler.GetGrayscaledCopy(CustomSkin.Texture); break;
                    case SkinColorFilter.BLUE:
                        Texture = EffectsHandler.GetColoredCopy(CustomSkin.Texture, new Color(0,0,255)); break;
                    case SkinColorFilter.GREEN:
                        Texture = EffectsHandler.GetColoredCopy(CustomSkin.Texture, new Color(0, 255, 0)); break;
                    case SkinColorFilter.RED:
                        Texture = EffectsHandler.GetColoredCopy(CustomSkin.Texture, new Color(255, 0, 0)); break;
                    case SkinColorFilter.YELLOW:
                        Texture = EffectsHandler.GetColoredCopy(CustomSkin.Texture, new Color(0, 255, 255)); break;
                }
                SkinColorOverride = filter;
                this.ShouldRefreshSkin = true;
            }
        }



        private void SetUnlocksCharacterModel(Texture2D tex)
        {
            CharacterModel[] cms = UnityEngine.Object.FindObjectsOfType<CharacterModel>();
            if (cms.Length > 0)
            {
                foreach (CharacterModel cm in cms)
                {
                    cm.SetSilhouette(false);
                    RendererHelper.AssignTextureToCharacterModelRenderers(cm, this.PlayerEntity, this.Texture);
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
            if (this.characterModel == null) return;
            if (this.PlayerEntity != null)
            {
                RendererHelper.AssignTextureToCharacterModelRenderers(this.characterModel, this.PlayerEntity, this.Texture);
            }
            else
            {
                RendererHelper.AssignTextureToCharacterModelRenderers(this.characterModel, this.Player.Character, this.Player.CharacterVariant, this.Texture);
            }

        }

        public void SetCustomSkin(CustomSkinHandler skinHandler)
        {
            this.skinHandler = skinHandler;
            if (this.CustomSkin == null) { return; }
            if (this.Player.Character != CustomSkin.Character || this.Player.CharacterVariant != CustomSkin.CharacterVariant)
            {
                SetCharacter(CustomSkin.Character, CustomSkin.CharacterVariant);

            }
            GameStatesLobbyUtils.RefreshPlayerState(this.Player);
        }

        public void RemoveCustomSkin()
        {
            if (this.CustomSkin == null) { return; }
            this.skinHandler = null;
        }

        public void SetCharacter(Character character, CharacterVariant characterVariant = CharacterVariant.DEFAULT)
        {
            if (LLBML.States.GameStates.IsInLobby())
            {
                this.Player.Character = skinHandler.CustomSkin.Character;
                this.Player.CharacterVariant = skinHandler.CustomSkin.CharacterVariant;
                bool flipped = StateApi.CurrentGameMode == GameMode._1v1 && this.Player.nr == 1;
                this.UpdateModel();
                this.characterModel.SetCharacterLobby(this.Player.nr, this.Player.Character, this.Player.CharacterVariant, flipped);
                this.characterModel.PlayCamAnim();
            }
        }


        virtual protected void ShowSkinNametags()
        {
            if (CustomSkin != null) //Show skin nametags
            {
                string labelTxt = CustomSkin.GetSkinLabel();
                GUI.skin.box.wordWrap = false;
                GUIContent content = new GUIContent(labelTxt);
                //TODO move that in localTexModPlayer
                //if (!intervalMode) content = new GUIContent(labelTxt);
                //else content = new GUIContent(labelTxt + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                GUI.skin.box.fontSize = 22;

                ScreenBase screenOne = ScreenApi.CurrentScreens[1];

                if (GameStates.IsInLobby())
                {
                    if (screenOne == null)
                    {
                        switch (StateApi.CurrentGameMode)
                        {
                            //TODO do that for more than 2 players
                            case GameMode.TUTORIAL:
                            case GameMode.TRAINING:
                                GUI.Box(new Rect((Screen.width / 8), (Screen.height / 12.5f), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                            case GameMode._1v1:
                                if (Player.nr == 0) GUI.Box(new Rect(Screen.width / 10, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt); //Check if local player is the player with ID 0
                                else GUI.Box(new Rect((Screen.width / 20) * 12.95f, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                            case GameMode.FREE_FOR_ALL:
                            case GameMode.COMPETITIVE:
                                if (Player.nr == 0) GUI.Box(new Rect(0 + Screen.width / 250, Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                else GUI.Box(new Rect((Screen.width / 4) + (Screen.width / 250), Screen.height / 12.5f, GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                                break;
                        }
                    }
                }

                //TODO move that stuff elsewhere, it's for the showcase, which shouldn' t use texmodplayer
                if (screenOne != null)
                {
                    if (screenOne.screenType == ScreenType.UNLOCKS_SKINS)
                    {
                        if (TextureMod.Instance.showcaseStudio.showUI == false)
                        {
                            TextureMod.Instance.showcaseStudio.skinName = labelTxt;
                            //TODO move that in localTexModPlayer
                            //TextureMod.Instance.showcaseStudio.refreshTimer = reloadCustomSkinTimer;
                            //TextureMod.Instance.showcaseStudio.refreshMode = intervalMode;
                        }
                        else
                        {
                            //TODO move that in localTexModPlayer
                            //if (intervalMode) GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt + " (Refresh " + "[" + reloadCustomSkinTimer + "]" + ")");
                            //else GUI.Box(new Rect((Screen.width - (Screen.width / 3.55f)) - (GUI.skin.box.CalcSize(content).x / 2), Screen.height - (Screen.height / 23), GUI.skin.box.CalcSize(content).x, GUI.skin.box.CalcSize(content).y), labelTxt);
                        }
                    }
                }
            }  //Show skin nametags
        }
    }
    public enum SkinColorFilter
    {
        RED = Team.Enum.RED,
        BLUE = Team.Enum.BLUE,
        YELLOW = Team.Enum.YELLOW,
        GREEN = Team.Enum.GREEN,
        NONE = Team.Enum.NONE,
        GRAY,
    }
}
