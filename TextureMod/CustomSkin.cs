using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using BepInEx.Logging;
using LLBML.Texture;

namespace TextureMod
{
    public class CustomSkin
    {
        private static ManualLogSource Logger => TextureMod.Log;

        public SkinHash SkinHash { get; private set; }
        public Character Character { get; private set; }
        public ModelVariant ModelVariant { get; private set; }
        public CharacterVariant CharacterVariant => VariantHelper.GetDefaultVariantForModel(ModelVariant);
        public Texture2D Texture { get; private set; }
        public string Author { get; private set; }
        public string Name { get; private set; }

        public CustomSkin(Character _character, ModelVariant _variant, string _name, string _author, Texture2D texture)
        {
            Character = _character;
            ModelVariant = _variant;
            Author = _author;
            Texture = texture;
            Name = Texture.name = _name;
            this.RegenerateSkinHash();
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
                    binaryWriter.Write(this.Author ?? "Unknown");
                    binaryWriter.Write(this.Name ?? "Unknown");
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
                    Texture2D texture = TextureUtils.DefaultTexture();
                    texture.name = name;
                    texture.LoadImage(pngImage);
                    result = new CustomSkin(character, modelVariant, name, author, texture);
                }
            }
            return result;
        }

        public void SetTexture(Texture2D texture)
        {
            this.Texture = texture;
            this.RegenerateSkinHash();
        }

        public void RegenerateSkinHash()
        {
            this.SkinHash = new SkinHash(this.Texture, this.Character, this.ModelVariant);
        }

        public override string ToString()
        {
            return $"<CustomSkin: {Character} | {CharacterVariant} | {Name} | {Author} | {SkinHash.ToString()}>";
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
