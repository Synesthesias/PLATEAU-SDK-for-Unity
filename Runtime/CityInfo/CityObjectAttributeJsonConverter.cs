using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PLATEAU.CityGML;
using System;
using System.Collections.Generic;
using static PLATEAU.CityInfo.SerializableCityObjectList;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// PLATEAU.CityInfo.CityObject用のJsonConverterです。
    /// </summary>
    internal class CityObjectSerializable_CityObjectListJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(SerializableCityObjectList);

        public override bool CanRead { get { return false; } }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var attr = value as SerializableCityObjectList;
            if (attr == null) return;

            writer.WriteStartObject();

            if (attr.rootCityObjects != null && attr.rootCityObjects.Count > 0)
            {
                writer.WritePropertyName("rootCityObjects");
                JToken.FromObject(attr.rootCityObjects).WriteTo(writer);
            }

            writer.WritePropertyName("outsideParent");
            if (!string.IsNullOrEmpty(attr.outsideParent))
                writer.WriteValue(attr.outsideParent);
            else
                writer.WriteValue("");

            if(attr.outsideChildren != null && attr.outsideChildren.Count > 0)
            {
                writer.WritePropertyName("outsideChildren");
                JToken.FromObject(attr.outsideChildren).WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// PLATEAU.CityInfo.CityObjectList.CityObject用のJsonConverterです。
    /// </summary>
    internal class CityObjectSerializable_CityObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(SerializableCityObject);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var param = new SerializableCityObject();
            
            JObject jObject = JObject.Load(reader);
            string gmlID = jObject["gmlID"]?.ToString();
            ulong cityObjectType = (ulong)Enum.Parse(typeof(CityObjectType), jObject["cityObjectType"].ToString());
            int[] cityObjectIndex = jObject["cityObjectIndex"]?.ToObject<int[]>();
            List<SerializableCityObject> children = jObject["children"]?.ToObject<List<SerializableCityObject>>();
            SerializableAttributes attributesMap = jObject["attributes"]?.ToObject<SerializableAttributes>() ?? new SerializableAttributes();

            param.Init(gmlID, cityObjectIndex, cityObjectType, attributesMap, children );
           
            return param;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var param = value as SerializableCityObject;
            if (param == null) return;

            writer.WriteStartObject();

            writer.WritePropertyName("gmlID");
            writer.WriteValue(param.GmlID);
            writer.WritePropertyName("cityObjectIndex");
            JToken.FromObject(param.CityObjectIndex).WriteTo(writer);
            writer.WritePropertyName("cityObjectType");
            writer.WriteValue(param.CityObjectType.ToString());

            if (param.Children != null && param.Children.Count > 0)
            {
                writer.WritePropertyName("children");
                JToken.FromObject(param.Children).WriteTo(writer);
            }
            if(param.AttributesMap.Count > 0)
            {
                writer.WritePropertyName("attributes");
                JToken.FromObject(param.AttributesMap).WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// PLATEAU.CityInfo.CityObject.Attributes用のJsonConverterです。
        /// </summary>
        internal class CityObjectSerializable_AttributesJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(SerializableAttributes);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var attr = new SerializableAttributes();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray) break;

                    JObject jObject = JObject.Load(reader);
                    string key = jObject["key"]?.ToString();
                    string type = jObject["type"]?.ToString();
                    if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(type))
                    {
                        if (type == AttributeType.Integer.ToString())
                            attr.AddAttribute(key, AttributeType.Integer, Int32.Parse(jObject["value"].ToString()));
                        else if (type == AttributeType.Double.ToString())
                            attr.AddAttribute(key, AttributeType.Double, Double.Parse(jObject["value"].ToString()));
                        else if (type == AttributeType.AttributeSet.ToString())
                            attr.AddAttribute(key, AttributeType.AttributeSet, jObject["value"].ToObject<SerializableAttributes>());
                        else
                            attr.AddAttribute(key, (AttributeType)Enum.Parse(typeof(AttributeType), type), jObject["value"].ToString());
                    }
                }
                return attr;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var attr = value as SerializableAttributes;
                if (attr == null) return;

                writer.WriteStartArray();

                foreach (var kv in attr)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("key");
                    writer.WriteValue(kv.Key);
                    writer.WritePropertyName("type");
                    writer.WriteValue(kv.Value.Type.ToString());
                    writer.WritePropertyName("value");
                    switch (kv.Value.Type)
                    {
                        case AttributeType.AttributeSet:
                            JToken.FromObject(kv.Value.AttributesMapValue).WriteTo(writer);
                            break;
                        case AttributeType.Integer:
                            writer.WriteValue(kv.Value.IntValue.ToString());
                            break;
                        case AttributeType.Double:
                            writer.WriteValue(kv.Value.DoubleValue.ToString());
                            break;
                        default:
                            writer.WriteValue(kv.Value.StringValue);
                            break;
                    }
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
        }
    }
}



