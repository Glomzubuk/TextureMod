using HarmonyLib;
using LLBML.Players;

namespace TextureMod.TMPlayer
{
    public class OpponentTexModPlayer : TexModPlayer
    {
        public OpponentTexModPlayer(Player player, CustomSkin skin = null, CharacterModel model = null) : base(player, skin, model)
        {
        }

        public override void Update()
        {
            base.Update();

            if (this.Player != null && this.characterModel != null)
            {
                Traverse tv_characterModel = Traverse.Create(characterModel);
                //TODO AccessRef theses, it's reflection  every frame otherwise
                Character modelCharacter = tv_characterModel.Field<Character>("character").Value;
                CharacterVariant modelCharacterVariant = tv_characterModel.Field<CharacterVariant>("characterVariant").Value;

                if (
                    (modelCharacter != customSkin.Character || modelCharacterVariant != customSkin.CharacterVariant) &&
                    (Player.CharacterSelected != Character.NONE || Player.CharacterVariant != CharacterVariant.CORPSE))
                {
                    if (packetSkinCharacter != Character.NONE && Player.CharacterSelected != (Character)32 && packetSkinCharacterVariant != CharacterVariant.CORPSE)
                    {
                        CustomSkinCharacter = packetSkinCharacter;
                        CustomSkinCharacterVariant = packetSkinCharacterVariant;

                        if (currentGameMode == GameMode._1v1 && Player.nr == 1) opponentLobbyCharacterModel.SetCharacterLobby(Player.nr, packetSkinCharacter, packetSkinCharacterVariant, true);
                        else opponentLobbyCharacterModel.SetCharacterLobby(Player.nr, packetSkinCharacter, packetSkinCharacterVariant, false);
                    }
                    else
                    {
                        if (Player.CharacterSelected != Character.NONE && Player.CharacterSelected != (Character)32 && Player.CharacterVariant != CharacterVariant.CORPSE)
                        {
                            CustomSkinCharacter = Player.CharacterSelected;
                            CustomSkinCharacterVariant = Player.CharacterVariant;
                            if (currentGameMode == GameMode._1v1 && Player.nr == 1) opponentLobbyCharacterModel.SetCharacterLobby(Player.nr, packetSkinCharacter, packetSkinCharacterVariant, true);
                            else opponentLobbyCharacterModel.SetCharacterLobby(Player.nr, Player.CharacterSelected, Player.CharacterVariant, false);
                        }
                        initOpponentPlayer = true;
                    }
                }

                if ((modelCharacter != customSkin.Character || modelCharacterVariant != customSkin.CharacterVariant) &&
                    (Player.CharacterSelected != Character.NONE || Player.CharacterVariant != CharacterVariant.CORPSE))
                {
                    if (ShouldRefreshSkin && customSkin != null)
                    {

                        if (this.customSkin.Character != Character.NONE && this.customSkin.CharacterVariant != CharacterVariant.CORPSE && Player.CharacterSelected != (Character)32)
                        {
                            characterModel.SetCharacterLobby(this.Player.nr, this.customSkin.Character, this.customSkin.CharacterVariant, true);
                        }
                        characterModel.PlayCamAnim();
                        CheckMirrors();
                        ShouldRefreshSkin = false;
                    }
                }
            }
        }
    }
}
