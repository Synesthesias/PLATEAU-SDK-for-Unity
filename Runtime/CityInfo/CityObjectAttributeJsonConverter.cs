using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PLATEAU.CityGML;
using System;
using System.Collections.Generic;
using static PLATEAU.CityInfo.CityObject;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// PLATEAU.CityInfo.CityObject用のJsonConverterです。
    /// </summary>
    internal class CityObjectSerializableJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(CityInfo.CityObject);

        public override bool CanRead { get { return false; } }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var attr = value as CityObject;
            if (attr == null) return;

            writer.WriteStartObject();

            writer.WritePropertyName("parent");
            if (!string.IsNullOrEmpty(attr.parent))
                writer.WriteValue(attr.parent);
            else
                writer.WriteValue("");

            if (attr.cityObjects != null && attr.cityObjects.Count > 0)
            {
                writer.WritePropertyName("cityObjects");
                JToken.FromObject(attr.cityObjects).WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// PLATEAU.CityInfo.CityObject.CityObjectParam用のJsonConverterです。
    /// </summary>
    internal class CityObjectSerializable_CityObjectParamJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(CityInfo.CityObject.CityObjectParam);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var param = new CityInfo.CityObject.CityObjectParam();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = reader.Value?.ToString();
                    reader.Read();

                    switch (prop)
                    {
                        case "gmlID":
                            param.gmlID = reader.Value.ToString();
                            break;
                        case "cityObjectIndex":
                            param.cityObjectIndex = new JsonSerializer().Deserialize<int[]>(reader);
                            break;
                        case "cityObjectType":
                            param.cityObjectType = Convert.ToUInt64(reader.Value);
                            break;
                        case "children":
                            param.children = new JsonSerializer().Deserialize<List<CityObjectParam>>(reader);
                            break;
                        case "attributes":
                            param.attributesMap = new JsonSerializer().Deserialize<Attributes>(reader);
                            break;
                    }
                }
            }
            return param;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var param = value as CityObjectParam;
            if (param == null) return;

            writer.WriteStartObject();

            writer.WritePropertyName("gmlID");
            writer.WriteValue(param.gmlID);
            writer.WritePropertyName("cityObjectIndex");
            JToken.FromObject(param.cityObjectIndex).WriteTo(writer);
            writer.WritePropertyName("cityObjectType");
            writer.WriteValue(param.cityObjectType.ToString());
            if (param.children != null && param.children.Count > 0)
            {
                writer.WritePropertyName("children");
                JToken.FromObject(param.children).WriteTo(writer);
            }
            if(param.attributesMap.Count > 0)
            {
                writer.WritePropertyName("attributes");
                JToken.FromObject(param.attributesMap).WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// PLATEAU.CityInfo.CityObject.Attributes用のJsonConverterです。
        /// </summary>
        internal class CityObjectSerializable_AttributesJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(CityInfo.CityObject.Attributes);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var attr = new CityInfo.CityObject.Attributes();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray) break;

                    JObject jObject = JObject.Load(reader);
                    string key = jObject["key"]?.ToString();
                    string type = jObject["type"]?.ToString();
                    if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(type))
                    {
                        if (type == AttributeType.Integer.ToString())
                            attr.SetAttribute(key, AttributeType.Integer, Int32.Parse(jObject["value"].ToString()));
                        else if (type == AttributeType.Double.ToString())
                            attr.SetAttribute(key, AttributeType.Double, Double.Parse(jObject["value"].ToString()));
                        else if (type == AttributeType.AttributeSet.ToString())
                            attr.SetAttribute(key, AttributeType.AttributeSet, jObject["value"].ToObject<Attributes>());
                        else
                            attr.SetAttribute(key, (AttributeType)Enum.Parse(typeof(AttributeType), type), jObject["value"].ToString());
                    }
                }
                return attr;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var attr = value as CityInfo.CityObject.Attributes;
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



