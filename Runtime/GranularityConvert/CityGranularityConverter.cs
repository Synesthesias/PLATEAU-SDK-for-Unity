using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityAdjust.NonLibData;
using PLATEAU.CityAdjust.NonLibDataHolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// オブジェクトの分割と結合を行います。
    /// </summary>
    public class CityGranularityConverter
    {
        /// <summary>
        /// 粒度を変換する処理を、各ゲームオブジェクトに対して逐次的に行うためのメソッドです。
        /// マテリアル分けを伴わない粒度変換であれば、このメソッドが直接利用されます。
        /// マテリアル分けを伴う粒度変換であれば、<see cref="MAExecutor"/>を介してこのメソッドが利用されます。
        /// </summary>
        public async Task<GranularityConvertResult> ConvertProgressiveAsync(MAExecutorConf conf,
            IMACondition maCondition)
        {
            
            var dstGranularity = conf.MeshGranularity;
            var result = new GranularityConvertResult();
            using var progressBar = new ProgressBar();

            // === ここから分解
            // 分解すべきオブジェクトを数える
            int objCountToDeconstruct = 0;
            conf.TargetTransforms
                .BfsExec(
                    trans =>
                    {
                        if (maCondition.ShouldDeconstruct(trans, conf.MeshGranularity)) objCountToDeconstruct++;
                        return NextSearchFlow.Continue;
                    });
            int countDeconstructed = 0;

            // 幅優先探索で、分解が必要なゲームオブジェクトを1つ見つけるごとに分解します。
            await conf.TargetTransforms.BfsExecAsync(async trans =>
            {
                if (!maCondition.ShouldDeconstruct(trans, conf.MeshGranularity)) return NextSearchFlow.Continue;
                progressBar.Display($"分解中 : {countDeconstructed + 1}/{objCountToDeconstruct} : {trans.name}", 0.3f);
                // 分解
                GranularityConvertOptionUnity currentConf = new GranularityConvertOptionUnity(
                    new GranularityConvertOption(conf.MeshGranularity.ToNativeGranularity(), 0),
                    new UniqueParentTransformList(trans),
                    conf.DoDestroySrcObjs);
                var currentResult = await ConvertAsync(currentConf, new DummyProgressBar());
                result.Merge(currentResult);
                if (!result.IsSucceed)
                {
                    Debug.LogError($"{trans.name}の分解に失敗したため処理を中断しました。");
                    return NextSearchFlow.Abort;
                }

                countDeconstructed++;
                return NextSearchFlow.SkipChildren; // 分解後、子は望みの粒度になったはずなのでスキップ
            });

            
            // === ここから結合
            // 深さ優先探索で、結合が必要なゲームオブジェクトを見つけて1つづつ結合します。


            if (countDeconstructed == 0) // 結合と分割は通常どちらかだけで良い
            {
                
                // 結合すべきものをここに記録します。
                var combineDict = new CombineDict();

                // 親Transformがnull(すなわちroot)のとき、combineDictのkeyをnullとしたいが、
                // Dictionaryのキーにnullは指定できないのでroot用の一時ゲームオブジェクトを作成
                var tmpRoot = new GameObject("tmpRoot");
                
                
                conf.TargetTransforms.DfsExec(trans =>
                {
                    if (trans == null)
                    {
                        Debug.LogError($"{trans.name} is null.");
                        return NextSearchFlow.SkipChildren;
                    }

                    // maConditionを使わず意図的にSimpleのメソッドを呼びます
                    if (!new MAConditionSimple().ShouldConstruct(trans, conf.MeshGranularity))
                        return NextSearchFlow.Continue;


                    // 主要内マテリアルごと結合の場合、主要の結合はせず最小へと処理を回します
                    if (dstGranularity == MAGranularity.PerMaterialInPrimary)
                    {
                        var cog = trans.GetComponent<PLATEAUCityObjectGroup>();
                        if (cog != null && cog.Granularity == MeshGranularity.PerPrimaryFeatureObject)
                        {
                            return NextSearchFlow.Continue;
                        }
                    }

                    // 結合リストに追加します.
                    // すなわち、後で結合するために、各結合が何を条件として行われるべきかを記録します。
                    var parentTrans = trans.parent;
                    if (parentTrans == null) parentTrans = tmpRoot.transform;
                    Material[] materialKey = null;
                    // 結合単位が主要内マテリアルごとでなければ、結合対象はあるTransformの子すべてなので、結合リストのキー（結合条件）は親オブジェクトのみです（マテリアルはnull）。
                    // 結合単位が主要内マテリアルごとの場合は、結合対象はあるTransformの子の一部なので、結合リストのキー（結合条件）は親オブジェクトとマテリアルになります。
                    if (dstGranularity == MAGranularity.PerMaterialInPrimary)
                    {
                        var mr = trans.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            materialKey = mr.sharedMaterials;
                        }
                    }
                    combineDict.Add(new CombineKey(parentTrans, materialKey), trans);

                    return NextSearchFlow.SkipChildren; // 明示せずとも子は結合対象になるのでスキップ
                });

                int countCombined = 0;
                // 実際の結合処理
                foreach (var (key, combineList) in combineDict.Data)
                {
                    if (combineList.Count <= 0) continue;
                    MeshGranularity convertGran = dstGranularity == MAGranularity.PerMaterialInPrimary
                        ? MeshGranularity.PerCityModelArea // 指定のもの（マテリアルを同じくするもの）を全部結合したい
                        : dstGranularity.ToNativeGranularity();
                    GranularityConvertOptionUnity currentConf = new GranularityConvertOptionUnity(
                        new GranularityConvertOption(convertGran, 1),
                        new UniqueParentTransformList(combineList),
                        conf.DoDestroySrcObjs);
                    progressBar.Display($"結合中 : {countCombined + 1}/{combineDict.Data.Count} : {key.Parent.name}の子", 0.3f);
                    var currentResult = await ConvertAsync(currentConf, new DummyProgressBar());
                    foreach (var generated in currentResult.GeneratedRootTransforms.Get)
                    {
                        generated.parent = key.Parent;
                        if (dstGranularity == MAGranularity.PerMaterialInPrimary)
                        {
                            generated.name = key.Materials[0] == null ? "null material" : key.Materials[0].name;
                            generated.GetComponent<PLATEAUCityObjectGroup>().Granularity =
                                MeshGranularity.PerAtomicFeatureObject;
                        }
                    }

                    result.Merge(currentResult);
                    if (!result.IsSucceed)
                    {
                        Debug.LogError($"結合失敗 :");
                    }

                    countCombined++;
                } // 実際の結合処理 ここまで 
                
                // === 後処理
                // tmpRootの子をrootに
                foreach (Transform root in tmpRoot.transform)
                {
                    root.parent = null;
                }

                Object.DestroyImmediate(tmpRoot);
            }

            return result;
        }


        /// <summary>
        /// 指定オブジェクトとその子を一括でまとめて共通ライブラリに渡して変換します。
        /// </summary>
        public async Task<GranularityConvertResult> ConvertAsync(GranularityConvertOptionUnity conf,
            IProgressBar progressBar)
        {
            try
            {
                if (!conf.IsValid())
                {
                    return GranularityConvertResult.Fail();
                }

                progressBar.Display("属性情報を取得中...", 0.1f);
                
                
                // 属性情報など、覚えておくべきものを記録します。
                var nonLibDataHolder = new NonLibDataHolder(
                    new PositionRotationDict(),
                    new InstancedCityModelDict(),
                    new GmlIdToSerializedCityObj()
                );
                nonLibDataHolder.ComposeFrom(conf.SrcTransforms);


                progressBar.Display("ゲームオブジェクトを共通モデルに変換中...", 0.2f);

                var unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithGameMaterial();

                // ゲームオブジェクトを共通ライブラリのModelに変換します。
                using var srcModel = UnityMeshToDllModelConverter.Convert(
                    conf.SrcTransforms,
                    unityMeshToDllSubMeshConverter,
                    true, // 非表示のゲームオブジェクトも対象に含めます。なぜなら、LOD0とLOD1のうちLOD1だけがActiveになっているという状況で、変換後もToolkitsのLOD機能を使えるようにするためです。
                    VertexConverterFactory.NoopConverter());

                progressBar.Display("共通モデルの変換中...", 0.5f);

                // 共通ライブラリの機能でモデルを分割・結合します。
                var converter = new GranularityConverter();
                using var dstModel = converter.Convert(srcModel, conf.NativeOption);

                progressBar.Display("変換後の3Dモデルを配置中...", 0.8f);

                // Toolkits向けの処理です
                bool isTextureCombined = SearchFirstCityObjGroup(conf.SrcTransforms).InfoForToolkits.IsTextureCombined;
                var infoForToolkits = new CityObjectGroupInfoForToolkits(isTextureCombined, true);

                // Modelをゲームオブジェクトに変換して配置します。
                var commonParent = CalcCommonParent(conf.SrcTransforms.Get.ToArray());

                var materialConverterToUnity =
                    new DllSubMeshToUnityMaterialByGameMaterial(unityMeshToDllSubMeshConverter);
                var placeToSceneConf =
                    new PlaceToSceneConfig(materialConverterToUnity, true, null, null, infoForToolkits,
                        conf.NativeOption.Granularity);

                var result = await PlateauToUnityModelConverter.PlateauModelToScene(
                    commonParent,
                    new DummyProgressDisplay(),
                    "",
                    placeToSceneConf,
                    dstModel,
                    new AttributeDataHelper(
                        new SerializedCityObjectGetterFromDict(nonLibDataHolder.Get<GmlIdToSerializedCityObj>(), dstModel),
                        true
                    ),
                    true);
                if (!result.IsSucceed)
                {
                    throw new Exception("Failed to convert plateau model to scene game objects.");
                }

                if (result.GeneratedRootTransforms.Count <= 0)
                {
                    Dialogue.Display("変換対象がありません。\nアクティブなオブジェクトを選択してください。", "OK");
                    return GranularityConvertResult.Fail();
                }

                // 覚えておいたものを復元します
                nonLibDataHolder.RestoreTo(result.GeneratedRootTransforms);

                if (conf.DoDestroySrcObjs)
                {
                    foreach (var srcTrans in conf.SrcTransforms.Get)
                    {
                        Object.DestroyImmediate(srcTrans.gameObject);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                return GranularityConvertResult.Fail();
            }
        }


        /// <summary> 前バージョンとの互換性のために残しておきます </summary>
        public async Task<GranularityConvertResult> ConvertAsync(GranularityConvertOptionUnity conf)
        {
            return await ConvertAsync(conf, new ProgressBar(""));
        }

        /// <summary>
        /// 引数の共通の親を探し、親のうちもっとも階層上の子であるものを返します。
        /// 共通の親がない場合、nullを返します。
        /// </summary>
        private static Transform CalcCommonParent(IReadOnlyList<Transform> srcList)
        {
            // 各親が、srcListのうちいくつの親であるかを数えます。
            Dictionary<Transform, int> descendantCountDict = new();
            foreach (var src in srcList)
            {
                var parent = src.parent;
                // 親をたどりながら子孫カウントをインクリメントします。
                while (parent != null)
                {
                    if (descendantCountDict.ContainsKey(parent))
                    {
                        descendantCountDict[parent]++;
                    }
                    else
                    {
                        descendantCountDict.Add(parent, 1);
                    }

                    parent = parent.parent;
                }
            }

            if (descendantCountDict.Count == 0) return null;

            var commonParents = descendantCountDict
                .Where(pair => pair.Value == srcList.Count)
                .Select(pair => pair.Key)
                .ToArray();
            if (commonParents.Length == 0) return null;

            // 共通の親のうち、もっとも子であるものを探します。
            for (int i = 0; i < commonParents.Length; i++)
            {
                var trans1 = commonParents[i];
                bool isTrans1Parented = false;
                for (int j = i + 1; j < commonParents.Length; j++)
                {
                    var trans2 = commonParents[j];
                    if (trans2.IsChildOf(trans1))
                    {
                        isTrans1Parented = true;
                        break;
                    }
                }

                if (!isTrans1Parented) return trans1;
            }

            throw new Exception("Failed to search common parent.");
        }

        private PLATEAUCityObjectGroup SearchFirstCityObjGroup(UniqueParentTransformList transforms)
        {
            foreach (var t in transforms.Get)
            {
                var cityObjGroups = t.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                if (cityObjGroups.Length > 0)
                {
                    return cityObjGroups[0];
                }
            }

            return null;
        }
    }


    /// <summary>
    /// 辞書からCityObjectを取得します。
    /// 分割結合で利用します。
    /// </summary>
    public class SerializedCityObjectGetterFromDict : ISerializedCityObjectGetter
    {
        private readonly GmlIdToSerializedCityObj srcData;
        private readonly Model dstModel;

        public SerializedCityObjectGetterFromDict(GmlIdToSerializedCityObj srcDict, Model dstModel)
        {
            srcData = srcDict;
            this.dstModel = dstModel;
        }

        public CityInfo.CityObjectList.CityObject GetDstCityObjectByID(string gmlIDArg, CityObjectIndex? _)
        {
            string gmlID = gmlIDArg.EndsWith("_combined") ? gmlIDArg.Replace("_combined", "") : gmlIDArg;
            if (srcData.TryGet(gmlID, out var serializedCityObj))
            {
                var srcCityObj = serializedCityObj;
                return srcCityObj.Copy();
            }
            else
            {
                Debug.LogWarning($"gmlID not found : {gmlID}");
                return null;
            }
        }

        public void Dispose()
        {
            // NOP
        }
    }


    /// <summary>
    /// 結合対象を記録するための辞書です。
    /// キーバリューペアは1つの結合を示し、キーは結合条件、バリューはその条件に合致する結合対象のリストを示します。
    /// </summary>
    internal class CombineDict
    {
        public Dictionary<CombineKey, List<Transform>> Data { get; } = new();
        
        public void Add(CombineKey key, Transform target)
        {
            var list = Data.GetValueOrCreate(key);
            list.Add(target);
        }

    }

    /// <summary>
    /// 1つの結合処理について、何を結合するかを表したキーです。
    /// <see cref="Parent"/>を親とし、マテリアルごとに結合する場合は<see cref="Material"/>を持つものを結合対象とします。
    /// マテリアルごとの結合でない場合は<see cref="Material"/>はnullとしてください。
    /// </summary>
    internal class CombineKey
    {
        public Transform Parent { get; }
        public Material[] Materials { get; }

        public CombineKey(Transform parent, Material[] materials)
        {
            Parent = parent;
            Materials = materials;
        }

        // Materialに関しては名前が合っていれば同値とします。
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (CombineKey)obj;
            if (Parent != other.Parent) return false;
            if ((Materials == null) != (other.Materials == null)) return false;
            if (Materials == null || other.Materials == null) return true;
            if (Materials.Length != other.Materials.Length) return false;
            for (int i = 0; i < Materials.Length; i++)
            {
                if (Materials[i].name != other.Materials[i].name) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Parent);
            if (Materials != null)
            {
                foreach (var m in Materials)
                {
                    // マテリアルは名前のみ確認
                    hash.Add(m.name);
                }
            }
            return hash.ToHashCode();
        }

    }

}