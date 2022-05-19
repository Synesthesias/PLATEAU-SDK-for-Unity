using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlateauUnitySDK.Runtime.SemanticsLoader
{
    // TODO 実装中
    public class IdToGmlFileTable : ScriptableObject, ISerializationCallbackReceiver//, IDictionary<string, string>
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<string> values = new List<string>();


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
