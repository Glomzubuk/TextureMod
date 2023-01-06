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
        private Vector3 labelScreenPos;

        public TexModPlayer(Player player, CustomSkin skin = null)
        {
            this.Player = player;
            this.skinHandler = new CustomSkinHandler(skin);
        }
        public Player Player { get; protected set; }
        public PlayerEntity PlayerEntity => Player?.playerEntity;

        public ModelHandler ModelHandler = null;

        public CustomSkinHandler skinHandler = null;
        public bool HasCustomSkin() => skinHandler?.CustomSkin != null;
        public CustomSkin CustomSkin => skinHandler?.CustomSkin;
        public Texture2D Texture
        {
            get
            {
                if (SkinColorOverride == SkinColorFilter.NONE)
                    return CustomSkin?.Texture;
                else
                    return Texture;
            }
            private set
            {
                Texture = value;
            }
        }
        public Character CustomSkinCharacter => CustomSkin?.Character ?? Character.NONE;
        public ModelVariant CustomSkinModelVariant => CustomSkin?.ModelVariant ?? ModelVariant.None;


        public bool ShouldRefreshSkin { get; set; }
        public SkinColorFilter SkinColorOverride { get; private set; } = SkinColorFilter.NONE;

        

        public void SetPlayer(Player player)
        {
            this.Player = player;
        }

        public virtual void Update()
        {
            if (this.Player.playerStatus != PlayerStatus.NONE)
            {
                UpdateModel();
            }
            if (GameStates.IsInLobby())
            {
                if (this.CustomSkin != null && (this.Player.CharacterSelected != this.CustomSkinCharacter || !VariantHelper.VariantMatch(this.Player.CharacterVariant, this.CustomSkinModelVariant)))
                {
                    if (ShouldRefreshSkin)
                    {
                        this.Player.CharacterSelected = this.CustomSkinCharacter;
                        this.Player.CharacterVariant = VariantHelper.GetDefaultVariantForModel(CustomSkin.ModelVariant);
                        GameStatesLobbyUtils.RefreshLocalPlayerState();
                    }
                    else
                    {
                        RemoveCustomSkin();
                    }
                }
                if (ScreenApi.CurrentScreens[0] is ScreenPlayers sp)
                {
                    Camera cam = Camera.main;
                    PlayersSelection ps = sp.playerSelections[Player.nr];
                    RectTransform btTeamTr = ps.btTeam.transform as RectTransform;
                    labelScreenPos = cam.WorldToScreenPoint(btTeamTr.position);
                    labelScreenPos.y -= btTeamTr.rect.height;
                }
            }
            else if (GameStates.IsInMatch() && HasCustomSkin() && PlayerEntity != null && Player.IsInMatch)
            {
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

        public void OnGUI()
        {
            if (GameStates.IsInLobby())
            {
                if (HasCustomSkin())
                {
                    ShowSkinNametags();
                }
            }
        }

        public void CheckMirrors()
        {
            throw new NotImplementedException();
        }

        public void UpdateModel()
        {
            if (ModelHandler == null || ModelHandler.IsObsolete() || !ModelHandler.ValidCharacter(this.Player.Character, this.Player.CharacterVariant))
            {
                this.ModelHandler = ModelHandler.GetCurrentModelHandler(this.Player.nr);
            }

            if (ModelHandler != null)
            {
                this.ModelHandler.Update();
                this.ModelHandler.texture = Texture;
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
                    RendererHelper.AssignTextureToCharacterModelRenderers(cm, this.PlayerEntity.character, this.PlayerEntity.variant, this.Texture);
                }
            }
        }


        public void SetCustomSkin(CustomSkinHandler skinHandler)
        {
            this.skinHandler = skinHandler;
            if (this.CustomSkin == null) { return; }
            //if (this.Player.Character != CustomSkin.Character || !VariantHelper.VariantMatch(this.Player.CharacterVariant, CustomSkin.ModelVariant))
            //{
                SetCharacter(CustomSkin.Character, VariantHelper.GetDefaultVariantForModel(CustomSkin.ModelVariant));
            //}
            bool flipped = StateApi.CurrentGameMode == GameMode._1v1 && this.Player.nr == 1;

            if (ScreenApi.CurrentScreens[0] is ScreenPlayers sp)
            {
                sp.playerSelections[this.Player.nr].characterModel.SetCharacterLobby(this.Player.nr, this.Player.Character, this.Player.CharacterVariant, flipped);
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
            if (GameStates.IsInLobby())
            {
                TexModPlayerManager.ForAllTexmodPlayers((tmPlayer) =>
                {
                    if (tmPlayer.Player.Character == character)
                    {
                        if (tmPlayer.Player.CharacterVariant == characterVariant)
                        {
                            characterVariant = VariantHelper.GetNextVariantForModel(CustomSkin.ModelVariant, characterVariant);
                        }
                    }
                });
                this.Player.Character = character;
                this.Player.CharacterVariant = characterVariant;
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

                ScreenBase screenZero = ScreenApi.CurrentScreens[0];
                ScreenBase screenOne = ScreenApi.CurrentScreens[1];

                if (GameStates.IsInLobby() && screenZero is ScreenPlayers sp)
                {
                    Vector2 labelSizes = GUI.skin.box.CalcSize(content);

                    Rect labelBox = new Rect(
                        labelScreenPos.x - (labelSizes.x / 2), 
                        Screen.height - (labelScreenPos.y),
                        labelSizes.x,
                        labelSizes.y
                    );

                    GUI.Box(labelBox, labelTxt);
                }
            }
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
