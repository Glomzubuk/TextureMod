using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using LLBML.Utils;

namespace TextureMod
{
    public class SkinHash : IEquatable<SkinHash>
    {
        public byte[] Bytes { get; private set; }
        private const int HashLength = 16;

        public SkinHash(byte[] bytes)
        {
            if (bytes.Length != HashLength) throw new NotSupportedException();
            this.Bytes = bytes;
        }
        public SkinHash(Texture2D tex, Character character, ModelVariant variant)
        {
            MD5 md5 = MD5.Create();
            byte[] rawData = tex.GetRawTextureData().Add((byte)character, (byte)variant);

            md5.ComputeHash(rawData);
            this.Bytes = md5.Hash;
            if (this.Bytes.Length != HashLength)
            {
                throw new NotSupportedException($"Computed hash length is {this.Bytes.Length} but supported hash length is {HashLength}");
            }
        }

        public SkinHash()
        {
            this.Bytes = new byte[HashLength];
        }

        public static SkinHash Compute(Texture2D tex, Character character, ModelVariant variant)
        {
            return new SkinHash(tex, character, variant);
        }

        public static explicit operator byte[](SkinHash a) => a.Bytes;
        public static explicit operator SkinHash(byte[] a) => new SkinHash(a);


        private static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is SkinHash)
            {
                SkinHash shs = (SkinHash)obj;
                return this.Equals(shs);
            }
            return false;
        }
        public bool Equals(byte[] other) => ByteArraysEqual(this.Bytes, other);
        public bool Equals(SkinHash other) => ByteArraysEqual(this.Bytes, other.Bytes);

        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        public static bool operator ==(SkinHash hash1, SkinHash hash2) => hash1.Equals(hash2);
        public static bool operator !=(SkinHash hash1, SkinHash hash2) => !(hash1 == hash2);
        public static bool operator ==(SkinHash hash1, byte[] hash2) => hash1.Equals(hash2);
        public static bool operator !=(SkinHash hash1, byte[] hash2) => !(hash1 == hash2);
        public static bool operator ==(byte[] hash1, SkinHash hash2) => hash2.Equals(hash1);
        public static bool operator !=(byte[] hash1, SkinHash hash2) => !(hash1 == hash2);

        public override string ToString()
        {
            return LLBML.Utils.StringUtils.BytesToHexString(Bytes);
        }
    }

}
