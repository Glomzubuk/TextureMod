using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Multiplayer;
using LLBML;
using LLBML.Messages;
using TextureMod.TMPlayer;

namespace TextureMod
{
    public static class ExchangeClient
    {
        public static void Init()
        {
            MessageApi.RegisterCustomMessage(TextureMod.Instance.Info, 40, "TEXMOD_SKINCHANGE", ReceiveCustomSkinChange);
            MessageApi.RegisterCustomMessage(TextureMod.Instance.Info, 41, "TEXMOD_SKINREQUEST", ReceiveSkinRequest);
            MessageApi.RegisterCustomMessage(TextureMod.Instance.Info, 42, "TEXMOD_SKIN", ReceiveSkin);
        }


        public static void SendCustomSkinChange(Hash128 skinHash)
        {
            P2P.SendOthers(new Message((Msg)40, P2P.localPeer.playerNr, -1, Encoding.ASCII.GetBytes(skinHash.ToString())));
        }

        public const int CUSTOM_SKIN_CHANGE_INDEX = 115;
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
                StateApi.SendMessage(new Message(Msg.SEL_SKIN, msg.playerNr, (int)TextureMod.skinCachesHandler[skinHash].CharacterVariant));
            }
        }

        public static void SendSkinRequest(int playerNrToRequest, Hash128 skinHash)
        {
            P2P.SendToPlayerNr(playerNrToRequest, new Message((Msg)41, P2P.localPeer.playerNr, -1, Encoding.ASCII.GetBytes(skinHash.ToString())));
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
            P2P.SendToPlayerNr(playerNrToSendSkin, new Message((Msg)42, P2P.localPeer.playerNr, skinAsBytes.Length, skinAsBytes));
        }

        public static void ReceiveSkin(Message msg)
        {
            CustomSkin receivedSkin = CustomSkin.FromBytes((byte[])msg.ob);
            if(receivedSkin != null)
            {
                //TODO Load skin in cache
                TextureMod.Instance.tc.debug[3] = "Got non null skin and set should refresh to true";
                TexModPlayer player = TextureMod.Instance.tc.tmPlayers[msg.playerNr];
                player.SetCustomSkin(receivedSkin);
            }
        }
    }
}
