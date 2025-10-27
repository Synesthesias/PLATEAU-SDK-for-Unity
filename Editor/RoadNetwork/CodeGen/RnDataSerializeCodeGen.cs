using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace PLATEAU.Editor.RoadNetwork.CodeGen
{
    public class RnDataSerializeCodeGen
    {
        /// <summary>
        /// ID化するために集めるタイプリスト
        /// </summary>
        public static List<Type> CollectDataTypes { get; } = new List<Type>
        {
            typeof(TrafficSignalLightController),
            typeof(TrafficSignalLight),
            typeof(TrafficSignalControllerPattern),
            typeof(TrafficSignalControllerPhase),
            typeof(RnPoint),
            typeof(RnLineString),
            typeof(RnLane),
            typeof(RnRoadBase),
            typeof(RnWay),
            typeof(RnSideWalk),
        };

        /// <summary>
        /// シリアライズ情報
        /// </summary>
        private class SerializeInfo
        {
            /// <summary>
            /// フィールド情報
            /// </summary>
            public List<FieldInfo> Fields { get; } = new List<FieldInfo>();
        }

        private class Work
        {
            /// <summary>
            /// 探索済み
            /// </summary>
            public HashSet<Type> Visited { get; } = new();

            /// <summary>
            /// Keyが存在する場合. 自分自身もしくはフィールドにCollect対象のデータを持っている
            /// key   : シリアライズの探索対象のタイプ
            /// value : そのフィールド情報
            /// </summary>
            public Dictionary<Type, SerializeInfo> SerializeInfos { get; } = new Dictionary<Type, SerializeInfo>();

            /// <summary>
            /// 検索対象のタイプ
            /// </summary>
            public HashSet<Type> CollectTargetTypes { get; } = new();

            /// <summary>
            /// key   : Type
            /// value : keyの子クラス
            /// ただし、同じAssemblyの物のみ
            /// </summary>
            public Dictionary<Type, HashSet<Type>> ChildTypes { get; } = new Dictionary<Type, HashSet<Type>>();

            /// <summary>
            /// チェック不要のタイプ. (この要素が対象外であることが保証されている
            /// </summary>
            public string TargetNameSpace { get; set; }

            public bool IsTarget(Type type)
            {
                return CollectTargetTypes.Contains(type);
            }

            /// <summary>
            /// 探索を無視していいタイプ
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public bool IsIgnore(Type type)
            {
                if (string.IsNullOrEmpty(TargetNameSpace) == false
                    && (type.Namespace?.StartsWith(TargetNameSpace) ?? false) == false)
                {
                    return true;
                }

                // 単純型は対象外
                if (TypeUtil.IsSimpleType(type))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Genericをすべて外した型を返す.
        /// List&lt;&lt;T&gt;List&gt;のような型の場合、Tの型を取得する.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type RemoveGenericType(Type type)
        {
            var t = type.GenericTypeArguments?.FirstOrDefault();
            if (t == null)
                return type;
            return RemoveGenericType(t);
        }

        /// <summary>
        /// typeのフィールドを再帰的に探索してSerializeInfoを作成しworkに書き込む
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private static bool CollectSerializeInfos(Type type, BindingFlags flags, Work work)
        {
            // 完全対象外チェック
            if (work.IsIgnore(type))
                return false;

            // すでに探索済みなら無視
            if (work.Visited.Contains(type))
                return work.SerializeInfos.ContainsKey(type);

            work.Visited.Add(type);
            // 自分自身が対象の場合はSerializeInfoにも入れる
            if (work.IsTarget(type))
                work.SerializeInfos.TryAdd(type, new SerializeInfo());

            var fieldInfos = type.GetFields(flags);
            foreach (var fieldInfo in fieldInfos)
            {
                // 自身のフィールドでない場合は無視
                if (fieldInfo.DeclaringType != type)
                    continue;

                var fieldType = RemoveGenericType(fieldInfo.FieldType);

                // 対象外チェック
                if (work.IsIgnore(fieldType))
                    continue;

                var found = CollectSerializeInfos(fieldType, flags, work);
                // 子クラスに対してもチェックを行う
                if (work.ChildTypes.TryGetValue(fieldType, out var childTypes))
                {
                    foreach (var ct in childTypes)
                    {
                        if (CollectSerializeInfos(ct, flags, work))
                            found = true;
                    }
                }

                // 再起したメンバーを持っている場合. の親でチェック中の場合はfalseが返るが
                if (found)
                {
                    work.SerializeInfos.TryAdd(type, new SerializeInfo());
                    work.SerializeInfos[type].Fields.Add(fieldInfo);
                }
            }

            return work.SerializeInfos.ContainsKey(type);
        }

        /// <summary>
        /// インデント周りを考慮したStringBuilder
        /// </summary>
        private class CodeBuilder
        {
            readonly StringBuilder sb = new StringBuilder();
            private int indent = 0;

            private string Tab => new string('\t', indent);

            public class TabLine : IDisposable
            {
                private readonly CodeBuilder codeBuilder;

                public TabLine(CodeBuilder codeBuilder)
                {
                    this.codeBuilder = codeBuilder;
                    codeBuilder.indent++;
                }

                public void Dispose()
                {
                    codeBuilder.indent--;
                    codeBuilder.AppendLine("}");
                }
            }

            public void AppendLine(string line)
            {
                sb.AppendLine($"{Tab}{line}");
            }

            public TabLine Nest(string line)
            {
                AppendLine(line + "{");
                return new TabLine(this);
            }

            public override string ToString()
            {
                return sb.ToString();
            }
        }

        /// <summary>
        /// Genericな型に対して良い感じの型名を返す
        /// https://stackoverflow.com/questions/1533115/get-generictype-name-in-good-format-using-reflection-on-c-sharp
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string GetWellFormedFullName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;
            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.Ordinal)));
            sb.Append(t.GetGenericArguments().Aggregate("<",
                (aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetWellFormedFullName(type)));
            sb.Append(">");

            return sb.ToString();
        }
        /// <summary>
        /// typeに関して
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        private static void Generate(Type type, string path)
        {
            var work = new Work { TargetNameSpace = type.Namespace, };

            foreach (var t in type.Assembly.DefinedTypes)
            {
                foreach (var baseType in t.GetBaseTypes())
                {
                    if (!work.ChildTypes.ContainsKey(baseType))
                        work.ChildTypes[baseType] = new HashSet<Type>();
                    work.ChildTypes[baseType].Add(t);
                }
            }

            foreach (var t in CollectDataTypes)
            {
                work.CollectTargetTypes.Add(t);
                if (work.ChildTypes.TryGetValue(t, out var childTypes))
                {
                    foreach (var ct in childTypes)
                    {
                        work.CollectTargetTypes.Add(ct);
                    }
                }
            }

            work.SerializeInfos.TryAdd(type, new SerializeInfo());
            CollectSerializeInfos(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, work);

            bool IsCodeGenTarget(Type type)
            {
                if (type == null)
                    return false;
                if (work.SerializeInfos.TryGetValue(type, out var si) && si.Fields.Count > 0)
                    return true;
                return type.BaseType != null && IsCodeGenTarget(type.BaseType);
            }

            var sb = new CodeBuilder();

            sb.AppendLine("// GENERATED CODE");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Reflection;");
            var serializeWorkClassName = $"Collect{type.Name}Work";
            using (sb.Nest($"namespace {type.Namespace}"))
            {
                using (sb.Nest($"internal class {serializeWorkClassName}"))
                {
                    string FieldName(Type t) => $"{t.Name}s";

                    sb.AppendLine("public HashSet<object> Visited {get;} = new();");

                    foreach (var x in CollectDataTypes)
                    {
                        sb.AppendLine($"public HashSet<{x.Name}> {FieldName(x)} {{ get; }} = new ();");
                    }

                    foreach (var x in CollectDataTypes)
                    {
                        sb.AppendLine($"public bool TryAdd({x.Name} x) {{ return {FieldName(x)}.Add(x); }}");
                    }
                }
            }

            // privateフィールドに対してReflectionで取得するかどうか
            // #NOTE : 今後partialではなくSerializer単体でデータ収集できるようにするためのもの
            //       : 現状はoverride対応が面倒なのでpartialでやっている
            bool useReflection = false;
            // ここからはSerializeInfoを元にコード生成
            // namespaceごとに分ける
            foreach (var group in work.SerializeInfos.GroupBy(x => x.Key.Namespace))
            {
                using var ns = sb.Nest($"namespace {group.Key}");
                foreach (var (key, val) in group.OrderBy(x => x.Key.Name))
                {
                    if (IsCodeGenTarget(key) == false)
                        continue;
                    using (sb.Nest($"partial class {key.Name}"))
                    {
                        var isTop = IsCodeGenTarget(key.BaseType) == false;

                        Dictionary<string, FieldInfo> privateFields = new();
                        sb.AppendLine($"// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する");
                        using (sb.Nest($"internal {(isTop ? "virtual " : "override")} bool Collect({serializeWorkClassName} collectWork)"))
                        {
                            // 親がいる場合はVisitedチェックは親に任せる(暴発防止)
                            if (IsCodeGenTarget(key.BaseType))
                            {
                                sb.AppendLine($"if(base.Collect(collectWork) == false) return false;");
                            }
                            else
                            {
                                sb.AppendLine($"if(collectWork.Visited.Add(this)== false) return false;");
                            }

                            foreach (var fieldInfo in val.Fields)
                            {
                                var originFieldName = fieldInfo.Name;

                                // 
                                if (fieldInfo.IsPrivate)
                                {
                                    // プロパティの場合はそっちに名前変更
                                    var m = TypeUtil.BackingFieldNameRegex.Match(originFieldName);
                                    if (m.Success)
                                    {
                                        originFieldName = m.Groups[1].Value;
                                    }

                                    if (useReflection)
                                    {
                                        var fieldInfoName = $"{originFieldName}FieldInfo";
                                        privateFields.Add(fieldInfoName, fieldInfo);
                                        sb.AppendLine(
                                            $"var {originFieldName}Tmp = {fieldInfoName}.GetValue(this) as {GetWellFormedFullName(fieldInfo.FieldType)};");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"var {originFieldName}Tmp = {originFieldName};");
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"var {originFieldName}Tmp = {originFieldName};");
                                }

                                var fieldName = $"{originFieldName}Tmp";
                                var rawFieldType = RemoveGenericType(fieldInfo.FieldType);


                                void AddCollectWork(string name)
                                {
                                    using var _ = sb.Nest($"if({name} != null)");
                                    if (work.IsTarget(rawFieldType))
                                    {
                                        using var a = sb.Nest($"if(collectWork.TryAdd({name}))");
                                        if (IsCodeGenTarget(rawFieldType))
                                            sb.AppendLine($"{name}.Collect(collectWork);");
                                    }
                                    else
                                    {

                                        if (IsCodeGenTarget(rawFieldType))
                                            sb.AppendLine($"{name}.Collect(collectWork);");
                                    }
                                }

                                if (rawFieldType != fieldInfo.FieldType)
                                {
                                    // #TODO : 2重リスト対応
                                    using (sb.Nest($"foreach(var v in {fieldName} ?? new ())"))
                                    {
                                        AddCollectWork("v");
                                    }
                                }
                                else
                                {
                                    AddCollectWork(fieldName);
                                }
                            }
                            sb.AppendLine("return true;");
                        }

                        foreach (var (fieldName, fieldInfo) in privateFields)
                        {
                            sb.AppendLine($"private static readonly FieldInfo {fieldName} = typeof({key.Name}).GetField(\"{fieldInfo.Name}\", BindingFlags.Instance | BindingFlags.NonPublic);");
                        }
                    }
                }
            }

            File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// 型Tのスクリプトが置かれているファイルパスを返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetScriptPath<T>() where T : class
        {
            // クラス型に対応するMonoScriptを探す
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(T).Name}");
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == typeof(T)) {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }
        
        
        /// <summary>
        /// #NOTE : 道路ネットワークのフォーマットに変更が走った場合は下記のMenuItemを有効化したうえで実行する
        /// RnModelに対してシリアライズ用のデータ抽出を行うコードを生成する.
        /// </summary>
        //[MenuItem("PLATEAU/Debug/Generate RnModel Export Code")]
        public static void GenerateRnDataCollectCode()
        {
            string thisFilePath = GetScriptPath<RnDataSerializeCodeGen>();
            var path = Path.Combine(Path.GetDirectoryName(thisFilePath),
                "../../../Runtime/RoadNetwork/Structure/RnModel.generated.cs");
            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException($"not found : {path}");
            }
            Generate(typeof(RnModel), path);
        }
    }
}