
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// シリアライズするときに対応するクラスを指定する
    /// </summary>
    [AttributeUsageAttribute(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class RoadNetworkSerializeDataAttribute : Attribute
    {
        public Type DataType { get; set; }

        public RoadNetworkSerializeDataAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }

    [AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class RoadNetworkSerializeMemberAttribute : Attribute
    {
        public string FieldName { get; }

        public RoadNetworkSerializeMemberAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public RoadNetworkSerializeMemberAttribute()
        {
            FieldName = "";
        }
    }

    public class RoadNetworkSerializer
    {
        private class MemberReference
        {
            public Type SrcType { get; set; }

            public Type DstType { get; set; }

            // key : dst MemberInfo
            // value : src member info
            public Dictionary<FieldInfo, FieldInfo> Dst2SrcMemberTable { get; set; }

            private MemberReference(Type srcType, Type dstType, Dictionary<FieldInfo, FieldInfo> dst2SrcMemberTable)
            {
                SrcType = srcType;
                DstType = dstType;
                Dst2SrcMemberTable = dst2SrcMemberTable;
            }

            public static MemberReference Create(Type dstType)
            {
                var srcType = dstType.GetCustomAttribute<RoadNetworkSerializeDataAttribute>()?.DataType;
                if (srcType == null)
                    throw new ArgumentException(
                        $"{dstType?.Name} has no attribute {nameof(RoadNetworkSerializeDataAttribute)}");
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var dst2Src
                    // #NOTE : Field or Propertyのみ対応
                    = dstType.GetProperties(flags)
                        .Concat(dstType.GetFields(flags).Cast<MemberInfo>())
                        // アトリビュート指定されている物を抽出
                        .Select(p => new { member = p, attr = p.GetCustomAttribute<RoadNetworkSerializeMemberAttribute>() })
                        .Where(p => p.attr != null)
                        .ToDictionary(m =>
                        {
                            if (m.member is PropertyInfo p)
                                return TypeUtil.GetPropertyBackingField(dstType, p);
                            return m.member as FieldInfo;
                        }, p =>
                        {
                            var fieldName = string.IsNullOrEmpty(p.attr.FieldName) ? p.member.Name : p.attr.FieldName;
                            var prop = srcType.GetProperty(fieldName, flags);
                            if (prop != null)
                            {
                                var ret = TypeUtil.GetPropertyBackingField(srcType, prop);
                                if (ret == null)
                                    throw new InvalidDataException($"Property {prop.Name} has no field info");
                                return ret;
                            }
                            var field = srcType.GetField(fieldName, flags);
                            if (field != null)
                                return field;
                            return null;
                        });
                var nullFields = dst2Src.Where(x => x.Value is null).ToList();
                if (nullFields.Any())
                    throw new InvalidDataException(
                        $"'{dstType.Name}'の[{string.Join(',', nullFields.Select(x => x.Key))}]に対応するフィールドが{srcType.Name}にありません");

                return new MemberReference(srcType, dstType, dst2Src);
            }

            public MemberReference GetReversed()
            {
                return new MemberReference(DstType, SrcType, Dst2SrcMemberTable.ToDictionary(i => i.Value, i => i.Key));
            }
        }

        private interface IValueConverter
        {
            object Convert(object val);
        }

        private class Object2RnIdConverter<TData> : IValueConverter
            where TData : IPrimitiveData
        {
            private Dictionary<object, RnID<TData>> Table { get; }

            public Object2RnIdConverter(Dictionary<object, RnID<TData>> table)
            {
                Table = table;
            }

            public object Convert(object val)
            {
                if (val == null)
                {
                    return RnID<TData>.Undefind;
                }

                return Table[val];
            }
        }

        /// <summary>
        /// RnIdから元のデータを引っ張ってくる
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        private class RnId2ObjectConverter<TData> : IValueConverter
            where TData : IPrimitiveData
        {
            private Dictionary<RnID<TData>, object> Table { get; }

            public RnId2ObjectConverter(Dictionary<RnID<TData>, object> table)
            {
                Table = table;
            }

            public object Convert(object val)
            {
                if (val is RnID<TData> id)
                {
                    if (id.IsValid == false)
                        return null;
                    return Table[id];
                }

                return null;
            }
        }

        private interface IDataStorage
        {
            void ConvertAll(IDataConverter converter);
        }

        private class DataStorage : IDataStorage
        {
            private Dictionary<object, object> DataTable { get; set; }

            public DataStorage(Dictionary<object, object> dataTable)
            {
                DataTable = dataTable;
            }

            public void ConvertAll(IDataConverter converter)
            {
                foreach (var item in DataTable)
                {
                    Convert(converter, item.Value.GetType(), item.Key, item.Value);
                }
            }
        }


        private interface IDataConverter
        {
            /// <summary>
            /// typeがコンバート可能かどうか
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            bool Contains(Type type);

            // 
            /// <summary>
            /// コンバート. srcはtype型である必要がある.
            /// srcがnullの事を考えてtypeも別途渡す
            /// </summary>
            /// <param name="type"></param>
            /// <param name="src"></param>
            /// <returns></returns>
            object Convert(Type type, object src);

            /// <summary>
            /// dstTypeのコンバート対象のテーブルを持ってくる
            /// </summary>
            /// <param name="dstType"></param>
            /// <returns></returns>
            MemberReference GetOrCreateMemberReference(Type dstType);
        }

        private class ReferenceTable : IDataConverter
        {
            private Dictionary<Type, IValueConverter> IdConverter { get; } = new();

            private List<DataStorage> Storage { get; } = new List<DataStorage>();

            /// <summary>
            /// Key : DstType
            /// Value : 対応するMemberReference
            /// </summary>
            private Dictionary<Type, MemberReference> MemberReferences { get; } =
                new Dictionary<Type, MemberReference>();

            /// <summary>
            /// key : dstType
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public MemberReference GetOrCreateMemberReference(Type key)
            {
                return CreateMemberReferenceOrSkip(key);
            }

            public MemberReference CreateMemberReferenceOrSkip(Type key)
            {
                if (MemberReferences.TryGetValue(key, out var ret))
                    return ret;
                var val = MemberReference.Create(key);
                MemberReferences[key] = val;
                MemberReferences[val.SrcType] = val.GetReversed();
                return MemberReferences[key];
            }

            public void AddStorage(DataStorage dataStorage)
            {
                Storage.Add(dataStorage);
            }

            public void AddConverter(Type srcType, IValueConverter valueConverter)
            {
                IdConverter[srcType] = valueConverter;
            }

            /// <summary>
            /// src -> dstへデータ変換
            /// </summary>
            public void ConvertAll()
            {
                foreach (var d in Storage)
                    d.ConvertAll(this);
            }

            public bool Contains(Type type)
            {
                return IdConverter.ContainsKey(type);
            }

            public object Convert(Type type, object src)
            {
                var idConverter = IdConverter[type];
                return idConverter.Convert(src);
            }
        }

        private static void Convert(IDataConverter converter, Type dstType, object src, object dst)
        {
            var memberReference = converter.GetOrCreateMemberReference(dstType);
            foreach (var m in memberReference.Dst2SrcMemberTable)
            {
                var dstMemberInfo = m.Key;
                var srcMemberInfo = m.Value; ;
                var srcMemberType = TypeUtil.GetMemberType(srcMemberInfo);
                var dstMemberType = TypeUtil.GetMemberType(dstMemberInfo);

                var srcValue = TypeUtil.GetValue(srcMemberInfo, src);
                if (srcValue != null)
                    srcMemberType = srcValue.GetType();
                if (srcMemberType == dstMemberType)
                {
                    TypeUtil.SetValue(dstMemberInfo, dst, srcValue);
                }
                else if (converter.Contains(srcMemberType))
                {
                    var dstValue = converter.Convert(srcMemberType, srcValue);
                    TypeUtil.SetValue(dstMemberInfo, dst, dstValue);
                }
                // #TODO
                // 配列は一旦サポート外
                else if (srcMemberType.IsArray && dstMemberType.IsArray)
                {
                    throw new NotSupportedException($"{srcMemberType.FullName} is not supported serialize");
                }
                // List
                else if (srcMemberType.IsGenericType && srcMemberType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var dstObj = dstMemberType.Assembly.CreateInstance(dstMemberType.FullName);
                    var addMethod = dstMemberType.GetMethod(nameof(List<int>.Add));
                    if (srcValue is IEnumerable enumerable)
                    {
                        foreach (var srcV in enumerable)
                        {
                            var dstValType = dstMemberType.GenericTypeArguments[0];
                            var srcValType = srcMemberType.GenericTypeArguments[0];
                            if (converter.Contains(srcValType))
                            {
                                var dstV = converter.Convert(srcValType, srcV);
                                addMethod.Invoke(dstObj, new[] { dstV });
                            }
                            else
                            {
                                var dstV = dstValType.Assembly.CreateInstance(dstValType.FullName);
                                Convert(converter, dstValType, srcV, dstV);
                                addMethod.Invoke(dstObj, new[] { dstV });
                            }
                        }
                    }
                    TypeUtil.SetValue(dstMemberInfo, dst, dstObj);

                }
                else
                {
                    throw new NotSupportedException($"{src.GetType().Name}/{srcMemberInfo.Name}/{srcMemberType.FullName} is not supported serialize");
                }
            }
        }

        private void CollectForSerialize<TData>(ReferenceTable refTable, RnModel model, PrimitiveDataStorage.PrimitiveStorage<TData> storage)
            where TData : class, IPrimitiveData, new()
        {
            var memberReference = refTable.GetOrCreateMemberReference(typeof(TData));

            // TSrcの型のインスタンスを全部探してくる
            var src = TypeUtil
                .GetAllMembersRecursively(model, memberReference.SrcType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x => x.Item2)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            // 変換後のデータのnewだけ行う
            var dataList = src.Select(x =>
            {
                var mf = refTable.GetOrCreateMemberReference(x.GetType());
                return mf.SrcType.Assembly.CreateInstance(mf.SrcType.FullName) as TData;
            }).ToArray();

            var ids = storage.WriteNew(dataList);

            var obj2Id = Enumerable.Range(0, ids.Length)
                .ToDictionary(i => src[i], i => ids[i]);

            var valueConverter =
                new Object2RnIdConverter<TData>(obj2Id);

            var obj2Data = Enumerable.Range(0, src.Count)
                .ToDictionary(i => src[i], i => dataList[i] as object);

            var dataStorage = new DataStorage(obj2Data);
            refTable.AddStorage(dataStorage);
            refTable.AddConverter(memberReference.SrcType, valueConverter);
            // SrcTypeの子クラスの参照も同じ
            foreach (var childType in memberReference.SrcType.Assembly.GetTypes().Where(t => memberReference.SrcType.IsAssignableFrom(t)))
                refTable.AddConverter(childType, valueConverter);
        }

        private static ReferenceTable CreateReferenceTable()
        {
            var refTable = new ReferenceTable();
            refTable.CreateMemberReferenceOrSkip(typeof(RnDataNeighbor));
            refTable.CreateMemberReferenceOrSkip(typeof(RnDataIntersection));
            refTable.CreateMemberReferenceOrSkip(typeof(RnDataRoad));
            return refTable;
        }

        public RoadNetworkStorage Serialize(RnModel roadNetworkModel)
        {

            var ret = new RoadNetworkStorage();
            var refTable = CreateReferenceTable();
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.Points);
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.LineStrings);
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.Lanes);
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.Blocks);
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.RoadBases);
            CollectForSerialize(refTable, roadNetworkModel, ret.PrimitiveDataStorage.Ways);

            refTable.ConvertAll();
            return ret;
        }



        private List<T> CollectForDeserialize<TData, T>(ReferenceTable refTable, PrimitiveDataStorage.PrimitiveStorage<TData> storage)
            where TData : IPrimitiveData
            where T : class, new()
        {
            refTable.GetOrCreateMemberReference(typeof(TData));
            // 先にデータを作成する
            var objList = storage.DataList.Select(x =>
            {
                var mf = refTable.GetOrCreateMemberReference(x.GetType());
                return mf.SrcType.Assembly.CreateInstance(mf.SrcType.FullName) as T;
            }).ToList();

            var id2Obj = Enumerable.Range(0, storage.DataList.Count)
                .ToDictionary(i => storage.RequsetID(i), i => objList[i] as object);

            var idConverter = new RnId2ObjectConverter<TData>(id2Obj);

            var data2Obj = Enumerable.Range(0, storage.DataList.Count)
                .ToDictionary(i => storage.DataList[i] as object, i => objList[i] as object);
            var dataStorage = new DataStorage(data2Obj);
            refTable.AddStorage(dataStorage);
            refTable.AddConverter(typeof(RnID<TData>), idConverter);
            return objList;
        }

        public RnModel Deserialize(RoadNetworkStorage roadNetworkStorage)
        {
            var refTable = CreateReferenceTable();
            var points = CollectForDeserialize<RnDataPoint, RnPoint>(refTable, roadNetworkStorage.PrimitiveDataStorage.Points);
            var lineStrings = CollectForDeserialize<RnDataLineString, RnLineString>(refTable, roadNetworkStorage.PrimitiveDataStorage.LineStrings);
            var blocks = CollectForDeserialize<RnDataBlock, RnBlock>(refTable, roadNetworkStorage.PrimitiveDataStorage.Blocks);
            var ways = CollectForDeserialize<RnDataWay, RnWay>(refTable, roadNetworkStorage.PrimitiveDataStorage.Ways);
            var lanes = CollectForDeserialize<RnDataLane, RnLane>(refTable, roadNetworkStorage.PrimitiveDataStorage.Lanes);
            var roadBases = CollectForDeserialize<RnDataRoadBase, RnRoadBase>(refTable, roadNetworkStorage.PrimitiveDataStorage.RoadBases);

            refTable.ConvertAll();
            var ret = new RnModel();
            foreach (var r in roadBases)
            {
                if (r is RnIntersection n)
                    ret.AddNode(n);
                else if (r is RnRoad l)
                    ret.AddLink(l);
            }
            return ret;
        }
    }
}