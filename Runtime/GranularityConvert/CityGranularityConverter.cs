using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor;
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
        public async Task<GranularityConvertResult> ConvertProgressiveAsync(GranularityConvertOptionUnity conf)
        {
            var dstGranularity = conf.NativeOption.Granularity;
            var result = new GranularityConvertResult();
            using var progressBar = new ProgressBar();

            // 分解すべきオブジェクトを数える
            int objCountToDeconstruct = 0;
            conf.SrcTransforms
                .BfsExec(
                    trans =>
                    {
                        if (ShouldDeconstruct(trans, dstGranularity)) objCountToDeconstruct++;
                        return NextSearchFlow.Continue;
                    });
            int countDeconstructed = 0;

            // 幅優先探索で、分解が必要なゲームオブジェクトを1つ見つけるごとに分解します。
            await conf.SrcTransforms.BfsExecAsync(async trans =>
            {
                if (!ShouldDeconstruct(trans, dstGranularity)) return NextSearchFlow.Continue;
                progressBar.Display($"分解中 : {countDeconstructed+1}/{objCountToDeconstruct} : {trans.name}", 0.3f);
                // 分解
                GranularityConvertOptionUnity currentConf = new GranularityConvertOptionUnity(
                    new GranularityConvertOption(dstGranularity, 0),
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

            // 深さ優先探索で、結合が必要なゲームオブジェクトを見つけて1つづつ結合します。
            
            // 結合すべきものをここに記録
            Dictionary<Transform, List<Transform>> combineDict = new(); // key: 親Transform, value: その親のもとで結合すべきTransformのリスト
            
            // 親Transformがnull(すなわちroot)のとき、combineDictのkeyをnullとしたいが、
            // Dictionaryのキーにnullは指定できないのでroot用の一時ゲームオブジェクトを作成
            var tmpRoot = new GameObject("tmpRoot");
            
            
            await conf.SrcTransforms.DfsExecAsync(async trans =>
            {
                if (!ShouldConstruct(trans, dstGranularity)) return NextSearchFlow.Continue;
                
                // 結合リストに追加
                var parentTrans = trans.parent;
                if(parentTrans == null) parentTrans = tmpRoot.transform;
                if (combineDict.TryGetValue(parentTrans, out var listToCombine))
                {
                    listToCombine.Add(trans);
                }
                else
                {
                    combineDict.Add(parentTrans, new List<Transform>{trans});
                }

                return NextSearchFlow.SkipChildren; // 明示せずとも子は結合対象になるのでスキップ

            });

            int countCombined = 0;
            // 実際の結合処理
            foreach(var (parent, combineList) in combineDict)
            {
                if (combineList.Count <= 0) continue;
                GranularityConvertOptionUnity currentConf = new GranularityConvertOptionUnity(
                    new GranularityConvertOption(dstGranularity, 0),
                    new UniqueParentTransformList(combineList),
                    conf.DoDestroySrcObjs);
                progressBar.Display($"結合中 : {countCombined+1}/{combineDict.Count} : {parent.name}の子", 0.3f);
                var currentResult = await ConvertAsync(currentConf, new DummyProgressBar());
                foreach (var generated in currentResult.GeneratedRootTransforms.Get)
                {
                    generated.parent = parent;
                }
                result.Merge(currentResult);
                if (!result.IsSucceed)
                {
                    Debug.LogError($"結合失敗 :");
                }

                countCombined++;
            }
            
            // tmpRootの子をrootに
            foreach (Transform root in tmpRoot.transform)
            {
                root.parent = null;
            }
            Object.DestroyImmediate(tmpRoot);
            
            return result;
        }

        private bool ShouldDeconstruct(Transform trans, MeshGranularity dstGranularity)
        {
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            if (dstGranularity >= srcGranularity) return false;
            return true;
        }

        private bool ShouldConstruct(Transform trans, MeshGranularity dstGranularity)
        {
            // 上のメソッドとほぼ同じ
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if (cityObjGroup == null) return false;
            var srcGranularity = cityObjGroup.Granularity;
            if (dstGranularity <= srcGranularity) return false;
            return true;
        }

        /// <summary>
        /// 指定オブジェクトとその子を一括でまとめて共通ライブラリに渡して変換します。
        /// </summary>
        public async Task<GranularityConvertResult> ConvertAsync(GranularityConvertOptionUnity conf, IProgressBar progressBar)
        {
            try
            {
                if (!conf.IsValid())
                {
                    return GranularityConvertResult.Fail();
                }
                
                progressBar.Display("属性情報を取得中...", 0.1f);

                // 属性情報を覚えておきます。
                var attributes = GmlIdToSerializedCityObj.ComposeFrom(conf.SrcTransforms);
                
                // PLATEAUInstancedCityModel が含まれる場合、これもコピーしたいので覚えておきます。
                var instancedCityModelDict = InstancedCityModelDict.ComposeFrom(conf.SrcTransforms); 
                

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

                var materialConverterToUnity = new DllSubMeshToUnityMaterialByGameMaterial(unityMeshToDllSubMeshConverter);
                var placeToSceneConf =
                    new PlaceToSceneConfig(materialConverterToUnity, true, null, null, infoForToolkits, conf.NativeOption.Granularity);

                var result = await PlateauToUnityModelConverter.PlateauModelToScene(
                    commonParent,
                    new DummyProgressDisplay(),
                    "",
                    placeToSceneConf,
                    dstModel,
                    new AttributeDataHelper(
                        new SerializedCityObjectGetterFromDict(attributes, dstModel),
                        conf.NativeOption.Granularity,
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
                
                // PLATEAUInstancedCityModelを復元します。
                instancedCityModelDict.Restore(result.GeneratedRootTransforms);

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
             foreach(var t in transforms.Get)
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

        public CityInfo.CityObjectList.CityObject GetDstCityObjectByID(string gmlID, CityObjectIndex? _)
        {
            if (srcData.TryGet(gmlID, out var serializedCityObj))
            {
                var srcCityObj =  serializedCityObj;
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
}