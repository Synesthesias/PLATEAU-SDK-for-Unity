using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityInfo.CityObject;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータです。
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(CityObjectSerializableJsonConverter))]
    public class CityObject
    {
        public string parent = "";
        public List<CityObjectParam> cityObjects = new List<CityObjectParam>();

        // TODO CityObjectChildParam と CityObjectParam の2つの異なる型にする必要があったのか？
        // TODO Childのほうが基底型であるというのも混乱する、継承の通常の使い方ではないように見える。この場合は継承ではなく親が子を持つというコンポジション関係にしたほうが自然。
        // TODO 親でも子でも children を持っていて、childrenが空である場合はjsonに含めないようにすればクラスを分ける必要はないのでは。
        // TODO または、「jsonにchildrenを表示しない」処理が面倒なら、json上で子が空のchildrenを持っていても問題ない。
        [Serializable]
        public class CityObjectParam: CityObjectChildParam
        {

            [JsonProperty(Order = 5)]
            public List<CityObjectChildParam> children = new List<CityObjectChildParam>();

            /// <summary>
            /// デバッグ用に自身の情報をstringで返します。
            /// </summary>
            public override string DebugString()
            {
                var sb = new StringBuilder();
                sb.Append(base.DebugString());
                sb.Append("children:\n");
                foreach (var child in children)
                {
                    sb.Append("\n\n");
                    sb.Append(child.DebugString());
                }

                return sb.ToString();
            }
        }

        // TODO このあたりのSerializable、本当に必要か？
        [Serializable]
        public class CityObjectChildParam
        {
            [JsonProperty] [SerializeField] private string gmlID = "";
            [JsonProperty] [SerializeField] private int[] cityObjectIndex = {-1, -1};
            [JsonProperty] [SerializeField] private ulong cityObjectType;
            [JsonProperty] [SerializeField] private List<Attribute> attributes = new();

            // Getters/Setters
            [JsonIgnore] public string GmlID => gmlID;
            
            [JsonIgnore] public int[] CityObjectIndex => cityObjectIndex;
            [JsonIgnore] public CityObjectType CityObjectType => (CityObjectType)cityObjectType;
            [JsonIgnore] public List<Attribute> Attributes => attributes;

            public CityObjectChildParam Init(string gmlIDArg, int[] cityObjectIndexArg, ulong cityObjectTypeArg,
                List<Attribute> attributesArg)
            {
                gmlID = gmlIDArg;
                cityObjectIndex = cityObjectIndexArg;
                cityObjectType = cityObjectTypeArg;
                attributes = attributesArg;
                return this;
            }
            
            // TODO AttributesMapにキーとキーバリューペアを保存しているのはキーがダブって無駄。AttributeMapを独自クラスとし、それ自体をDictionaryとして扱えるように対応すべき。
            // TODO CityObjectParam からのみAttributesがDictionary形式で扱えるようになるのは不自然。 Attributesの入れ子構造を手軽に扱うことができない。
            [JsonIgnore]
            public Dictionary<string, Attribute> AttributesMap
            {
                get
                {
                    var attrMap = new Dictionary<string, Attribute>();
                    foreach( var attr in attributes)
                    {
                        if (!attrMap.ContainsKey(attr.key))
                            attrMap.Add(attr.key, attr);
                    }
                    return attrMap;
                }
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
                foreach (var attr in Attributes)
                {
                    sb.Append(attr.DebugString());
                }
                return sb.ToString();
            }
        }

        [Serializable]
        [JsonConverter(typeof(CityObjectSerializable_AttributeJsonConverter))]
        public class Attribute
        {
            public string key = "";
            public string type = "";
            public dynamic value = "";

            public string DebugString()
            {
                var sb = new StringBuilder();
                sb.Append($"key: {key}, ");
                // TODO valueの値を見るためにこのような変換をするのはユーザーにとってわかりにくい。
                // TODO 属性情報を便利に見れるよう、AttributeMapという型を作るべき。
                if (value is List<Attribute> attrs)
                {
                    sb.AppendLine("value: {");
                    foreach (var attr in attrs)
                    {
                        sb.AppendLine(attr.DebugString());
                    }
                    
                    sb.Append("\n}\n");
                }
                else
                {
                    sb.Append("value: ");
                    sb.AppendLine(Convert.ToString(value));
                }

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// GML.CityObjectからシリアライズ可能なCityInfo.CityObjectデータを生成します
    /// </summary>
    internal static class CityObjectSerializableConvert
    {
        public static T FromCityGMLCityObject<T>(CityGML.CityObject  obj, CityObjectIndex? idx = null) where T : CityObjectChildParam, new()
        {
            string gmlID = obj.ID;
            ulong cityObjectType = (ulong)obj.Type;
            int[] cityObjectIndex = { -1, -1 };
            List<CityObject.Attribute> attributes = new List<CityObject.Attribute>();
            if( idx != null )
                cityObjectIndex = new[] { idx.Value.PrimaryIndex, idx.Value.AtomicIndex };
            foreach (var m in obj.AttributesMap)
            {
                CityObject.Attribute att = FromAttributesMap(m);
                attributes.Add(att);
            }

            var ret = new T();
            ret.Init(gmlID, cityObjectIndex, cityObjectType, attributes);
            return ret;
        }

        public static CityObject.Attribute FromAttributesMap(KeyValuePair<string, AttributeValue> map)
        {
            CityObject.Attribute attr = new CityObject.Attribute();
            attr.key = map.Key;
            attr.type = map.Value.Type.ToString();
            attr.value = AttributeValueByType(map.Value);
            return attr;
        }

        public static dynamic AttributeValueByType(AttributeValue val )
        {
            if(val.Type == AttributeType.AttributeSet)
            {
                List<CityObject.Attribute> set = new List<CityObject.Attribute>();
                AttributesMap map = val.AsAttrSet;
                foreach(var m in map)
                {
                    CityObject.Attribute attr = FromAttributesMap(m);
                    set.Add(attr);
                }
                return set;
            }
            return val.Type switch
            {
                AttributeType.Integer => val.AsInt,
                AttributeType.Double => val.AsDouble,     
                _ => val.AsString,
            };
        }
    }
}
