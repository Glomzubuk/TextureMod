using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BepInEx.Logging;

namespace TextureMod
{
    public class CustomSkin
    {
        public static ManualLogSource Logger = TextureMod.Log;

        public Hash128 SkinHash { get; private set; }
        public Character Character { get; private set; }
        public ModelVariant ModelVariant { get; private set; }
        public CharacterVariant CharacterVariant => VariantHelper.GetDefaultVariantForModel(ModelVariant);
        public Texture2D Texture { get; private set; }
        //public List<Color> SkinColors { get; private set; }
        public string Author { get; private set; }
        public string Name { get; private set; }
        public string FileLocation { get; private set; }

        public CustomSkin(Character _character, ModelVariant _variant, string _name, string _author, Texture2D texture)
        {
            Character = _character;
            ModelVariant = _variant;
            Name = Texture.name = _name;
            Author = _author;
            Texture = texture;
            FileLocation = null;
        }
        public CustomSkin(Character character, ModelVariant modelVariant, string skinName, string author, string filePath)
        {
            Character = character;
            ModelVariant = modelVariant;
            Name = Texture.name = skinName;
            Author = author;
            FileLocation = filePath;
            Texture = TextureHelper.LoadPNG(FileLocation);
        }

        public void ReloadSkin()
        {
            if (FileLocation != null)
            {
                Logger.LogInfo($"Loading Texture at: {FileLocation}");
                Texture = TextureHelper.LoadPNG(FileLocation);
                RegenerateSkinHash();
            }
            else
            {
                throw new FileNotFoundException($"Skin '{this.Name}' doesn't have a file registered");
            }
        }

        public void SaveToDisk(string newFilePath = null)
        {
            if (newFilePath != null)
            {
                if (FileLocation != null)
                {
                    Logger.LogWarning($"Overriting existing path for texture {this.Name}: \n" +
                    $"\t - Old one: {this.FileLocation}\n"+
                    $"\t - New one: {newFilePath}");
                }
                this.FileLocation = newFilePath;
            }
            else if(FileLocation == null)
            {
                throw new FileNotFoundException($"Skin '{this.Name}' doesn't have a file registered and none was given");
            }
            File.WriteAllBytes(this.FileLocation, Texture.EncodeToPNG());
        }

        public bool IsForVariant(CharacterVariant characterVariant)
        {
            return VariantHelper.VariantMatch(characterVariant, this.ModelVariant);
        }

        public string GetSkinLabel()
        {
            StringBuilder sBuilder = new StringBuilder(Name);
            if (Author != null)
            {
                sBuilder.Append($" by {Author}");
            }
            return sBuilder.ToString();
        }

        public byte[] ToBytes()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((byte)this.Character);
                    binaryWriter.Write((byte)this.ModelVariant);
                    binaryWriter.Write(this.Author);
                    binaryWriter.Write(this.Name);
                    byte[] pngImage = Texture.EncodeToPNG();
                    binaryWriter.Write(pngImage.Length);
                    binaryWriter.Write(pngImage);
                }
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static CustomSkin FromBytes(byte[] bytes)
        {
            CustomSkin result = null;
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    Character character = (Character)binaryReader.ReadByte();
                    ModelVariant modelVariant = (ModelVariant)binaryReader.ReadByte();
                    string author = binaryReader.ReadString();
                    string name = binaryReader.ReadString();
                    int imageLength = binaryReader.ReadInt32();
                    byte[] pngImage = binaryReader.ReadBytes(imageLength); 
                    Texture2D texture = TextureHelper.NewDefaultTexture();
                    texture.name = name;
                    texture.LoadImage(pngImage);
                    result = new CustomSkin(character, modelVariant, name, author, texture);
                }
            }
            return result;
        }

        public Hash128 GenerateSkinHash()
        {
            byte[] imageData = Texture.GetRawTextureData();
            return Hash128.Compute(imageData.ToString() + this.Character.ToString() + this.ModelVariant.ToString());
        }

        public void RegenerateSkinHash()
        {
            this.SkinHash = GenerateSkinHash();
        }
    }

    public enum ModelVariant
    {
        None,
        Default,
        Alternative,
        DLC,
    }
}
