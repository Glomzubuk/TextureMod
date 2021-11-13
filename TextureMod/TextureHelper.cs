using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TextureMod
{
    class TextureHelper
    {
        public static Texture2D LoadPNG(string _path, string _fileName = "") //Loads a png from a file and returns it (Loads the asset into memory, do only load it once)
        {
            string fullPath = Path.Combine(_path, _fileName);
            if (!File.Exists(fullPath))
            {
                Debug.Log("Could not find " + fullPath);
                return null;
            }

            byte[] spriteBytes;
            using (var fileStream = File.OpenRead(fullPath))
            using (var reader = new BinaryReader(fileStream))
            {
                var png = PNGTools.ReadPNGFile(reader);
                for (int i = png.chunks.Count - 1; i >= 0; i--)
                {
                    var c = png.chunks[i];
                    if (c.type == EChunkType.gAMA ||
                        c.type == EChunkType.pHYs ||
                        c.type == EChunkType.sRBG)
                    {
                        png.chunks.RemoveAt(i);
                    }
                }
                using (var mem = new MemoryStream())
                {
                    PNGTools.WritePNGFile(png, new BinaryWriter(mem));
                    spriteBytes = mem.ToArray();
                }
            }

            string texName = Path.GetFileNameWithoutExtension(fullPath);


            Texture2D tex = NewDefaultTexture();
            tex.name = texName;
            tex.LoadImage(spriteBytes); //This resizes the texture width and height

            return tex;
        }

        public static Texture2D NewDefaultTexture(int width = 512, int height = 512)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, true, false)
            {
                filterMode = FilterMode.Trilinear,
                anisoLevel = 9,
                mipMapBias = -0.5f,
            };
        }

#if oldCode

        public static Texture2D ReloadSkin(Character _character, Texture2D _texture)
        {
            TextureLoader TL = TextureMod.Instance.tl;
            Texture2D newTex = null;

            string characterDirectory = "";
            string skinName = "";

            foreach (string path in TL.chars)
            {
                if (TL.StringToChar(Path.GetFileName(path)) == _character)
                {
                    characterDirectory = path;
                }
            }

            Debug.Log("characterDirectory:" + characterDirectory);

            foreach (var skin in TL.characterTextures[_character])
            {
                if (skin.Value == _texture)
                {
                    skinName = skin.Key;
                }
            }

            string oldSkinName = skinName;

            if (skinName.Contains(" by "))
            {
                int i = skinName.IndexOf(" by ");
                if (i >= 0) skinName = skinName.Remove(i);
            }

            foreach (string dir in Directory.GetDirectories(characterDirectory))
            {
                foreach (string file in Directory.GetFiles(dir))
                {

                    Debug.Log($"File: {file} : {skinName}");
                    if (Path.GetFileNameWithoutExtension(file) == skinName)
                    {
                        newTex = LoadPNG(file);
                        TL.characterTextures[_character][oldSkinName] = newTex;
                    }
                }
            }

            foreach (string file in Directory.GetFiles(characterDirectory))
            {
                Debug.Log($"File: {file} : {skinName}");
                if (Path.GetFileNameWithoutExtension(file) == skinName)
                {
                    newTex = LoadPNG(file);
                    TL.characterTextures[_character][skinName] = newTex;
                }
            }

            Resources.UnloadUnusedAssets();
            return newTex;
        }
#endif
    }

    public enum EChunkType : uint
    {
        IHDR = 0x49484452,
        sRBG = 0x73524742,
        gAMA = 0x67414D41,
        cHRM = 0x6348524D,
        pHYs = 0x70485973,
        IDAT = 0x49444154,
        IEND = 0x49454E44,
    }

    public class PNGFile
    {
        public static ulong m_Signature = 0x89504E470D0A1A0AU; // 
        public ulong Signature;
        public List<PNGChunk> chunks;
        public int FindChunk(EChunkType aType, int aStartIndex = 0)
        {
            if (chunks == null)
                return -1;
            for (int i = aStartIndex; i < chunks.Count; i++)
            {
                if (chunks[i].type == aType)
                    return i;
            }
            return -1;
        }
    }

    public class PNGChunk
    {
        public uint length;
        public EChunkType type;
        public byte[] data;
        public uint crc;
        public uint CalcCRC()
        {
            var crc = PNGTools.UpdateCRC(0xFFFFFFFF, (uint)type);
            crc = PNGTools.UpdateCRC(crc, data);
            return crc ^ 0xFFFFFFFF;
        }
    }

    public class PNGTools
    {
        static uint[] crc_table = new uint[256];
        static PNGTools()
        {
            for (int n = 0; n < 256; n++)
            {
                uint c = (uint)n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) > 0)
                        c = 0xedb88320 ^ (c >> 1);
                    else
                        c = c >> 1;
                }
                crc_table[n] = c;
            }
        }

        public static uint UpdateCRC(uint crc, byte aData)
        {
            return crc_table[(crc ^ aData) & 0xff] ^ (crc >> 8);
        }

        public static uint UpdateCRC(uint crc, uint aData)
        {
            crc = crc_table[(crc ^ ((aData >> 24) & 0xFF)) & 0xff] ^ (crc >> 8);
            crc = crc_table[(crc ^ ((aData >> 16) & 0xFF)) & 0xff] ^ (crc >> 8);
            crc = crc_table[(crc ^ ((aData >> 8) & 0xFF)) & 0xff] ^ (crc >> 8);
            crc = crc_table[(crc ^ (aData & 0xFF)) & 0xff] ^ (crc >> 8);
            return crc;
        }


        public static uint UpdateCRC(uint crc, byte[] buf)
        {
            for (int n = 0; n < buf.Length; n++)
                crc = crc_table[(crc ^ buf[n]) & 0xff] ^ (crc >> 8);
            return crc;
        }

        public static uint CalculateCRC(byte[] aBuf)
        {
            return UpdateCRC(0xffffffff, aBuf) ^ 0xffffffff;
        }
        public static List<PNGChunk> ReadChunks(BinaryReader aReader)
        {
            var res = new List<PNGChunk>();
            while (aReader.BaseStream.Position < aReader.BaseStream.Length - 4)
            {
                var chunk = new PNGChunk();
                chunk.length = BinaryReaderWriterExt.ReadUInt32BE(aReader);
                if (aReader.BaseStream.Position >= aReader.BaseStream.Length - 4 - chunk.length)
                    break;
                res.Add(chunk);
                chunk.type = (EChunkType)BinaryReaderWriterExt.ReadUInt32BE(aReader);
                chunk.data = aReader.ReadBytes((int)chunk.length);
                chunk.crc = BinaryReaderWriterExt.ReadUInt32BE(aReader);

                uint crc = chunk.CalcCRC();

                if ((chunk.crc ^ crc) != 0)
                    Debug.Log("Chunk CRC wrong. Got 0x" + chunk.crc.ToString("X8") + " expected 0x" + crc.ToString("X8"));
                if (chunk.type == EChunkType.IEND)
                    break;
            }
            return res;
        }

        public static PNGFile ReadPNGFile(BinaryReader aReader)
        {
            if (aReader == null || aReader.BaseStream.Position >= aReader.BaseStream.Length - 8)
                return null;
            var file = new PNGFile();
            file.Signature = BinaryReaderWriterExt.ReadUInt64BE(aReader);
            file.chunks = ReadChunks(aReader);
            return file;
        }
        public static void WritePNGFile(PNGFile aFile, BinaryWriter aWriter)
        {
            BinaryReaderWriterExt.WriteUInt64BE(aWriter,PNGFile.m_Signature);
            foreach (var chunk in aFile.chunks)
            {
                BinaryReaderWriterExt.WriteUInt32BE(aWriter,(uint)chunk.data.Length);
                BinaryReaderWriterExt.WriteUInt32BE(aWriter,(uint)chunk.type);
                aWriter.Write(chunk.data);
                BinaryReaderWriterExt.WriteUInt32BE(aWriter,chunk.crc);
            }
        }

        public static void SetPPM(PNGFile aFile, uint aXPPM, uint aYPPM)
        {
            int pos = aFile.FindChunk(EChunkType.pHYs);
            PNGChunk chunk;
            if (pos > 0)
            {
                chunk = aFile.chunks[pos];
                if (chunk.data == null || chunk.data.Length < 9)
                    throw new System.Exception("PNG: pHYs chunk data size is too small. It should be at least 9 bytes");
            }
            else
            {
                chunk = new PNGChunk();
                chunk.type = EChunkType.pHYs;
                chunk.length = 9;
                chunk.data = new byte[9];
                aFile.chunks.Insert(1, chunk);
            }
            var data = chunk.data;
            data[0] = (byte)((aXPPM >> 24) & 0xFF);
            data[1] = (byte)((aXPPM >> 16) & 0xFF);
            data[2] = (byte)((aXPPM >> 8) & 0xFF);
            data[3] = (byte)((aXPPM) & 0xFF);

            data[4] = (byte)((aYPPM >> 24) & 0xFF);
            data[5] = (byte)((aYPPM >> 16) & 0xFF);
            data[6] = (byte)((aYPPM >> 8) & 0xFF);
            data[7] = (byte)((aYPPM) & 0xFF);

            data[8] = 1;
            chunk.crc = chunk.CalcCRC();
        }

        public static byte[] ChangePPM(byte[] aPNGData, uint aXPPM, uint aYPPM)
        {
            PNGFile file;
            using (var stream = new MemoryStream(aPNGData))
            using (var reader = new BinaryReader(stream))
            {
                file = ReadPNGFile(reader);
            }
            SetPPM(file, aXPPM, aYPPM);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                WritePNGFile(file, writer);
                return stream.ToArray();
            }
        }
        public static byte[] ChangePPI(byte[] aPNGData, float aXPPI, float aYPPI)
        {
            return ChangePPM(aPNGData, (uint)(aXPPI * 39.3701f), (uint)(aYPPI * 39.3701f));
        }
    }

    public static class BinaryReaderWriterExt
    {
        public static uint ReadUInt32BE(BinaryReader aReader)
        {
            return ((uint)aReader.ReadByte() << 24) | ((uint)aReader.ReadByte() << 16)
                | ((uint)aReader.ReadByte() << 8) | ((uint)aReader.ReadByte());
        }

        public static ulong ReadUInt64BE(BinaryReader aReader)
        {
            return (ulong)ReadUInt32BE(aReader) << 32 | ReadUInt32BE(aReader);
        }

        public static void WriteUInt32BE(BinaryWriter aWriter, uint aValue)
        {
            aWriter.Write((byte)((aValue >> 24) & 0xFF));
            aWriter.Write((byte)((aValue >> 16) & 0xFF));
            aWriter.Write((byte)((aValue >> 8) & 0xFF));
            aWriter.Write((byte)((aValue) & 0xFF));
        }

        public static void WriteUInt64BE(BinaryWriter aWriter, ulong aValue)
        {
            WriteUInt32BE(aWriter,(uint)(aValue >> 32));
            WriteUInt32BE(aWriter, (uint)(aValue));
        }
    }
}