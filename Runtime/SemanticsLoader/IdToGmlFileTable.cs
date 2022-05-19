using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.SemanticsLoader
{
    /// <summary>
    /// <see cref="LibPLATEAU.NET.CityGML.CityObject"/> のIDから、対応するGMLのファイル名を検索できる辞書データを格納した
    /// <see cref="ScriptableObject"/> です。
    /// </summary>
    public class IdToGmlFileTable : ScriptableObject, ISerializationCallbackReceiver//, IDictionary<string, string>
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();
        // Unityの仕様上、シリアライズするときは List 形式で行い、 デシリアライズするときは Dictionary 形式に直します。
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<string> values = new List<string>();

        /// <summary>
        /// シリアライズするときに List形式に直します。
        /// </summary>
        public void OnBeforeSerialize()
        {
            this.keys.Clear();
            this.values.Clear();
            foreach (var pair in this.dictionary)
            {
                this.keys.Add(pair.Key);
                this.values.Add(pair.Value);
            }
        }

        /// <summary>
        /// デシリアライズするときに List から Dictionary 形式に直します。
        /// </summary>
        public void OnAfterDeserialize()
        {
            this.dictionary = new Dictionary<string, string>();
            int cnt = Math.Min(this.keys.Count, this.values.Count);
            for (int i = 0; i < cnt; i++)
            {
                this.dictionary.Add(this.keys[i], this.values[i]);
            }
        }

        public void Add(string id, string gmlFileName)
        {
            Add(new KeyValuePair<string, string>(id, gmlFileName));    
        }

        public void Add(KeyValuePair<string, string> item)
        {
            this.dictionary.Add(item.Key, item.Value);
        }

        public bool ContainsKey(string key) => this.dictionary.ContainsKey(key);
    }
}
