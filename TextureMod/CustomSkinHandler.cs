using System;
using System.IO;
using UnityEngine;
using BepInEx.Logging;
using LLBML.Texture;

namespace TextureMod
{
    public class CustomSkinHandler
    {
        private static ManualLogSource Logger => TextureMod.Log;

        public CustomSkin CustomSkin { get; private set; }

        public bool InUse { get; private set; } = false;
        public bool Enabled { get; private set; } = true;
        public bool InMemory { get; private set; } = false;
        public FileInfo FileLocation { get; private set; } = null;


        public CustomSkinHandler(CustomSkin skin)
        {
            this.InMemory = true;
            this.FileLocation = null;
            this.CustomSkin = skin;
        }

        public CustomSkinHandler(Character character, ModelVariant modelVariant, string skinName, string author, Texture2D texture)
        {
            Logger.LogDebug($"Creating skin: {character} | {modelVariant} | {skinName} | {author} | InMemory");
            this.InMemory = true;
            this.FileLocation = null;
            this.CustomSkin = new CustomSkin(character, modelVariant, skinName, author, texture);
        }

        public CustomSkinHandler(Character character, ModelVariant modelVariant, string skinName, string author, string filePath)
        {
            Logger.LogDebug($"Creating skin: {character} | {modelVariant} | {skinName} | {author} | {filePath}");
            this.FileLocation = new FileInfo(filePath);
            this.CustomSkin = new CustomSkin(character, modelVariant, skinName, author, TextureUtils.LoadPNG(FileLocation));

        }

        public CustomSkinHandler(Character character, ModelVariant modelVariant, string skinName, string author, FileInfo file)
        {
            Logger.LogDebug($"Creating skin: {character} | {modelVariant} | {skinName} | {author} | {file.FullName}");
            this.FileLocation = file;
            this.CustomSkin = new CustomSkin(character, modelVariant, skinName, author, TextureUtils.LoadPNG(FileLocation));
        }

        public void ReloadSkin()
        {
            if (InMemory)
            {
                Logger.LogInfo($"Loading Texture at: {FileLocation}");
                this.CustomSkin.SetTexture(TextureUtils.LoadPNG(FileLocation));
            }
            else
            {
                throw new FileNotFoundException($"Skin '{this.CustomSkin.Name}' doesn't have a file registered");
            }
        }

        public void SaveToDisk(string newFilePath = null)
        {
            if (newFilePath != null)
            {
                if (FileLocation != null)
                {
                    Logger.LogWarning($"Overriting existing path for texture {this.CustomSkin.Name}: \n" +
                    $"\t - Old one: {this.FileLocation}\n" +
                    $"\t - New one: {newFilePath}");
                }
                this.FileLocation = new FileInfo(newFilePath);
            }
            else if (FileLocation == null)
            {
                throw new FileNotFoundException($"Skin '{this.CustomSkin.Name}' doesn't have a file registered and none was given");
            }
            File.WriteAllBytes(this.FileLocation.FullName, this.CustomSkin.Texture.EncodeToPNG());
        }

        public bool CanBeUsed()
        {

            if (CustomSkin.ModelVariant == ModelVariant.DLC && !TextureMod.ownedDLCs.Contains(CustomSkin.Character))
            {
                Logger.LogWarning($"You tried to use a skin for which you do not have the DLC: {CustomSkin.Character.ToString()}. Couldn't add {CustomSkin.Name} to the cache");
                //TODO Reenable this when i find a fix for the ownedDLCs
                //return false;
            }
            return true;
        }
    }
}
