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

        [Serializable]
        public class CityObjectParam: CityObjectChildParam
        {

            [JsonProperty(Order = 5)]
            public List<CityObjectChildParam> children = new List<CityObjectChildParam>();
        }

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
            public string DebugString()
            {
                var sb = new StringBuilder();
                sb.Append($"gmlID : {gmlID}\n");
                sb.Append($"cityObjectIndex : [{cityObjectIndex[0]} , {cityObjectIndex[1]}]\n");
                sb.Append(
                    $"cityObjectType : {string.Join(", ", EnumUtil.EachFlags((CityObjectType)cityObjectType))}\n");

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
