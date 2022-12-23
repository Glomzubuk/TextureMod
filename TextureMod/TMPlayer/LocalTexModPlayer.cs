using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using GameplayEntities;
using LLGUI;
using Multiplayer;
using LLHandlers;
using LLScreen;
using LLBML;
using LLBML.Players;
using LLBML.States;
using TextureMod.CustomSkins;

namespace TextureMod.TMPlayer
{
    public class LocalTexModPlayer : TexModPlayer
    {
        public static Player LocalLobbyPlayer => Player.GetPlayer(P2P.localPeer?.playerNr ?? 0);
        public static PlayerEntity LocalGamePlayerEntity => LocalLobbyPlayer?.playerEntity;

        public static int localPlayerNr => P2P.localPeer?.playerNr ?? 0;

        public int skinIndex = -1;
        public LocalTexModPlayer(Player player, CustomSkin skin = null, CharacterModel model = null) 
            : base(player, skin ?? CheckForStoredSkin(player), model)
        {
        }

        private static CustomSkin CheckForStoredSkin(Player player)
        {
            return null;
        }

        public override void Update()
        {
            base.Update();

            if (this.Player != null) // Determine and assign skin to local player
            {
                //Player.Selected - Has the Player selected their character yet.
                if (this.Player.selected)
                {
                    if (GameStates.IsInLobby())
                    {
                        //HandleSwitchSkinInputs();
                    }
                    /*
                    if (HandleSwitchSkinInputs()) // Assign skin to local player
                    {
                        if (setAntiMirrior)
                        {
                            string opponentSkinPath = Path.Combine(imageFolder, "opponent.png");
                            opponentCustomTexture = TextureHelper.LoadPNG(opponentSkinPath);
                            setAntiMirrior = false;
                        }
                        */
                    bool isRandom = false;
                        /*
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
                                GameStatesLobbyUtils.RefreshLocalPlayerState();
                                if (lockButtonsOnRandom.Value)
                                {
                                    foreach (LLButton b in buttons) b.SetActive(false);
                                    randomizedChar = true;
                                }
                            }

                            isRandom = true;
                        }*/
                        /*
                        if (TextureChanger.InLobby(GameType.Online))
                        {
                            doSkinPost = true;
                            postTimer = 0;
                            setAntiMirrior = false;
                            calculateMirror = true;
                        }*/
//                    }
                }


            }

            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (screenOne != null)
            {
                if (screenOne?.screenType == ScreenType.UNLOCKS_SKINS)
                {
                    var screenUnlocksSkins = screenOne as ScreenUnlocksSkins;

                    CharacterModel previewModel = screenUnlocksSkins.previewModel;
                    /*if (silouetteTimer > 0)
                    {
                        if (customSkin != null)
                        {
                            characterModel.SetSilhouette(false);
                            AssignTextureToCharacterModelRenderers(characterModel, localPlayer.customSkin.Texture);
                        }
                    }*/
                    /*
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
                    }*/
                }
                else if (screenOne?.screenType == ScreenType.UNLOCKS_CHARACTERS)
                {
                    /*
                    localCustomSkin = null;
                    intervalMode = false;
                    reloadCustomSkinTimer = skinReloadIntervalInFrames.Value;
                    */
                }
            }
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
            if (Player.CharacterSelectedIsRandom)
            {

            }
            else
            {
                // TODO Improve that
                List<CustomSkinHandler> skins = SkinsManager.skinCache.GetUsableHandlers(character);
                if (skins == null) return;

                Logger.LogDebug($"Counter: {skinCounter}, skin length: {skins.Count}");
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




        void DisableCharacterButtons()
        {
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];
            if (Player.CharacterSelectedIsRandom && screenOne != null) // If you have randomized your character, activate buttons again
            {
                if (screenOne.screenType == ScreenType.PLAYERS_STAGE || screenOne.screenType == ScreenType.PLAYERS_STAGE_RANKED)
                {
                    LLButton[] buttons = UnityEngine.Object.FindObjectsOfType<LLButton>();
                    foreach (LLButton b in buttons) b.SetActive(true);
                }
            }
        }

        private void HandleSwitchSkinInputs()
        {
            LLButton[] buttons = UnityEngine.Object.FindObjectsOfType<LLButton>();

            if (TextureMod.useOnlySetKey.Value == true)
            {
                HandleSkinChangeButtonDown();
                return;
            }

            if (Input.GetKey(TextureMod.holdKey1.Value) && buttons.Length > 0)
            {
                HandleSkinChangeButtonDown();

                foreach (LLButton b in buttons) b.SetActive(false); //Deactivate buttons
            }
            else if (Input.GetKeyUp(TextureMod.holdKey1.Value) && buttons.Length > 0)
            {
                foreach (LLButton b in buttons) b.SetActive(true); //Reactivate buttons
            }
        }


        private void HandleSkinChangeButtonDown()
        {
            var test = Input.mousePosition;
            ScreenPlayers.GetCursorX();

            

            if (Input.GetKeyDown(TextureMod.nextSkin.Value) || Controller.FromNr(this.Player.nr, false).GetButtonDown(InputAction.EXPRESS_RIGHT))
            {
                NextSkin(this.Player.Character);
            }
            else if (Input.GetKeyDown(TextureMod.previousSkin.Value) || Controller.FromNr(this.Player.nr, false).GetButtonDown(InputAction.EXPRESS_LEFT))
            {
                PreviousSkin(this.Player.Character);
            }
        }
    }
}
