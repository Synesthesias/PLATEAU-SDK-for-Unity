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
using CityObjectList = PLATEAU.CityInfo.CityObjectList;
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
        /// 指定オブジェクトとその子を一括でまとめて共通ライブラリに渡して変換します。
        /// </summary>
        public async Task<PlaceToSceneResult> ConvertAsync(GranularityConvertOptionUnity conf,
            IProgressBar progressBar)
        {
            try
            {
                if (!conf.IsValid())
                {
                    return PlaceToSceneResult.Fail();
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

                var unityMeshToDllSubMeshConverter = new GameMaterialIDRegistry();

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
                    new RecoverFromGameMaterialID(unityMeshToDllSubMeshConverter);
                var placeToSceneConf =
                    new PlaceToSceneConfig(materialConverterToUnity, true, null, null, infoForToolkits,
                        conf.NativeOption.Granularity.ToMeshGranularity());

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
                    return PlaceToSceneResult.Fail();
                }

                // 覚えておいたものを復元します
                nonLibDataHolder.RestoreTo(result.GeneratedRootTransforms);

                if (conf.DoDestroySrcObjs)
                {
                    foreach (var srcTrans in conf.SrcTransforms.Get)
                    {
                        if (srcTrans == null) continue;
                        Object.DestroyImmediate(srcTrans.gameObject);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                return PlaceToSceneResult.Fail();
            }
        }


        /// <summary> 前バージョンとの互換性のために残しておきます </summary>
        public async Task<PlaceToSceneResult> ConvertAsync(GranularityConvertOptionUnity conf)
        {
            using var progressBar = new ProgressBar("");
            return await ConvertAsync(conf, progressBar);
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

        public CityInfo.CityObjectList.CityObject[] GetDstCityObjectsByNode(Node node, CityObjectIndex? _, string parentGmlID)
        {
            var mesh = node.Mesh;
            if (mesh == null) return null;
            var col = mesh.CityObjectList;
            if (col.Length == 0) return null;
            var indices = col.GetAllKeys();
            var ret = new List<CityObjectList.CityObject>();
            foreach (var idx in indices)
            {
                string gmlID = col.GetAtomicID(idx);
                
                // 親と重複する分は登録しない（主要内マテリアルごとで必要）
                if (gmlID == parentGmlID) continue;
                
                var co = GetDstCityObjectByGmlID(gmlID, _);
                if (co == null) continue;
                ret.Add(co);
            }

            return ret.ToArray();
        }

        public CityInfo.CityObjectList.CityObject GetDstCityObjectByGmlID(string gmlIDArg, CityObjectIndex? _)
        {
            string gmlID = gmlIDArg.EndsWith("_combined") ? gmlIDArg.Replace("_combined", "") : gmlIDArg;
            if (srcData.TryGet(gmlID, out var serializedCityObj))
            {
                var srcCityObj = serializedCityObj;
                return srcCityObj.CopyWithoutChildren();
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