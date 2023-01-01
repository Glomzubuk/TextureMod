using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Multiplayer;
using BepInEx.Logging;
using LLBML;
using LLBML.States;
using LLBML.Players;
using LLBML.Messages;
using LLBML.Networking;
using TextureMod.TMPlayer;
using TextureMod.CustomSkins;

namespace TextureMod
{
    public static class ExchangeClient
    {
        private static ManualLogSource Logger => TextureMod.Log;
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
            MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKINREQUEST, TexModMessages.TEXMOD_SKINREQUEST.ToString(), ReceiveSkinRequest);
            MessageApi.RegisterCustomMessage(pInfo, (ushort)TexModMessages.TEXMOD_SKIN, TexModMessages.TEXMOD_SKIN.ToString(), ReceiveSkinFromMessage);
            PlayerLobbyState.RegisterPayload(pInfo, OnSendPayload, OnReceivePayload);
            NetworkApi.RegisterModPacketCallback(pInfo, OnReceiveModPacket);
        }

        public static byte[] OnSendPayload(PlayerLobbyState pls)
        {
            if (!TextureMod.sendSkinsToOpponents.Value) return null;

            TexModPlayer tmPlayer = TexModPlayerManager.GetPlayer(pls.playerNr);
            TextureMod.Log.LogDebug("Test: " + pls.ToString());
            if (tmPlayer.Player.isLocal)
            {
                if (tmPlayer.HasCustomSkin())
                {
                    List<byte> payload = new List<byte>();
                    byte[] skinHash = (byte[])tmPlayer.CustomSkin.SkinHash;
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

            if (!TextureMod.receiveSkinsFromOpponents.Value) return;

            try
            {
                SkinHash skinHash = new SkinHash(rawHash);
                Logger.LogDebug($"Player {pls.playerNr} has custom skin: " + skinHash);
                CustomSkinHandler handler = SkinsManager.skinCache.GetHandlerFromHash(skinHash);
                if (handler != null)
                {
                    Logger.LogDebug($"Skin is known, applying." );
                    tmPlayer.SetCustomSkin(handler);
                    GameStatesLobbyUtils.RefreshPlayerState(tmPlayer.Player);
                }
                else
                {
                    Logger.LogDebug($"Skin is not known, requesting.");
                    SendSkinRequest(tmPlayer.Player.nr, skinHash);
                }
    
            }
            catch (Exception e)
            {
                Logger.LogError("Caught Exception trying to receive lobby state: " + e);
            }
        }

        private static int transactionIDCounter = 0;
        public static void SendSkinRequest(int playerNrToRequest, SkinHash skinHash)
        {
            TextureMod.Log.LogDebug("Requesting skin with hash: " + skinHash);
            NetworkApi.SendMessageToPlayer(playerNrToRequest, new Message((Msg)TexModMessages.TEXMOD_SKINREQUEST, P2P.localPeer.playerNr, transactionIDCounter++, skinHash.Bytes, skinHash.Bytes.Length));
        }

        public static void ReceiveSkinRequest(Message msg)
        {
            if (msg.playerNr == P2P.localPeer.playerNr) return;
            SkinHash skinHash = new SkinHash((byte[])msg.ob);
            TextureMod.Log.LogDebug("Received skin request for hash: " + skinHash);
            CustomSkin skin = SkinsManager.skinCache.GetSkinFromHash(skinHash);
            if (skin != null)
            {
                SendSkin(msg.playerNr, msg.index, skin);
            }
        }

        public static void SendSkin(int playerNrToSendSkin, int requestID, CustomSkin skin)
        {
            TextureMod.Log.LogDebug($"Sending skin {skin.Name} to player {playerNrToSendSkin}");
            byte[] skinAsBytes = skin.ToBytes();
            //NetworkApi.SendMessageToPlayer(playerNrToSendSkin, new Message((Msg)TexModMessages.TEXMOD_SKIN, P2P.localPeer.playerNr, requestID, skinAsBytes));
            NetworkApi.SendModPacket(TextureMod.Instance.Info, Player.GetPlayer(playerNrToSendSkin), skinAsBytes);
        }

        public static void OnReceiveModPacket(Peer sender, byte[] data)
        {
            ReceiveSkin(Player.GetPlayer(sender.playerNr), CustomSkin.FromBytes(data));
        }

        public static void ReceiveSkinFromMessage(Message msg)
        {
            ReceiveSkin(Player.GetPlayer(msg.playerNr), CustomSkin.FromBytes((byte[])msg.ob));
        }

        public static void ReceiveSkin(Player sender, CustomSkin receivedSkin)
        {
            TextureMod.Log.LogDebug($"Received skin from player {sender.nr}, is it null? {(receivedSkin == null ? "Yes" : "No")}");
            if (receivedSkin != null)
            {
                TextureMod.Log.LogDebug($"Successfuly received {receivedSkin.Name} from player {sender.nr}");

                CustomSkinHandler receivedSkinHandler = new CustomSkinHandler(receivedSkin)
                {
                    IsRemote = true
                };
                //TODO Load skin in cache
                SkinsManager.skinCache.Add(receivedSkinHandler.CustomSkin.Character, receivedSkinHandler);
                TextureMod.Instance.tc.debug[3] = "Got non null skin and set should refresh to true";
                TexModPlayer player = TexModPlayerManager.Instance.tmPlayers[sender.nr];
                player.SetCustomSkin(receivedSkinHandler);
            }
        }
    }
}
