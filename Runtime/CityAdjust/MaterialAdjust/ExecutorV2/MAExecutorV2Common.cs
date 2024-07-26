using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.NonLibData;
using PLATEAU.CityAdjust.NonLibDataHolder;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2
{
    /// <summary>
    /// マテリアル分けの共通処理を提供します。
    /// </summary>
    internal class MAExecutorV2Common
    {
        /// <summary>
        /// ゲームオブジェクトをC++のModelに変換します
        /// </summary>
        public void Prepare(UniqueParentTransformList target, out NonLibData.NonLibDataHolder nonLibData, out GameMaterialIDRegistry materialRegistry, out GranularityConvertResult sceneResult)
        {
            nonLibData = new NonLibData.NonLibDataHolder(
                new InstancedCityModelDict(),
                new NameToAttrsDict(),
                new PositionRotationDict(),
                new GmlIdToSerializedCityObj()
            );
            nonLibData.ComposeFrom(target);
            materialRegistry = new GameMaterialIDRegistry();
            sceneResult = new GranularityConvertResult();
        }
        
        public Model ConvertToCppModel(Transform target, GameMaterialIDRegistry materialRegistry)
        {
            return UnityMeshToDllModelConverter.Convert(
                new UniqueParentTransformList(target),
                materialRegistry,
                false,
                VertexConverterFactory.NoopConverter()
            );
        }

        /// <summary>
        /// C++のモデルをゲームオブジェクトに変換します。
        /// </summary>
        public async Task PlaceModelToSceneAsync(Model model, Transform target, GameMaterialIDRegistry materialRegistry, NonLibData.NonLibDataHolder nonLibData, GranularityConvertResult currentResult)
        {
            var result = await
                PlateauToUnityModelConverter.PlateauModelToScene(
                target.parent,
                new DummyProgressDisplay(), // TODO
                "",
                new PlaceToSceneConfig(
                    new RecoverFromGameMaterialID(materialRegistry),
                    true, null, null,
                    new CityObjectGroupInfoForToolkits(false, false), // TODO
                    MeshGranularity.PerAtomicFeatureObject // TODO
                ),
                model,
                new AttributeDataHelper(
                    new SerializedCityObjectGetterFromDict(nonLibData.Get<GmlIdToSerializedCityObj>(), model),
                    false
                )
                , true
            );
            currentResult.Merge(result);
        }
        
        public void Finishing(GranularityConvertResult result, NonLibData.NonLibDataHolder nonLibData, MAExecutorConf conf)
        {
            nonLibData.RestoreTo(result.GeneratedRootTransforms);

            if (conf.DoDestroySrcObjs)
            {
                foreach (var src in conf.TargetTransforms.Get)
                {
                    Object.DestroyImmediate(src.gameObject);
                }
            }
        }
    }
}