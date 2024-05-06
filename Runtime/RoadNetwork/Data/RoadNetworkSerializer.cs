using Codice.Client.ChangeTrackerService;
using PLATEAU.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLATEAU.RoadNetwork.Data
{
    /// <summary>
    /// シリアライズするときに
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
            public Dictionary<MemberInfo, MemberInfo> Dst2SrcMemberTable { get; set; }


            public MemberReference(Type srcType, Type dstType)
            {
                SrcType = srcType;
                DstType = dstType;

                var flags = BindingFlags.Instance | BindingFlags.Public;
                Dst2SrcMemberTable
                    // #NOTE : Field or Propertyのみ対応
                    = DstType.GetProperties(flags).Concat(DstType.GetFields(flags).Cast<MemberInfo>())
                    // アトリビュート指定されている物を抽出
                    .Select(p => new { member = p, attr = p.GetCustomAttribute<RoadNetworkSerializeMemberAttribute>() })
                    .Where(p => p.attr != null)
                    .ToDictionary(p => p.member, p =>
                    {
                        var prop = srcType.GetProperty(p.attr.FieldName);
                        if (prop != null)
                            return (MemberInfo)prop;
                        var field = srcType.GetField(p.attr.FieldName);
                        if (field != null)
                            return field;
                        return null;
                    });
            }
        }

        private class MemberSerializer
        {
            public Dictionary<Type, MemberReference> MemberReferences { get; set; } =
                new Dictionary<Type, MemberReference>();

            public void Serialize(object src, object dst, ReferenceTable table)
            {

            }
        }


        private abstract class DataTable
        {
            public MemberReference MemberTable { get; }

            protected DataTable(MemberReference table)
            {
                MemberTable = table;
            }

            public abstract void Serialize(ReferenceTable table);

            public abstract object GetId(object s);
        }

        private class Table<TSrc, TData> : DataTable where TData : IPrimitiveData where TSrc : class
        {
            public Dictionary<TSrc, Tuple<TData, RnId<TData>>> DataTable { get; set; }

            public Table(MemberReference table, Dictionary<TSrc, Tuple<TData, RnId<TData>>> data) :
                base(table)
            {
                DataTable = data;
            }

            public override object GetId(object s)
            {
                var src = s as TSrc;
                if (src == null)
                {
                    return new RnId<TData>();
                }

                return DataTable[src].Item2;
            }


            public override void Serialize(ReferenceTable table)
            {
                foreach (var item in DataTable)
                {
                    var src = item.Key;
                    var dst = item.Value.Item1;

                    foreach (var m in MemberTable.Dst2SrcMemberTable)
                    {
                        var dstMemberInfo = m.Key;
                        var srcMemberInfo = m.Value; ;
                        var srcMemberType = TypeUtil.GetMemberType(srcMemberInfo);
                        var dstMemberType = TypeUtil.GetMemberType(dstMemberInfo);

                        var srcValue = TypeUtil.GetValue(srcMemberInfo, src);
                        if (srcMemberType == dstMemberType)
                        {
                            TypeUtil.SetValue(dstMemberInfo, dst, srcValue);
                        }
                        else if (table.IdConverter.ContainsKey(srcMemberType))
                        {
                            var dstValue = table.IdConverter[srcMemberType](srcValue);
                            TypeUtil.SetValue(dstMemberInfo, dst, dstValue);
                        }
                        // 配列
                        else if (srcMemberType.IsArray && dstMemberType.IsArray)
                        {
                            var srcArray = srcValue as Array;
                            var f = srcMemberInfo as FieldInfo;
                            //var obj = dstMemberType.Assembly.CreateInstance(dstMemberType.FullName, args: new[] { srcArray.Length }) as Array;

                        }
                        // List
                        else if (srcMemberType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var dstObj = dstMemberType.Assembly.CreateInstance(dstMemberType.FullName);
                            var addMethod = dstMemberType.GetMethod(nameof(List<int>.Add));
                            if (srcValue is IEnumerable enumerable)
                            {
                                foreach (var srcV in enumerable)
                                {
                                    var dstValue = table.IdConverter[srcMemberType.GenericTypeArguments[0]](srcV);
                                    addMethod.Invoke(dstObj, new[] { dstValue });
                                }
                            }
                            TypeUtil.SetValue(dstMemberInfo, dst, dstObj);
                        }
                        else
                        {
                            throw new NotSupportedException($"{srcMemberType.FullName} is not supported serialize");
                        }
                    }
                }
            }
        }

        private class ReferenceTable
        {
            public Dictionary<RoadNetworkDataPoint, RnId<RoadNetworkDataPoint>> PointTable { get; set; }

            // Key : 
            public Dictionary<Type, Func<object, object>> IdConverter { get; set; } = new Dictionary<Type, Func<object, object>>();

            public List<DataTable> DataList { get; } = new List<DataTable>();

            public void Add(DataTable table)
            {
                DataList.Add(table);
                IdConverter[table.MemberTable.SrcType] = table.GetId;
            }

            /// <summary>
            /// src -> dstへデータ変換
            /// </summary>
            public void Serialize()
            {
                foreach (var d in DataList)
                    d.Serialize(this);
            }
        }

        private Table<TSrc, TData> Collect<TSrc, TData>(RoadNetworkModel model, PrimitiveDataStorage.PrimitiveStorage<TData> storage)
            where TData : IPrimitiveData, new()
            where TSrc : class
        {
            // TSrcの型のインスタンスを全部探してくる
            var src = TypeUtil
                .GetAllMembersRecursively<TSrc>(model, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x => x.Item2)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            // 変換後のデータのnewだけ行う
            var datas = Enumerable.Range(0, src.Count).Select(x => new TData()).ToArray();

            var ids = storage.WriteNew(datas);
            var table = Enumerable.Range(0, ids.Length).ToDictionary(i => src[i], i => new Tuple<TData, RnId<TData>>(datas[i], ids[i]));
            return new Table<TSrc, TData>(new MemberReference(typeof(TSrc), typeof(TData)), table);
        }



        private void Serialize<TSrc, TData>(TSrc src, TData dst, ReferenceTable refTable, Table<TSrc, TData> table)
            where TData : class, IPrimitiveData, new()
            where TSrc : class
        {

        }

        public RoadNetworkStorage Serialize(RoadNetworkModel roadNetworkModel)
        {
            var ret = new RoadNetworkStorage();
            var refTable = new ReferenceTable();
            refTable.Add(Collect<RoadNetworkLink, RoadNetworkDataLink>(roadNetworkModel, ret.PrimitiveDataStorage.Links));
            refTable.Add(Collect<RoadNetworkLane, RoadNetworkDataLane>(roadNetworkModel, ret.PrimitiveDataStorage.Lanes));
            refTable.Add(Collect<RoadNetworkTrack, RoadNetworkDataTrack>(roadNetworkModel, ret.PrimitiveDataStorage.Tracks));
            refTable.Add(Collect<RoadNetworkBlock, RoadNetworkDataBlock>(roadNetworkModel, ret.PrimitiveDataStorage.Blocks));
            refTable.Add(Collect<RoadNetworkNode, RoadNetworkDataNode>(roadNetworkModel, ret.PrimitiveDataStorage.Nodes));
            refTable.Add(Collect<RoadNetworkWay, RoadNetworkDataWay>(roadNetworkModel, ret.PrimitiveDataStorage.Ways));

            var lineStrings =
                Collect<RoadNetworkLineString, RoadNetworkDataLineString>(roadNetworkModel,
                    ret.PrimitiveDataStorage.LineStrings);
            refTable.Add(lineStrings);


            refTable.Serialize();
            return ret;
        }


    }
}