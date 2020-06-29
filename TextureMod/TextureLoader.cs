using LLScreen;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace TextureMod
{
    public class TextureLoader : MonoBehaviour
    {
        private static string characterImagesFolder = Path.Combine(TextureMod.resourceFolder, "Characters");
        public List<string> chars = new List<string>();
        public Dictionary<Character, Dictionary<string, Texture2D>> characterTextures = new Dictionary<Character, Dictionary<string, Texture2D>>();

        public bool loadingExternalFiles = true;

        private void Start()
        {
            LoadLibrary();
        }

        public void LoadLibrary()
        {
            chars.Clear();
            characterTextures.Clear();

            foreach (string path in Directory.GetDirectories(characterImagesFolder))
            {
                chars.Add(path);
            }

            foreach (string character in chars)
            {
                Dictionary<string, Texture2D> skins = new Dictionary<string, Texture2D>();
                foreach (string dir in Directory.GetDirectories(character))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        skins.Add(Path.GetFileName(file) + " by " + Path.GetFileName(dir), TextureHelper.LoadPNG(file));
                    }
                }
                foreach (string file in Directory.GetFiles(character))
                {
                    skins.Add(Path.GetFileName(file), TextureHelper.LoadPNG(file));
                }
                characterTextures.Add(StringToChar(Path.GetFileName(character)), skins);
            }
            UIScreen.SetLoadingScreen(false);
            loadingExternalFiles = false;
        }

        public Character StringToChar(string charString)
        {
            Character ret = Character.NONE;
            switch (charString)
            {
                case "CANDYMAN":
                    ret = Character.CANDY;
                    break;
                case "DICE":
                    ret = Character.PONG;
                    break;
                case "DOOMBOX":
                    ret = Character.BOSS;
                    break;
                case "GRID":
                    ret = Character.ELECTRO;
                    break;
                case "JET":
                    ret = Character.SKATE;
                    break;
                case "LATCH":
                    ret = Character.CROC;
                    break;
                case "NITRO":
                    ret = Character.COP;
                    break;
                case "RAPTOR":
                    ret = Character.KID;
                    break;
                case "SONATA":
                    ret = Character.BOOM;
                    break;
                case "SWITCH":
                    ret = Character.ROBOT;
                    break;
                case "TOXIC":
                    ret = Character.GRAF;
                    break;
                case "DUST&ASHES":
                    ret = Character.BAG;
                    break;
                case "CACTUAR":
                    ret = (Character)50;
                    break;
                default:
                    ret = Character.NONE;
                    break;
            }
            return ret;
        }
    }
}
