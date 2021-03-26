﻿using LLHandlers;
using LLScreen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;


namespace TextureMod
{
    public class TextureLoader : MonoBehaviour
    {
        private readonly string resourceFolder = Application.dataPath.Replace("/", @"\") + @"\Managed\TextureModResources\Images\Characters\";
        public List<string> chars;
        //public Dictionary<Character, Dictionary<string, Texture2D>> characterTextures = new Dictionary<Character, Dictionary<string, Texture2D>>();
        public Dictionary<Character, List<CustomSkin>> newCharacterTextures = new Dictionary<Character, List<CustomSkin>>();
        Regex regex = new Regex(@"((_ALT\d?$)|(^\d+#))");

        public bool loadingExternalFiles = true;
        public bool hasCactuar = false;

        private void Start()
        {
            LoadLibrary();
        }

        private List<string> GetCharacterFolders()
        {
            return new List<string>(Directory.GetDirectories(resourceFolder.Replace("/", @"\")));
        }

        public void ReloadChacterSpecificSkins(Character character)
        {
            List<CustomSkin> characterSkins = newCharacterTextures[character];
            for (int i = 0; i < characterSkins.Count - 1; i++)
            {
                characterSkins[i].ReloadSkin();
            }
        }

        public void LoadLibrary()
        {
            try
            {
                chars?.Clear();
                Resources.UnloadUnusedAssets();
                newCharacterTextures.Clear();

                chars = GetCharacterFolders();

                foreach (string characterFolder in chars)
                {
                    int indexer = 0;
                    List<CustomSkin> customSkins = new List<CustomSkin>();
                    Character character = StringToChar(Path.GetFileName(characterFolder));
                    bool hasDLC = CheckHasDLCForCharacter(character);

                    foreach (string file in Directory.GetFiles(characterFolder, "*.png", SearchOption.TopDirectoryOnly))
                    {
                        ModelVariant modelVariant = ModelVariantFromFile(file);
                        if (modelVariant == ModelVariant.DLC && hasDLC == false)
                        {
                            continue;
                        }
                        string cleanName = Path.GetFileNameWithoutExtension(file);
                        cleanName = regex.Replace(cleanName, m => { return ""; });
                        customSkins.Add(new CustomSkin(indexer, character, (CustomSkin.VariantType)modelVariant, cleanName, "", file));
                        indexer++;
                    }

                    foreach (string dir in Directory.GetDirectories(characterFolder))
                    {
                        string authorName = Path.GetFileName(dir);
                        authorName = regex.Replace(authorName, m => { return ""; });

                        foreach (string file in Directory.GetFiles(dir, "*.png", SearchOption.TopDirectoryOnly))
                        {
                            ModelVariant modelVariant = ModelVariantFromFile(file);
                            if (modelVariant == ModelVariant.DLC && hasDLC == false)
                            {
                                continue;
                            }

                            string cleanName = Path.GetFileNameWithoutExtension(file);
                            cleanName = regex.Replace(cleanName, m => { return ""; });
                            customSkins.Add(new CustomSkin(indexer, character, (CustomSkin.VariantType)modelVariant, cleanName, authorName, file));
                            indexer++;
                        }
                    }

                    newCharacterTextures.Add(character, customSkins);
                }

                UIScreen.SetLoadingScreen(false);
                loadingExternalFiles = false;
            }
            catch (Exception e)
            {
                TextureMod.loadingText = $"TextureMod failed to load textures";
                Debug.Log($"{e}");
                throw;
            }
        }

        bool CheckHasDLCForCharacter(Character character)
        {
            foreach (Character DLC in TextureMod.ownedDLCs)
            {
                if (DLC == character)
                {
                    return true;
                }
            }
            return false;
        }

        public enum ModelVariant
        {
            None,
            Default,
            Alternative,
            DLC,
        }

        ModelVariant ModelVariantFromFile(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.Contains("_ALT2"))
            {
                return ModelVariant.DLC;
            }
            else if (fileName.Contains("_ALT"))
            {
                return ModelVariant.Alternative;
            }
            else return ModelVariant.Default;
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
