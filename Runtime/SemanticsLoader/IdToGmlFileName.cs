using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlateauUnitySDK.Runtime.SemanticsLoader
{
    // 実装中
    public class IdToGmlFileName : ScriptableObject, ISerializationCallbackReceiver
    {
        private Dictionary<string, string> dictionary;
        [SerializeField] private List<string> keys;
        [SerializeField] private List<string> values;


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
    }
}
