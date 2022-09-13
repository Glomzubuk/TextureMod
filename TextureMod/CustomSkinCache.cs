using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using BepInEx.Logging;
using LLBML;
using LLBML.Utils;

namespace TextureMod
{
    public class CustomSkinCache : GenericCache<Character, CustomSkinHandler>
    {
        public static readonly Regex altRegex = new Regex(@"((_ALT\d?$)|(^\d+#))");
        protected static new ManualLogSource Logger => TextureMod.Log;

        public void LoadSkins(DirectoryInfo texLibPath)
        {
            var unload = Resources.UnloadUnusedAssets();

            foreach (Character character in CharacterApi.GetPlayableCharacters())
            {
                DirectoryInfo characterFolder = texLibPath.CreateSubdirectory(StringUtils.GetCharacterSafeName(character).ToUpper());
                this.LoadSkins(character, characterFolder);
            }
        }

        public void LoadSkins(Character character, DirectoryInfo characterDirPath)
        {
            foreach (FileInfo file in characterDirPath.GetFiles("*.png", SearchOption.TopDirectoryOnly))
            {
                this.LoadSkin(character, file);
            }

            foreach (DirectoryInfo authorDir in characterDirPath.GetDirectories())
            {
                string authorName = authorDir.Name;
                authorName = altRegex.Replace(authorName, m => { return ""; });

                foreach (FileInfo file in authorDir.GetFiles("*.png", SearchOption.TopDirectoryOnly))
                {
                    this.LoadSkin(character, file, authorName);
                }
            }
        }

        public void LoadSkin(Character character, FileInfo skinFile, string authorName = null)
        {
            ModelVariant modelVariant = VariantHelper.GetModelVariantFromFilePath(skinFile.Name);

            string cleanName = Path.GetFileNameWithoutExtension(skinFile.Name);
            cleanName = altRegex.Replace(cleanName, m => { return ""; });
            var newHandler = new CustomSkinHandler(character, modelVariant, cleanName, authorName, skinFile.FullName);

            this.Add(character, newHandler);
        }


        public override void Add(Character key, CustomSkinHandler newSkin)
        {
            CustomSkinHandler handler = this.GetHandlerFromHash(newSkin.CustomSkin.SkinHash);
            if (handler != null)
            {
                Logger.LogWarning($"There's already a skin with the following hash in cache: {newSkin.CustomSkin.SkinHash}. Couldn't add {newSkin.CustomSkin.Name} to the cache");
                return;
            }
            base.Add(key, newSkin);       
        }

        public List<CustomSkinHandler> GetHandlers(Character key)
        {
            if (this.ContainsKey(key)) return this[key];
            else
            {
                Logger.LogWarning($"No skins for {key.ToString()}.");
                return null;
            }
        }

        public CustomSkinHandler GetHandler(Character key)
        {
            return this.GetHandlers(key)?[0];
        }

        public List<CustomSkin> GetSkins(Character key)
        {
            return this.GetHandlers(key)?.Select((CustomSkinHandler csh) => csh.CustomSkin).ToList();
        }

        public CustomSkin GetSkin(Character key)
        {
            return this.GetHandler(key)?.CustomSkin;
        }

        public List<CustomSkin> GetUsableSkins(Character key)
        {
            return this.GetHandlers(key)?
                .Where((CustomSkinHandler csh) => csh.CanBeUsed() == true)
                .Select((CustomSkinHandler csh) => csh.CustomSkin)
                .ToList();
        }
        public CustomSkin GetUsableSkin(Character key)
        {
            return this.GetUsableSkins(key)?[0];
        }

        public CustomSkinHandler GetHandlerFromHash(SkinHash hash)
        {
            foreach (List<CustomSkinHandler> handlers in this.cache.Values)
            {
                foreach (CustomSkinHandler handler in handlers)
                {
                    if (handler.CustomSkin.SkinHash == hash)
                    {
                        return handler;
                    }
                }
            }
            return null;
        }


        public CustomSkin GetSkinFromHash(SkinHash hash)
        {
            return this.GetHandlerFromHash(hash)?.CustomSkin;
        }
    }
}
