using HarmonyLib;
using UnityEngine;
using GameplayEntities;
using Multiplayer;
using LLBML.Players;

namespace TextureMod.TMPlayer
{
    public class LocalTexModPlayer : TexModPlayer
    {
        public static Player LocalLobbyPlayer => Player.GetPlayer(P2P.localPeer?.playerNr ?? 0);
        public static PlayerEntity LocalGamePlayerEntity => LocalLobbyPlayer?.playerEntity;

        public static int localPlayerNr => P2P.localPeer?.playerNr ?? 0;

        public int skinIndex = -1;
        public LocalTexModPlayer(Player player, CustomSkin skin = null, CharacterModel model = null) : base(player, skin, model)
        {
        }



        public override void Update()
        {
            base.Update();
            if (this.Player != null && randomizedChar == false) // Determine and assign skin to local player
            {
                //Player.Selected - Has the Player selected their character yet.
                if (this.Player.selected)
                {

                    if (localPlayer.Player.CharacterSelected != localLobbyPlayer.CharacterSelected || localPlayerCharVar != localLobbyPlayer.CharacterVariant)
                    {
                        if (assignFirstSkinOnCharacterSelection.Value)
                        {
                            this.NextSkin();
                        }
                        else
                        {
                            initLocalPlayer = true;
                        }
                        localPlayerChar = localLobbyPlayer.CharacterSelected;
                        localPlayerCharVar = localLobbyPlayer.CharacterVariant;
                    }

                    if (HandleSwitchSkinInputs()) changeSkin = true;

                    if (changeSkin && InLobby(GameType.Any)) // Assign skin to local player
                    {
                        if (setAntiMirrior)
                        {
                            string opponentSkinPath = Path.Combine(imageFolder, "opponent.png");
                            opponentCustomTexture = TextureHelper.LoadPNG(opponentSkinPath);
                            setAntiMirrior = false;
                        }

                        HDLIJDBFGKN gameStatesOnlineLobby = UnityEngine.Object.FindObjectOfType<HDLIJDBFGKN>();
                        if (TextureChanger.InLobby(GameType.Online))
                        {
                            AccessTools.Method(typeof(HDLIJDBFGKN), "JPNNBHNHHJC").Invoke(gameStatesOnlineLobby, null); // AutoReadyReset
                            AccessTools.Method(typeof(HDLIJDBFGKN), "EMFKKOJEIPN").Invoke(gameStatesOnlineLobby, new object[] { this.Player.nr, false }); // SetReady

                            AccessTools.Method(typeof(HDLIJDBFGKN), "BFIGLDLHKPO").Invoke(gameStatesOnlineLobby, null); // UpdateReadyButton
                            AccessTools.Method(typeof(HDLIJDBFGKN), "OFGNNIBJOLH").Invoke(gameStatesOnlineLobby, new object[] { this.Player }); // SendPlayerState
                                                                                                                                                /*
                                                                                                                                                gameStatesOnlineLobby.JPNNBHNHHJC(); // gameStatesOnlineLobby.AutoReadyReset
                                                                                                                                                gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, false); // SetReady
                                                                                                                                                gameStatesOnlineLobby.BFIGLDLHKPO(); // gameStatesOnlineLobby.UpdateReadyButton
                                                                                                                                                gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer); // gameStatesOnlineLobby.SendPlayerState
                                                                                                                                                */
                        }

                        bool isRandom = false;
                        if (this.Player.CharacterSelectedIsRandom) // Randomize skin and char
                        {
                            //Creats a list of characters that have no skins and should be excluded from the character randomizer
                            List<Character> characters = new List<Character>();
                            foreach (var character in TextureMod.Instance.tl.newCharacterTextures)
                            {
                                if (character.Value.Count == 0)
                                {
                                    characters.Add(character.Key);
                                }
                            }

                            Character randomChar = localLobbyPlayer.HGPNPNPJBMK(characters.ToArray());
                            localLobbyPlayer.CharacterSelected = randomChar;

                            if (InLobby(GameType.Online))
                            {
                                AccessTools.Method(typeof(HDLIJDBFGKN), "EMFKKOJEIPN").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer.CJFLMDNNMIE, true }); // SetReady
                                AccessTools.Method(typeof(HDLIJDBFGKN), "OFGNNIBJOLH").Invoke(gameStatesOnlineLobby, new object[] { localLobbyPlayer }); // SendPlayerState
                                                                                                                                                         /*
                                                                                                                                                         gameStatesOnlineLobby.EMFKKOJEIPN(localLobbyPlayer.CJFLMDNNMIE, true); // SetReady
                                                                                                                                                         gameStatesOnlineLobby.OFGNNIBJOLH(localLobbyPlayer); //Send player state (Signalizes that we have changes characters and that we are ready)
                                                                                                                                                         */

                                if (lockButtonsOnRandom.Value)
                                {
                                    foreach (LLButton b in buttons) b.SetActive(false);
                                    randomizedChar = true;
                                }
                            }

                            isRandom = true;
                        }

                        SetLocalCustomSkin(localLobbyPlayer.CharacterSelected, isRandom);

                        if (TextureChanger.InLobby(GameType.Online))
                        {
                            doSkinPost = true;
                            postTimer = 0;
                            setAntiMirrior = false;
                            calculateMirror = true;
                        }
                    }
                }
            }
        }

        public void NextSkin(Character character, bool random = false)
        {
            if (customSkin.Character != Player.CharacterSelected && !Player.CharacterSelectedIsRandom)
            {

            }
        }
    }
}
