using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
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

            new public CityObjectParam Clone()
            {
                var param = new CityObjectParam();
                param.gmlID = gmlID;
                param.cityObjectIndex = cityObjectIndex;
                param.cityObjectType = cityObjectType;
                param.attributes = new List<Attribute>(attributes);
                param.children = new List<CityObjectChildParam>(children);
                return param;
            }
        }

        [Serializable]
        public class CityObjectChildParam
        {
            public string gmlID = "";
            public int[] cityObjectIndex = new int[0];
            public ulong cityObjectType;
            public List<Attribute> attributes = new List<Attribute>();

            /// <summary>
            /// Getters/Setters
            /// </summary>
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

            public CityObjectChildParam Clone()
            {
                var param = new CityObjectChildParam();
                param.gmlID = gmlID;
                param.cityObjectIndex = cityObjectIndex;
                param.cityObjectType = cityObjectType;
                param.attributes = new List<Attribute>(attributes);
                return param;
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
            T co = new T();
            co.gmlID = obj.ID;
            co.cityObjectType = (ulong)obj.Type;
            if( idx != null )
                co.cityObjectIndex = new int[] { idx.Value.PrimaryIndex, idx.Value.AtomicIndex };
            foreach (var m in obj.AttributesMap)
            {
                CityInfo.CityObject.Attribute att = FromAttributesMap(m);
                co.attributes.Add(att);
            }
            return co;
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

        public static T DeepCopy<T>(this T src) where T : class
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, src);
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }
    }
}
