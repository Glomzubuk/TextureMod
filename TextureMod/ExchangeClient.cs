using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

namespace TextureMod
{
    public class ExchangeClient : MonoBehaviour
    {
        private string serverUrl = "http://157.230.110.135:12700/";

        public IEnumerator PostSkin(string localPeerId, Character c, CharacterVariant cv, Texture2D texture)
        {
            byte[] imageData = texture.EncodeToPNG();
            string skin = Convert.ToBase64String(imageData);

            WWWForm body = new WWWForm();
            body.AddField("action", "Add");
            body.AddField("char", c.ToString());
            body.AddField("charVar", cv.ToString());
            body.AddField("skin", skin);

            UnityWebRequest www = UnityWebRequest.Post(serverUrl + "?ID=" + localPeerId, body);
            yield return www.SendWebRequest();
        }

        public IEnumerator GetSkin(string peerId)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(serverUrl + "?ID=" + peerId))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isDone)
                {
                    if (webRequest.downloadHandler.text != "null")
                    {
                        string[] rets = webRequest.downloadHandler.text.Split(':');
                        string character = rets[0];
                        string characterVariant = rets[1];
                        string skin = rets[2];
                        byte[] imageData = Convert.FromBase64String(skin);
                        File.WriteAllBytes(BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "opponent.png"), imageData);
                        TextureMod.Instance.tc.newSkinToApply = true;
                        TextureMod.Instance.tc.debug[3] = "Got non null skin and set newSkinToApply to true";
                        TextureMod.Instance.tc.opponentCustomSkinCharacter = (Character)Enum.Parse(typeof(Character), character);
                        TextureMod.Instance.tc.opponentCustomSkinCharacterVariant = (CharacterVariant)Enum.Parse(typeof(CharacterVariant), characterVariant);
                        TextureMod.Instance.tc.packetSkinCharacter = (Character)Enum.Parse(typeof(Character), character);
                        TextureMod.Instance.tc.packetSkinCharacterVariant = (CharacterVariant)Enum.Parse(typeof(CharacterVariant), characterVariant);
                    } else
                    {
                        TextureMod.Instance.tc.debug[3] = "Got null skin";
                    }
                }
            }
        }

        public IEnumerator PostCancel(string localPeerId)
        {
            WWWForm body = new WWWForm();
            body.AddField("action", "cancel");
            UnityWebRequest www = UnityWebRequest.Post(serverUrl + "?ID=" + localPeerId, body);
            yield return www.SendWebRequest();
        }
    }
}
