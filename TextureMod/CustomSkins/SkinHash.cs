using System;
using System.Security.Cryptography;
using UnityEngine;
using LLBML.Math;
using LLBML.Utils;

namespace TextureMod.CustomSkins
{
    public class SkinHash : Hash<CustomSkin>
    {
        static SkinHash()
        {
            HashLength = 16;
        }
        public SkinHash(byte[] data) : base(data) { }
        public SkinHash(Texture2D tex, Character character, ModelVariant variant):base(Compute(tex, character, variant)) { }
        public SkinHash(CustomSkin skin) : base(Compute(skin.Texture, skin.Character, skin.ModelVariant)) { }

        private static byte[] Compute(Texture2D tex, Character character, ModelVariant variant)
        {
            MD5 md5 = MD5.Create();
            byte[] rawData = tex.GetRawTextureData().Add((byte)character, (byte)variant);

            md5.ComputeHash(rawData);
            var bytes = md5.Hash;
            if (bytes.Length != HashLength)
            {
                throw new NotSupportedException($"Computed hash length is {bytes.Length} but supported hash length is {HashLength}");
            }
            return bytes;
        }

        public static explicit operator SkinHash(byte[] a) => new SkinHash(a);

        public override bool Equals(CustomSkin other) => BinaryUtils.ByteArraysEqual(this.Bytes, Compute(other.Texture,other.Character, other.ModelVariant));
    }

}
