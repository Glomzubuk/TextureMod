using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Multiplayer;
using LLBML;
using LLBML.States;
using LLBML.Players;
using LLBML.Messages;
using TextureMod.TMPlayer;

namespace TextureMod
{
    public static class ExchangeClient
    {
        enum TexModMessages
        {
            TEXMOD_SKINCHECK = 4040,
            TEXMOD_SKINCHANGE = 4041,
            TEXMOD_SKINREQUEST = 4042,
            TEXMOD_SKIN = 4043,
        };
        public static void Init()
        {
            var pInfo = TextureMod.Instance.Info;
            //MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKINCHECK, TexModMessages.TEXMOD_SKINCHECK.ToString(), ReceiveSkinCheck);
            //MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKINCHANGE, TexModMessages.TEXMOD_SKINCHANGE.ToString(), ReceiveCustomSkinChange);
            MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKINREQUEST, TexModMessages.TEXMOD_SKINREQUEST.ToString(), ReceiveSkinRequest);
            MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKIN, TexModMessages.TEXMOD_SKIN.ToString(), ReceiveSkin);
            PlayerLobbyState.RegisterPayload(pInfo, OnSendPayload, OnReceivePayload);
        }

        public static byte[] OnSendPayload(PlayerLobbyState pls)
        {
            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(pls.playerNr);
            if (tmPlayer.Player.isLocal)
            {
                if (tmPlayer.HasCustomSkin())
                {
                    List<byte> payload = new List<byte>();
                    byte[] skinHash = Encoding.ASCII.GetBytes(tmPlayer.customSkin.SkinHash.ToString());
                    payload.Add((byte)skinHash.Length);
                    payload.AddRange(skinHash);
                    return payload.ToArray();
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public static void OnReceivePayload(PlayerLobbyState pls, byte[] payload)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(payload));
            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(pls.playerNr);
            int hashLength = br.ReadByte();
            if (hashLength == 0) return;
            byte[] rawHash = br.ReadBytes(hashLength);

            try
            {
                Hash128 skinHash = Hash128.Parse(Encoding.ASCII.GetString(rawHash));
                TextureMod.Log.LogDebug("Test: " + skinHash.ToString());
                if (tmPlayer.customSkin?.SkinHash != skinHash)
                {
                    if (TexModPlayerManager.skinCache.ContainsHash(skinHash))
                    {
                        tmPlayer.SetCustomSkin(TexModPlayerManager.skinCache[skinHash]);
                    }
                }

            }
            catch
            { }
        }
        /*
        public static void SendSkinCheck(int playerNr)
        {
            P2P.SendToPlayerNr(playerNr, new Message((Msg)TexModMessages.TEXMOD_SKINCHECK, P2P.localPeer.playerNr, -1, Encoding.ASCII.GetBytes(skinHash.ToString())));
        }


        public static void ReceiveSkinCheck(Message msg)
        {
            if (msg.playerNr == P2P.localPeer.playerNr) return;
            Hash128 skinHash = Hash128.Parse(Encoding.ASCII.GetString((byte[])msg.ob));

            if (TextureMod.skinCachesHandler.ContainsHash(skinHash))
            {
                SendSkin(msg.playerNr, TextureMod.skinCachesHandler[skinHash]);
            }
        }

        public static void SendCustomSkinChange(Hash128 skinHash)
        {
            P2P.SendOthers(new Message((Msg)TexModMessages.TEXMOD_SKINCHANGE, P2P.localPeer.playerNr, -1, Encoding.ASCII.GetBytes(skinHash.ToString())));
        }

        public static void ReceiveCustomSkinChange(Message msg)
        {
            if (msg.playerNr == P2P.localPeer.playerNr) return;
            Hash128 skinHash = Hash128.Parse(Encoding.ASCII.GetString((byte[])msg.ob));

            if(!TextureMod.skinCachesHandler.ContainsHash(skinHash))
            {
                SendSkinRequest(msg.playerNr, skinHash);
            }
            else
            {
                GameStates.Send(new Message(Msg.SEL_SKIN, msg.playerNr, (int)TextureMod.skinCachesHandler[skinHash].CharacterVariant));
            }
        }
        */
        public static void SendSkinRequest(int playerNrToRequest, Hash128 skinHash)
        {
            P2P.SendToPlayerNr(playerNrToRequest, new Message((Msg)TexModMessages.TEXMOD_SKINREQUEST, P2P.localPeer.playerNr, -1, Encoding.ASCII.GetBytes(skinHash.ToString())));
        }

        public static void ReceiveSkinRequest(Message msg)
        {
            if (msg.playerNr == P2P.localPeer.playerNr) return;
            Hash128 skinHash = Hash128.Parse(Encoding.ASCII.GetString((byte[])msg.ob));

            if (TextureMod.skinCachesHandler.ContainsHash(skinHash))
            {
                SendSkin(msg.playerNr, TextureMod.skinCachesHandler[skinHash]);
            }
        }

        public static void SendSkin(int playerNrToSendSkin, CustomSkin skin)
        {
            byte[] skinAsBytes = skin.ToBytes();
            //TODO That won' t work by default, default packet size limit is too small
            P2P.SendToPlayerNr(playerNrToSendSkin, new Message((Msg)TexModMessages.TEXMOD_SKIN, P2P.localPeer.playerNr, skinAsBytes.Length, skinAsBytes));
        }

        public static void ReceiveSkin(Message msg)
        {
            CustomSkin receivedSkin = CustomSkin.FromBytes((byte[])msg.ob);
            if(receivedSkin != null)
            {
                //TODO Load skin in cache
                TextureMod.Instance.tc.debug[3] = "Got non null skin and set should refresh to true";
                TexModPlayer player = TexModPlayerManager.Instance.tmPlayers[msg.playerNr];
                player.SetCustomSkin(receivedSkin);
            }
        }
    }
}
