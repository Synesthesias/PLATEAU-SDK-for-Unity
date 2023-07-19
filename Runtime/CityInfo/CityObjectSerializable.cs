using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityInfo.CityObject;
using static PLATEAU.CityInfo.CityObjectSerializable_CityObjectParamJsonConverter;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータです。
    /// </summary>
    [JsonConverter(typeof(CityObjectSerializableJsonConverter))]
    public class CityObject
    {
        public string parent = "";
        public List<CityObjectParam> cityObjects = new List<CityObjectParam>();

        [JsonConverter(typeof(CityObjectSerializable_CityObjectParamJsonConverter))]
        public class CityObjectParam
        {
            [JsonProperty][SerializeField] internal string gmlID = "";
            [JsonProperty][SerializeField] internal int[] cityObjectIndex = {-1, -1};
            [JsonProperty][SerializeField] internal ulong cityObjectType;
            [JsonProperty][SerializeField] internal List<CityObjectParam> children = new List<CityObjectParam>();
            [JsonProperty][SerializeField] internal Attributes attributesMap = new Attributes();

            // Getters/Setters
            [JsonIgnore] public string GmlID => gmlID;
            [JsonIgnore] public int[] CityObjectIndex => cityObjectIndex;
            [JsonIgnore] public CityObjectType CityObjectType => (CityObjectType)cityObjectType;
            [JsonIgnore] public Attributes AttributesMap => attributesMap;

            public CityObjectParam Init(string gmlIDArg, int[] cityObjectIndexArg, ulong cityObjectTypeArg, Attributes attributesMapArg )
            {
                gmlID = gmlIDArg;
                cityObjectIndex = cityObjectIndexArg;
                cityObjectType = cityObjectTypeArg;
                attributesMap = attributesMapArg;
                return this;
            }

            [JsonIgnore]
            public CityObjectType type => (CityObjectType)cityObjectType;
            [JsonIgnore]
            public CityObjectIndex IndexInMesh
            {
                get
                {
                    var idx = new CityObjectIndex();
                    if (cityObjectIndex.Length > 1)
                    {
                        idx.PrimaryIndex = cityObjectIndex[0];
                        idx.AtomicIndex = cityObjectIndex[1];
                    }
                    return idx;
                }
            }
            
            /// <summary>
            /// デバッグ用に自身の情報をstringで返します。
            /// </summary>
            public virtual string DebugString()
            {
                var sb = new StringBuilder();
                sb.Append($"GmlID : {GmlID}\n");
                sb.Append($"CityObjectIndex : [{CityObjectIndex[0]} , {CityObjectIndex[1]}]\n");
                sb.Append(
                    $"CityObjectType : {string.Join(", ", EnumUtil.EachFlags(CityObjectType))}\n");
                sb.Append($"Attributes:\n");
                sb.AppendLine(attributesMap.DebugString());

                return sb.ToString();
            }
        }

        /// <summary>
        /// シリアライズ可能なAttributeMapデータです。
        /// </summary>
        [JsonConverter(typeof(CityObjectSerializable_AttributesJsonConverter))]
        public class Attributes
        {
            private Dictionary<string, Value> attrMap = new Dictionary<string, Value>();

            public int Count => attrMap.Count;
            
            public Value this[string key] => attrMap[key];

            public IEnumerable<string> Keys => attrMap.Keys;

            public IEnumerable<Value> Values => attrMap.Values;

            public IEnumerator<KeyValuePair<string, Value>> GetEnumerator() { return this.attrMap.GetEnumerator(); }

            public bool TryGetValue(string key, out Value val)
            {
                return attrMap.TryGetValue(key, out val);
            }

            public void SetAttribute(string key, AttributeValue value)
            {
                attrMap.Add(key, new Value(value));
            }
            public void SetAttribute(string key, AttributeType type, object value)
            {
                attrMap.Add(key, new Value(type, value));
            }

            public string DebugString()
            {
                var sb = new StringBuilder();
                foreach (var pair in attrMap)
                {
                    sb.AppendLine($"key: {pair.Key}, value: {pair.Value.DebugString()}");
                }

                return sb.ToString();
            }

            public class Value
            {
                public AttributeType Type   { get; private set; }
                public string StringValue   { get; private set; }
                public int IntValue         { get; private set; }
                public double DoubleValue   { get; private set; }

                public CityObject.Attributes AttributesMapValue = new CityObject.Attributes();

                public Value(AttributeValue value)
                {
                    this.Type = value.Type;

                    if (value.Type == AttributeType.AttributeSet)
                    {
                        var map = value.AsAttrSet;
                        foreach (var attr in map)
                            AttributesMapValue.SetAttribute(attr.Key, attr.Value);
                    }
                    else
                    {
                        switch(value.Type)
                        {
                            case AttributeType.Integer:
                                IntValue = value.AsInt;
                                StringValue = IntValue.ToString();
                                break;
                            case AttributeType.Double:
                                DoubleValue = value.AsDouble;
                                StringValue = DoubleValue.ToString();
                                break;
                            default:
                                StringValue = value.AsString;
                                break;
                        }
                    }
                }

                public Value(AttributeType type, object value)
                {
                    this.Type = type;
                    switch (type)
                    {
                        case AttributeType.Integer:
                            IntValue = (int)value;
                            StringValue = IntValue.ToString();
                            break;
                        case AttributeType.Double:
                            DoubleValue = (Double)value;
                            StringValue = DoubleValue.ToString();
                            break;
                        case AttributeType.AttributeSet:
                            AttributesMapValue = value as Attributes;
                            break;
                        default:
                            StringValue = value as string;
                            break;
                    }
                }

                public string DebugString()
                {
                    var sb = new StringBuilder();
                    if (Type == AttributeType.AttributeSet)
                    {
                        sb.AppendLine("{");
                        foreach (var pair in AttributesMapValue)
                        {
                            sb.Append($"key: {pair.Key}, value: ");
                            sb.AppendLine(pair.Value.DebugString());
                        }

                        sb.Append("}");
                    }
                    else
                    {
                        sb.AppendLine(Convert.ToString(StringValue));
                    }

                    return sb.ToString();
                }
            }
        }
    }

    /// <summary>
    /// GML.CityObjectからシリアライズ可能なCityInfo.CityObjectデータを生成します
    /// </summary>
    internal static class CityObjectSerializableConvert
    {
        public static CityObjectParam FromCityGMLCityObject(CityGML.CityObject obj, CityObjectIndex? idx = null)
        {
            string gmlID = obj.ID;
            ulong cityObjectType = (ulong)obj.Type;
            int[] cityObjectIndex = { -1, -1 };
            CityObject.Attributes map = new Attributes();

            if( idx != null )
                cityObjectIndex = new[] { idx.Value.PrimaryIndex, idx.Value.AtomicIndex };
            foreach (var m in obj.AttributesMap)
            {
                map.SetAttribute(m.Key, m.Value);
            }

            var ret = new CityObjectParam();
            ret.Init(gmlID, cityObjectIndex, cityObjectType, map);
            return ret;
        }
    }
}
