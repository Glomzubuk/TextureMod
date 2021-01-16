using LLScreen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace TextureMod
{
    public class TextureLoader : MonoBehaviour
    {
        private static string characterImagesFolder = Path.Combine(TextureMod.resourceFolder, "Characters");
        public List<string> chars = new List<string>();
        public Dictionary<Character, Dictionary<string, Texture2D>> characterTextures = new Dictionary<Character, Dictionary<string, Texture2D>>();

        public bool loadingExternalFiles = true;
        public bool hasCactuar = false;

        private void Start()
        {
            LoadLibrary();
        }

        public void LoadLibrary()
        {
            chars.Clear();
            Resources.UnloadUnusedAssets();
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
                        string cleanfile = Path.GetFileName(file);
                        if (cleanfile.Contains("#"))
                        {
                            List<char> charsFile = cleanfile.ToCharArray().ToList();
                            for (var i = 0; i < charsFile.Count; i++)
                            {
                                if (charsFile[0] != '#') charsFile.RemoveAt(0);
                                else
                                {
                                    charsFile.RemoveAt(0);
                                    break;
                                }
                            }
                            cleanfile = new string(charsFile.ToArray());
                        }

                        string cleanDir = Path.GetFileName(dir);
                        if (cleanDir.Contains("#"))
                        {   
                            List<char> charsDir = cleanDir.ToCharArray().ToList();
                            for (var i = 0; i < charsDir.Count; i++)
                            {
                                if (charsDir[0] != '#') charsDir.RemoveAt(0);
                                else
                                {
                                    charsDir.RemoveAt(0);
                                    break;
                                }
                            }
                            cleanDir = new string(charsDir.ToArray());
                        }

                        skins.Add(cleanfile + " by " + cleanDir, TextureHelper.LoadPNG(file));
                    }
                }
                foreach (string file in Directory.GetFiles(character))
                {
                    string cleanfile = Path.GetFileName(file);
                    if (cleanfile.Contains("#"))
                    {
                        List<char> charsFile = cleanfile.ToCharArray().ToList();
                        for (var i = 0; i < charsFile.Count; i++)
                        {
                            if (charsFile[0] != '#') charsFile.RemoveAt(0);
                            else
                            {
                                charsFile.RemoveAt(0);
                                break;
                            }
                        }
                        cleanfile = new string(charsFile.ToArray());
                    }
                    skins.Add(cleanfile, TextureHelper.LoadPNG(file));
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
                    hasCactuar = true;
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
