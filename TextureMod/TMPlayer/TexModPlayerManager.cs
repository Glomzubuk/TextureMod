using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Multiplayer;
using BepInEx.Logging;
using LLBML;
using LLBML.Players;
using LLBML.GameEvents;

namespace TextureMod.TMPlayer
{
    public class TexModPlayerManager : MonoBehaviour
    {
        public static TexModPlayerManager Instance { get; private set; }
        private static ManualLogSource Logger => TextureMod.Log;

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


        public TexModPlayerManager()
        {
        }
        void Awake()
        {
            Instance = this;
            LobbyEvents.OnLobbyReady += LobbyEvents_OnLobbyReady;
        }


        void OnDestroy()
        {
            LobbyEvents.OnLobbyReady -= LobbyEvents_OnLobbyReady;
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
            });/*
            ForAllRemoteTexmodPlayers((TexModPlayer tmp) =>
            {
                ExchangeClient.SendSkinCheck(tmp.Player.nr);
            });*/
        }

        void Update()
        {
            for (int i = 0; i < Player.MAX_PLAYERS; i++)
            {
                Player player = Player.GetPlayer(i);
                if (player == null)
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
        }

        private void UpdatePlayers()
        {
            foreach (TexModPlayer tmPlayer in tmPlayers)
            {
                tmPlayer.Update();
            }
        }

        public void ManageMirrorSkins()
        {
            ForAllTexmodPlayers((TexModPlayer tmp) =>
            {
                if (tmp.Player.nr == localPlayer.Player.nr) return;
                if (tmp.HasCustomSkin() && tmp.customSkin?.SkinHash == localPlayer.customSkin?.SkinHash)
                {
                    tmp.SetColorFilter((SkinColorFilter)tmp.Player.nr + 1);
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



        public static void ForAllRemoteTexmodPlayers(Action<TexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer != null) action(tmPlayer);
            }
        }

        public static void ForAllTexModPlayersInMatch(Action<TexModPlayer> action)
        {
            foreach (TexModPlayer tmPlayer in Instance.tmPlayers)
            {
                if (tmPlayer.Player.IsInMatch) action(tmPlayer);
            }
        }
    }
}
