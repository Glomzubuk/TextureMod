using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx.Logging;
using LLHandlers;
using LLBML;
using LLBML.Players;
using LLBML.GameEvents;
using LLBML.States;
using LLBML.Networking;
using TextureMod.CustomSkins;

namespace TextureMod.TMPlayer
{
    public class TexModPlayerManager : MonoBehaviour
    {
        public static TexModPlayerManager Instance { get; private set; }
        private static ManualLogSource Logger => TextureMod.Log;

        private static int reloadCustomSkinTimer = 0;

        public List<TexModPlayer> tmPlayers = new List<TexModPlayer>(new TexModPlayer[Player.MAX_PLAYERS]);
        public List<RemoteTexModPlayer> Opponents
        {
            get
            {
                return tmPlayers.Where((tmPlayer) => tmPlayer != null && tmPlayer.GetType() == typeof(RemoteTexModPlayer)).Cast<RemoteTexModPlayer>().ToList();
            }
        }
        public TexModPlayer localPlayer
        {
            get { return Player.LocalPlayerNumber < tmPlayers.Count ? tmPlayers[Player.LocalPlayerNumber] : null; }
            set { tmPlayers[Player.LocalPlayerNumber] = value; }
        }

        void Awake()
        {
            Instance = this;
            LobbyEvents.OnLobbyReady += LobbyEvents_OnLobbyReady;
            GameStateEvents.OnStateChange += GameEvents_OnGameIntro;
        }


        void OnDestroy()
        {
            LobbyEvents.OnLobbyReady -= LobbyEvents_OnLobbyReady;
            GameStateEvents.OnStateChange -= GameEvents_OnGameIntro;
        }


        void LobbyEvents_OnLobbyReady(object source, LobbyReadyArgs e)
        {
            Player.ForAll((Player p) => {
                if (p.isLocal)
                {
                    tmPlayers[p.nr] = new LocalTexModPlayer(p);
                }
                else
                {
                    tmPlayers[p.nr] = new RemoteTexModPlayer(p);
                }
            });
        }

        void GameEvents_OnGameIntro(object source, OnStateChangeArgs e)
        {
            if (e.newState == GameState.GAME_INTRO)
            {
                ForAllLocalTexmodPlayers((tmPlayer) => {
                    if (tmPlayer.Player.CharacterSelectedIsRandom && TextureMod.randomSkinOnRandomSelect.Value)
                    {
                        tmPlayer.SetRandomCustomSkin();
                        if (NetworkApi.IsOnline && tmPlayer.HasCustomSkin() && TextureMod.sendSkinsToOpponents.Value)
                        {
                            ExchangeClient.SendSkinNotice(tmPlayer.CustomSkin.SkinHash);
                        }
                    }
                });
            }
        }

        void Update()
        {
            for (int i = 0; i < Player.MAX_PLAYERS; i++)
            {
                Player player = Player.GetPlayer(i);
                if (player == null || player.playerStatus == PlayerStatus.NONE)
                {
                    tmPlayers[i] = null;
                }
                else if (tmPlayers[i] == null)
                {
                    if (player.isLocal)
                    {
                        tmPlayers[i] = new LocalTexModPlayer(player);
                    }
                    else
                    {
                        tmPlayers[i] = new RemoteTexModPlayer(player);
                    }
                }
            }

            UpdatePlayers();

            CheckForSkinReload();
        }

        private void UpdatePlayers()
        {
            foreach (TexModPlayer tmPlayer in tmPlayers)
            {
                tmPlayer?.Update();
            }
        }

        private void OnGUI()
        {
            foreach (TexModPlayer tmPlayer in tmPlayers)
            {
                tmPlayer?.OnGUI();
            }
        }

        public void HandleMirrorSkins()
        {
            ForAllTexmodPlayers((TexModPlayer tmp) =>
            {
                if (tmp.Player.nr == localPlayer.Player.nr) return;
                if (tmp.HasCustomSkin() && tmp.CustomSkin?.SkinHash == localPlayer.CustomSkin?.SkinHash)
                {
                    tmp.SetColorFilter((SkinColorFilter)tmp.Player.nr);
                }
            });
        }

        public static TexModPlayer GetPlayer(int nr)
        {
            //TODO DO Checks
            return Instance.tmPlayers[nr];
        }


        public static void ForAllTexmodPlayers(Action<TexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer != null) action(tmPlayer);
            }
        }


        public static void ForAllLocalTexmodPlayers(Action<LocalTexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer != null && tmPlayer is LocalTexModPlayer ltmp) action(ltmp);
            }
        }
        public static void ForAllRemoteTexmodPlayers(Action<RemoteTexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer != null && tmPlayer is RemoteTexModPlayer rtmp) action(rtmp);
            }
        }

        public static void ForAllTexModPlayersInMatch(Action<TexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer.Player.IsInMatch) action(tmPlayer);
            }
        }

        private void CheckForSkinReload()
        {

            if (Input.GetKeyDown(TextureMod.reloadCustomSkin.Value))
            {
                ReloadCurrentSkins();
            }
            else if (TextureMod.reloadCustomSkinOnInterval.Value)
            {
                if (!NetworkApi.IsOnline)
                {
                    if (reloadCustomSkinTimer > 0)
                    {
                        reloadCustomSkinTimer--;
                    }
                    else
                    {
                        ReloadCurrentSkins();
                        reloadCustomSkinTimer = TextureMod.skinReloadIntervalInFrames.Value;
                    }
                }
            }
        }


        public static bool InPostGame()
        {
            return (StateApi.CurrentGameMode == GameMode._1v1 || StateApi.CurrentGameMode == GameMode.FREE_FOR_ALL || StateApi.CurrentGameMode == GameMode.COMPETITIVE)
                && GameStates.GetCurrent() == GameState.GAME_RESULT;
        }

        public static void ReloadCurrentSkins()
        {
            ForAllLocalTexmodPlayers((tmp) =>
            {
                try { tmp?.skinHandler?.ReloadSkin(); }
                catch { AudioHandler.PlaySfx(Sfx.MENU_BACK); }
            });
        }
    }
}
