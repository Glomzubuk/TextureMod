using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using LLBML;
using LLBML.Utils;

namespace TextureMod
{
    public class CustomSkinCache : GenericCache<Character, CustomSkinHandler>
    {
        public static readonly Regex altRegex = new Regex(@"((_ALT\d?$)|(^\d+#))");

        public void LoadSkins(DirectoryInfo texLibPath)
        {
            var unload = Resources.UnloadUnusedAssets();
            this.Clear();

            foreach (Character character in CharacterApi.GetPlayableCharacters())
            {
                DirectoryInfo characterFolder = texLibPath.CreateSubdirectory(StringUtils.GetCharacterSafeName(character));
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
            if (modelVariant == ModelVariant.DLC && !TextureMod.ownedDLCs.Contains(character))
            {
                return;
            }

            string cleanName = Path.GetFileNameWithoutExtension(skinFile.Name);
            cleanName = altRegex.Replace(cleanName, m => { return ""; });
            this.Add(character, new CustomSkinHandler(character, modelVariant, cleanName, authorName, skinFile.FullName));
        }


        public override void Add(Character key, CustomSkinHandler csh)
        {
            base.Add(key, csh);
        }

        public List<CustomSkinHandler> GetSkins(Character key)
        {
            return cache[key];
        }

        public CustomSkinHandler GetSkin(Character key)
        {
            return this.GetSkins(key)[0];
        }
    }
}
