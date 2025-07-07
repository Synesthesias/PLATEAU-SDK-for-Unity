using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAUCityObject = PLATEAU.CityGML.CityObject;

namespace PLATEAU.CityInfo
{

    /// <summary>
    /// <see cref="CityObjectList"/>のシリアライズとデシリアライズをします。
    /// バージョンに応じた場合分けにより後方互換性を保ちます。
    /// </summary>
    public static class CityObjectListSerializer
    {
        public const int CurrentSerializeVersion = 1; // シリアライズの方式を変える時はこの数字を増やしてください。

        /// <summary>
        /// MessagePack形式のシリアライズとデシリアライズに使うオプションです。
        ///
        /// MessagePack+LZ4圧縮を選定した理由：
        /// シリアライズとデシリアライズの変換処理にかかる時間よりも、データ量のほうがボトルネックになっています。
        /// データ量のせいで、動的タイルのInstantiateでコピー処理が重かったり、地域単位の地物の選択時に重くなったりします。
        /// そこでMessagePack+LZ4圧縮を採用してデータ量を下げます。
        /// </summary>
        private static readonly MessagePackSerializerOptions messagePackOption =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray); // LZ4圧縮
        
        /// <summary> バージョンに応じてデシリアライズします。 </summary>
        public static CityObjectList Deserialize(int serializeVersion, byte[] messagePack, string oldJson)
        {
            switch (serializeVersion)
            {
                // json文字列で保存していた時代（後方互換性に配慮）
                case 0: 
                    if (string.IsNullOrEmpty(oldJson)) return new CityObjectList();
                    return JsonConvert.DeserializeObject<CityObjectList>(oldJson);
                
                // MessagePackを導入
                case 1:
                    if (messagePack.Length == 0) return new CityObjectList();
                    try
                    {
                        return MessagePackSerializer.Deserialize<CityObjectList>(messagePack, messagePackOption);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"MessagePack deserialization failed: {ex.Message}", ex);
                    }
                default:
                    throw new Exception($"Unsupported serialization version. Supported version: up to {{CurrentSerializeVersion}}, received version: {{serializeVersion}}");
            }
        }

        /// <summary> シリアライズします。 </summary>
        public static byte[] Serialize(CityObjectList cityObjectList)
        {
            return MessagePackSerializer.Serialize(cityObjectList, messagePackOption);
        }
    }
    
    /// <summary>
    /// シリアライズ可能なCityObjectデータです。
    /// シリアライズ時はMessagePack形式にしますが、人間が読む用にjson形式でも出力可能です。
    /// </summary>
    [MessagePackObject]
    [JsonConverter(typeof(CityObjectSerializable_CityObjectListJsonConverter))]
    public partial class CityObjectList // partialはMessagePackの自動生成コードを利用するために必要です
    {
        // MessagePackのKey名は短い文字列で指定します。
        // Key名は短いほうが軽量化できますが、かといってキーがないとMessagePack保存時にMapではなく配列になってしまうため将来的に順番に変更があった場合に読めなくなってしまいます。
        [Key("p")]
        public string outsideParent = "";
        
        [Key("c")]
        public List<string> outsideChildren = new List<string>();
        
        [Key("o")]
        public List<CityObject> rootCityObjects = new ();

        public bool IsEmpty() => outsideParent == "" && outsideChildren.Count == 0 && rootCityObjects.Count == 0;

        [MessagePackObject(true, AllowPrivate = true)]
        [JsonConverter(typeof(CityObjectSerializable_CityObjectJsonConverter))]
        public partial class CityObject // // partialはMessagePackの自動生成コードを利用するために必要です
        {
            [Key("i")]
            private string gmlID = "";
            
            [Key("x")]
            private int[] cityObjectIndex = {-1, -1};
            
            [Key("t")]
            private ulong cityObjectType;
            
            [Key("c")]
            private List<CityObject> children = new List<CityObject>();
            
            [Key("a")]
            private Attributes attributesMap = new Attributes();

            [IgnoreMember]
            public string GmlID
            {
                get
                {
                    return gmlID;
                }
                set
                {
                    gmlID = value;
                }
            }

            [IgnoreMember]
            public int[] CityObjectIndex
            {
                get => cityObjectIndex;
                set => cityObjectIndex = value;
            }
            
            [IgnoreMember]
            public CityObjectType CityObjectType => (CityObjectType)cityObjectType;
            
            [IgnoreMember]
            public List<CityObject> Children => children;

            [IgnoreMember]
            public Attributes AttributesMap
            {
                get { return attributesMap; }
                set { attributesMap = value; }
            }

            public CityObject Init(string gmlIDArg, int[] cityObjectIndexArg, ulong cityObjectTypeArg, Attributes attributesMapArg, List<CityObject> childrenArg = null )
            {
                gmlID = gmlIDArg;
                cityObjectIndex = cityObjectIndexArg;
                cityObjectType = cityObjectTypeArg;
                attributesMap = attributesMapArg;
                if( childrenArg != null )
                    children = childrenArg;
                return this;
            }

            public CityObject CopyWithoutChildren()
            {
                var copy = new CityObject();
                copy.Init(gmlID, cityObjectIndex, cityObjectType, attributesMap);
                return copy;
            }

            [IgnoreMember]
            public CityObjectType type => (CityObjectType)cityObjectType;

            [IgnoreMember]
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
                sb.Append($"GmlID : {GmlID}\n");
                sb.Append($"CityObjectIndex : [{CityObjectIndex[0]} , {CityObjectIndex[1]}]\n");
                sb.Append(
                    $"CityObjectType : {string.Join(", ", EnumUtil.EachFlags(CityObjectType))}\n");
                sb.Append($"Attributes:\n");
                sb.AppendLine(attributesMap.DebugString(1));

                return sb.ToString();
            }
        }

        /// <summary>
        /// シリアライズ可能なAttributeMapデータです。
        /// </summary>
        [MessagePackObject(AllowPrivate = true)]
        [JsonConverter(typeof(CityObjectSerializable_CityObjectJsonConverter.CityObjectSerializable_AttributesJsonConverter))]
        public partial class Attributes // partialはMessagePackの自動生成コードを利用するために必要です
        {
            [Key("m")]
            private readonly Dictionary<string, Value> attrMap = new ();

            [IgnoreMember]
            public int Count => attrMap.Count;
            
            [IgnoreMember]
            public Value this[string key] => attrMap[key];

            [IgnoreMember]
            public IEnumerable<string> Keys => attrMap.Keys;

            [IgnoreMember]
            public IEnumerable<Value> Values => attrMap.Values;

            public IEnumerator<KeyValuePair<string, Value>> GetEnumerator() { return attrMap.GetEnumerator(); }
            
            public Attributes(){}

            public Attributes(Dictionary<string, Value> attrMap)
            {
                if (attrMap == null) throw new ArgumentNullException(nameof(attrMap));
                this.attrMap = attrMap;
            }
            
            /// <summary>
            /// 属性情報のキーバリューペアのうち、キーを指定してバリューを得ます。
            /// 成否をboolで返します。
            /// </summary>
            public bool TryGetValue(string key, out Value val)
            {
                return attrMap.TryGetValue(key, out val);
            }

            /// <summary>
            /// 属性情報のキーバリューペアのうち、キーを指定してバリューを得ます。
            /// キーにスラッシュ"/"を含む場合、属性情報が入れ子になっていると見なし、再帰的にキーを探索してバリューを取得します。
            /// 成否をboolで返します。
            /// </summary>
            public bool TryGetValueWithSlash(string keyWithSlash, out Value val)
            {
                var keys = keyWithSlash.Split("/");
                string firstKey = keys[0];
                // "/"がないならそのまま属性値を返します。
                if (keys.Length == 1)
                {
                    return TryGetValue(firstKey, out val);
                }

                // "/"があるなら、最初のキーで取得し、残りのキーで再帰します。
                if (TryGetValue(firstKey, out var firstValue))
                {
                    // 後続のキーがあるということは、最初のValueは入れ子(AttributeSet型)であるはずです。
                    // そうでない場合は、見つからなかったとします。
                    if (firstValue.Type == AttributeType.AttributeSet)
                    {
                        var nextKeys = keys.Skip(1);
                        var nextKeysWithSlash = string.Join("/", nextKeys);
                        return firstValue.AttributesMapValue.TryGetValueWithSlash(nextKeysWithSlash, out val);
                    }
                }

                // 見つからない場合
                val = null;
                return false;
            }

            public void AddAttribute(string key, NativeAttributeValue value)
            {
                attrMap.Add(key, new Value(value));
            }
            public void AddAttribute(string key, AttributeType type, object value)
            {
                attrMap.Add(key, new Value(type, value));
            }

            /// <summary>
            /// AttributesにAttributesを追加します。
            /// すでに存在するキーは無視されます。
            /// </summary>
            public void AddAttributes(Attributes attrs)
            {
                foreach (var attr in attrs)
                {
                    attrMap.TryAdd(attr.Key, attr.Value);
                }
            }

            public string DebugString(int indent)
            {
                var sb = new StringBuilder();
                foreach (var pair in attrMap)
                {
                    Indent(sb, indent + 1);
                    sb.AppendLine($"key: {pair.Key}, value: {pair.Value.DebugString(indent + 1)}");
                }

                return sb.ToString();
            }
            
            [MessagePackObject(true)]
            public partial class Value // partialはMessagePackの自動生成コードを利用するために必要です
            {
                [Key("t")]
                public AttributeType Type   { get; private set; }
                
                [Key("s")]
                public string StringValue   { get; private set; }
                
                [Key("i")]
                public int IntValue         { get; private set; }
                
                [Key("d")]
                public double DoubleValue   { get; private set; }

                [Key("m")]
                public Attributes AttributesMapValue = new Attributes();
                
                public Value(){}

                public Value(NativeAttributeValue value)
                {
                    Type = value.Type;

                    if (value.Type == AttributeType.AttributeSet)
                    {
                        var map = value.AsAttrSet;
                        foreach (var attr in map)
                            AttributesMapValue.AddAttribute(attr.Key, attr.Value);
                    }
                    else
                    {
                        switch(value.Type)
                        {
                            case AttributeType.Integer:
                                try
                                {
                                    IntValue = value.AsInt;
                                    StringValue = IntValue.ToString();
                                }
                                catch (OverflowException)
                                {
                                    // 沼津市のLOD3の道路でTypeがIntegerなのにintの範囲を超えることがあったので対応
                                    IntValue = 0;
                                    StringValue = value.AsString;
                                }

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
                    Type = type;
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

                public string DebugString(int indent)
                {
                    var sb = new StringBuilder();
                    if (Type == AttributeType.AttributeSet)
                    {
                        sb.AppendLine("{");
                        foreach (var pair in AttributesMapValue)
                        {
                            Indent(sb, indent + 1);
                            sb.Append($"key: {pair.Key}, value: ");
                            sb.AppendLine(pair.Value.DebugString(indent + 1));
                        }
                        Indent(sb, indent);
                        sb.Append("}");
                    }
                    else
                    {
                        sb.Append(Convert.ToString(StringValue));
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// <paramref name="indent"/> × 4個の半角スペースを <paramref name="sb"/> に追加します。
            /// </summary>
            private static void Indent(StringBuilder sb, int indent)
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append("    ");
                }
            }
        }
    }

    /// <summary>
    /// GML.CityObjectからシリアライズ可能なCityInfo.CityObjectデータを生成します
    /// </summary>
    internal static class CityObjectSerializableConvert
    {
        public static CityObjectList.CityObject FromCityGMLCityObject(PLATEAUCityObject obj, CityObjectIndex? idx = null)
        {
            string gmlID = obj.ID;
            ulong cityObjectType = (ulong)obj.Type;
            int[] cityObjectIndex = { -1, -1 };
            CityObjectList.Attributes map = new CityObjectList.Attributes();

            if( idx != null )
                cityObjectIndex = new[] { idx.Value.PrimaryIndex, idx.Value.AtomicIndex };
            foreach (var m in obj.NativeAttributesMap)
            {
                map.AddAttribute(m.Key, m.Value);
            }

            var ret = new CityObjectList.CityObject();
            ret.Init(gmlID, cityObjectIndex, cityObjectType, map);
            return ret;
        }
    }
}
