using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PLATEAU.CityGML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            if (attr.cityObjects !=null && attr.cityObjects.Count > 0)
            {
                writer.WritePropertyName("cityObjects");
                JToken.FromObject(attr.cityObjects).WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// PLATEAU.CityInfo.CityObject.Attribute用のJsonConverterです。
    /// </summary>
    internal class CityObjectSerializable_AttributeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(CityInfo.CityObject.Attribute);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var attr = new CityInfo.CityObject.Attribute();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = reader.Value?.ToString();
                    reader.Read();

                    switch (prop)
                    {
                        case "key":
                            attr.key = reader.Value.ToString();
                            break;
                        case "type":
                            attr.type = reader.Value.ToString();
                            break;
                        case "value":
                            if (attr.type == AttributeType.Integer.ToString())
                                attr.value = Int32.Parse(reader.Value.ToString());
                            else if (attr.type == AttributeType.Double.ToString())
                                attr.value = Double.Parse(reader.Value.ToString());
                            else if (attr.type == AttributeType.AttributeSet.ToString())
                                attr.value = new JsonSerializer().Deserialize<IList<CityInfo.CityObject.Attribute>>(reader);
                            else
                                attr.value = reader.Value;

                            break;
                    }
                }
            }
            return attr;
        }
        
        public override bool CanWrite { get { return false; } }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

