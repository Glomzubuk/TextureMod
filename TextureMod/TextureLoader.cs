using LLHandlers;
using LLScreen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using BepInEx.Logging;


namespace TextureMod
{
    public class TextureLoader : MonoBehaviour
    {
        public static TextureLoader Instance { get; private set; }
        private static readonly ManualLogSource Logger = TextureMod.Log;
        private static readonly string charactersFolder = BepInEx.Utility.CombinePaths(TextureMod.ResourceFolder, "Images", "Characters");
        public List<string> chars;
        //public Dictionary<Character, Dictionary<string, Texture2D>> characterTextures = new Dictionary<Character, Dictionary<string, Texture2D>>();
        public CustomSkinCache newCharacterTextures = new CustomSkinCache();
        Regex altRegex = new Regex(@"((_ALT\d?$)|(^\d+#))");


        public bool loadingExternalFiles = true;
        public bool hasCactuar = false;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            LoadLibrary();
        }

        private List<string> GetCharacterFolders()
        {
            return new List<string>(Directory.GetDirectories(charactersFolder));
        }

        public void ReloadChacterSpecificSkins(Character character)
        {
            List<CustomSkinHandler> characterSkins = newCharacterTextures[character];
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
                    Character character = StringToChar(Path.GetFileName(characterFolder));
                    bool hasDLC = CheckHasDLCForCharacter(character);

                    foreach (string file in Directory.GetFiles(characterFolder, "*.png", SearchOption.TopDirectoryOnly))
                    {
                        ModelVariant modelVariant = VariantHelper.GetModelVariantFromFilePath(file);
                        if (modelVariant == ModelVariant.DLC && hasDLC == false)
                        {
                            continue;
                        }
                        string cleanName = Path.GetFileNameWithoutExtension(file);
                        cleanName = altRegex.Replace(cleanName, m => { return ""; });
                        newCharacterTextures.Add(character, new CustomSkinHandler(character, modelVariant, cleanName, null, file));
                    }

                    foreach (string authorDir in Directory.GetDirectories(characterFolder))
                    {
                        string authorName = Path.GetFileName(authorDir);
                        authorName = altRegex.Replace(authorName, m => { return ""; });

                        foreach (string file in Directory.GetFiles(authorDir, "*.png", SearchOption.TopDirectoryOnly))
                        {
                            ModelVariant modelVariant = VariantHelper.GetModelVariantFromFilePath(file);
                            if (modelVariant == ModelVariant.DLC && hasDLC == false)
                            {
                                continue;
                            }

                            string cleanName = Path.GetFileNameWithoutExtension(file);
                            cleanName = altRegex.Replace(cleanName, m => { return ""; });
                            CustomSkinHandler newSkin = new CustomSkinHandler(character, modelVariant, cleanName, authorName, file);
                            newCharacterTextures.Add(character, newSkin);
                        }
                    }
                }

                //UIScreen.SetLoadingScreen(false);
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
