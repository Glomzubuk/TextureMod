using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TextureMod
{
    public class SkinCache
    {
        public Dictionary<Hash128, CustomSkin> Cache { get; protected set; }
        public string rootPath;

        public SkinCache(string rootPath)
        {
            this.rootPath = rootPath;
            this.Cache = new Dictionary<Hash128, CustomSkin>();
        }

        public CustomSkin this[Hash128 key] { get => Cache[key]; set => Cache[key] = value; }

        public ICollection<Hash128> Hashes => Cache.Keys;

        public ICollection<CustomSkin> Skins => Cache.Values;

        public int SkinCount => Cache.Count;

        public void AddSkin(CustomSkin customSkin) => Cache.Add(customSkin.SkinHash, customSkin);
        public bool ContainsHash(Hash128 skinHash) => Cache.ContainsKey(skinHash);
        public bool ContainsSkin(CustomSkin customSkin) => Cache.ContainsKey(customSkin.SkinHash);


        public IEnumerator<KeyValuePair<Hash128, CustomSkin>> GetEnumerator() => Cache.GetEnumerator();

        public bool Remove(Hash128 skinHash) => Cache.Remove(skinHash);
        public bool Remove(CustomSkin customSkin) => Cache.Remove(customSkin.SkinHash);
        public void Clear() => Cache.Clear();

        public bool TryGetValue(Hash128 key, out CustomSkin value) => Cache.TryGetValue(key, out value);
    }

    public class SkinCachesHandler
    {
        public SkinCache Local { get; private set; }
        public SkinCache Distant { get; private set; }
        private Dictionary<Hash128, CustomSkin> All => Local.Cache.Concat(Distant.Cache).ToDictionary(x => x.Key, x => x.Value);
        public CustomSkin this[Hash128 key] {
            get
            {
                if (Local.ContainsHash(key))
                    return Local[key];
                if (Distant.ContainsHash(key))
                    return Distant[key];
                throw new KeyNotFoundException("Key \""+ key + "\" does not exists in either cache.");
            }
        }


        public bool ContainsHash(Hash128 skinHash) {
            return Local.ContainsHash(skinHash) || Distant.ContainsHash(skinHash);
        }
        public bool ContainsSkin(CustomSkin customSkin) => ContainsHash(customSkin.SkinHash);

        public IEnumerator GetEnumerator()
        {
            return All.GetEnumerator();
        }

        public void AddLocalSkin(CustomSkin customSkin)
        {
            if (Distant.ContainsSkin(customSkin)) throw new ArgumentException($"An element with the key '{customSkin.SkinHash}' already exists in the remote cache.");
            Local.AddSkin(customSkin);
        }
        public void AddDistantSkin(CustomSkin customSkin)
        {
            if (Distant.ContainsSkin(customSkin)) throw new ArgumentException($"An element with the key '{customSkin.SkinHash}' already exists in the local cache.");
            Distant.AddSkin(customSkin);
        }

        public void ClearAll()
        {
            Local.Clear();
            Distant.Clear();
        }
    }
}
