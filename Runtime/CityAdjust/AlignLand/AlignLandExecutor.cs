using PLATEAU.CityAdjust.NonLibData;
using PLATEAU.CityAdjust.NonLibDataHolder;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.GranularityConvert;
using PLATEAU.HeightMapAlign;
using PLATEAU.PolygonMesh;
using PLATEAU.TerrainConvert;
using PLATEAU.Texture;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityAdjust.AlignLand
{
    public class AlignLandExecutor
    {
        private const float HeightOffset = 0.3f; // 高さ合わせの結果に対して、埋まらないようにこれだけ上げます(メートル)
        
        public async Task ExecAsync(ALConfig conf, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress("", 0f, "ハイトマップを作成中...");
            var landTransforms = conf.Lands;

            List<ConvertedTerrainData.HeightmapData> heightmaps = new();
            
            // 土地ごとのループ
            foreach (var land in landTransforms)
            {
                // テレインの場合、テレインからハイトマップを取得します。
                if (land.GetComponent<Terrain>() != null)
                {
                    var terrain = land.GetComponent<Terrain>();
                    heightmaps.Add(ConvertedTerrainData.HeightmapData.CreateFromTerrain(terrain));
                }
                // メッシュの場合、メッシュからハイトマップを生成します。
                else if (land.GetComponent<MeshRenderer>() != null)
                {
                    var heights = CreateHeightMapFromMesh(land, conf);
                    if (heights == null)
                    {
                        Dialogue.Display("ハイトマップの生成に失敗しました。", "OK");
                        return;
                    }
                    heightmaps.AddRange(heights);
                }
                else
                {
                    Dialogue.Display("地形を取得できませんでした。", "OK");
                    return;
                }
            }
           

            // 高さを変えるモデルについて情報収集します。
            progressDisplay.SetProgress("", 0f, "処理対象の情報を収集中...");
            var alignTarget = new UniqueParentTransformList();
            var alignInvertTarget = new UniqueParentTransformList();
            foreach (var cog in conf.TargetModel.GetComponentsInChildren<PLATEAUCityObjectGroup>())
            {
                var package = conf.TargetModel.GetPackage(cog);
                if (!conf.TargetPackages.Contains(package)) continue;
                var targetMf = cog.GetComponent<MeshFilter>();
                if (targetMf == null) continue;
                var targetMesh = targetMf.sharedMesh;
                if (targetMesh == null) continue;

                // LOD3の道路は、道路の高さの正確性を尊重するため、土地のほうを道路に合わせます。
                // それ以外はモデルを土地に合わせます。
                if (package == PredefinedCityModelPackage.Road && cog.Lod >= 3)
                {
                    alignInvertTarget.Add(cog.transform);
                }
                else
                {
                    alignTarget.Add(cog.transform);
                }
                
            }

            var nonLibDataHolder = new NonLibData.NonLibDataHolder(
                new GmlIdToSerializedCityObj(),
                new NameToAttrsDict(),
                new ContourMeshesMaker()
            );
            nonLibDataHolder.ComposeFrom(alignTarget);

            // 土地の高さをC++に送ります。
            using var heightmapAligner = HeightMapAligner.Create(HeightOffset, CoordinateSystem.EUN);
            foreach (var m in heightmaps)
            {
                heightmapAligner.AddHeightmapFrame(m.HeightData, m.textureWidth, m.textureHeight, (float)m.min.X, (float)m.max.X, (float)m.min.Y, (float)m.max.Y, (float)m.min.Z, (float)m.max.Z, CoordinateSystem.EUN);
            }
            
            
            var sumResult = new PlaceToSceneResult();
            var alTargets = alignTarget.Get.ToArray();
            var alignTargetModels = new Model[alTargets.Length];
            var subMeshConverters = new GameMaterialIDRegistry[alTargets.Length];
            // 非土地の高さ合わせターゲットメッシュをC++のModelに変換します
            for (int i = 0; i < alTargets.Length; i++)
            {
                var target = alTargets[i];
                progressDisplay.SetProgress(target.name, ((float)i * 100)/alTargets.Length, "共通ライブラリのModelに変換中...");
                subMeshConverters[i] = new GameMaterialIDRegistry();
                alignTargetModels[i] = UnityMeshToDllModelConverter.Convert(
                    new UniqueParentTransformList(target),
                    subMeshConverters[i],
                    false,
                    VertexConverterFactory.NoopConverter());
            }

            if (conf.AlignInvert)
            {
                // 逆高さ合わせのモデルをC++のModelに変換します
                var alInvertTargets = alignInvertTarget.Get.ToArray();
                var alignInvertTargetModels = new Model[alInvertTargets.Length];
                for (int i = 0; i < alInvertTargets.Length; i++)
                {
                    var target = alInvertTargets[i];
                    progressDisplay.SetProgress(target.name, ((float)i * 100)/alInvertTargets.Length, "共通ライブラリのModelに変換中(alignInvert対象)...");
                    alignInvertTargetModels[i] = UnityMeshToDllModelConverter.Convert(
                        new UniqueParentTransformList(target),
                        new UnityMeshToDllSubMeshWithEmptyMaterial(),
                        false,
                        VertexConverterFactory.NoopConverter()
                    );
                }
            

                // 逆高さ合わせ（土地をモデルに合わせる）
                foreach (var m in alignInvertTargetModels)
                {
                    heightmapAligner.AlignInvert(m);
                }
                // 逆高さ合わせの結果を反映
                for (int i = 0; i < landTransforms.Length; i++)
                {
                    var land = landTransforms[i];
                    var terrain = land.GetComponent<Terrain>();
                    // 現状テレインのみ対応
                    if (terrain == null) continue;
                    var heightmap1d = heightmapAligner.GetHeightMapAt(i);
                    var terrainData = terrain.terrainData;
                    var heightmap2d = HeightmapGenerator.ConvertTo2DFloatArray(heightmap1d, terrainData.heightmapResolution, terrainData.heightmapResolution);
                    terrain.terrainData.SetHeights(0, 0, heightmap2d);
                }
            }

            if (conf.AlignLandNormal)
            {
                // 高さ合わせをします
                for (int i = 0; i < alTargets.Length; i++)
                {
                    var target = alTargets[i];
                    progressDisplay.SetProgress(target.name, ((float)i * 100)/alTargets.Length, "高さを変換中...");
                
            
                    // 高さ合わせをします。
                    heightmapAligner.Align(alignTargetModels[i]);
            
                    var result = await PlateauToUnityModelConverter.PlateauModelToScene(
                        null, new DummyProgressDisplay(), "",
                        new PlaceToSceneConfig(new RecoverFromGameMaterialID(subMeshConverters[i]), true, null, null,
                            new CityObjectGroupInfoForToolkits(false, false), MeshGranularity.PerPrimaryFeatureObject),
                        alignTargetModels[i],
                        new AttributeDataHelper(
                            new SerializedCityObjectGetterFromDict(nonLibDataHolder.Get<GmlIdToSerializedCityObj>(), alignTargetModels[i]),
                            true),
                        true
                    );
                
                    // 親を変換前と同じにします。
                    foreach (var r in result.GeneratedRootTransforms.Get)
                    {
                        r.parent = target.parent;
                    }
                
                    sumResult.Merge(result);
                }
                
                if (conf.DoDestroySrcObj)
                {
                    foreach (var src in alignTarget.Get)
                    {
                        Object.DestroyImmediate(src.gameObject);
                    }
                }
            }
            
            
            // 変換前の情報を復元します。
            nonLibDataHolder.RestoreTo(sumResult.GeneratedRootTransforms);
            
            
            #if UNITY_EDITOR
            var selected = Selection.objects.ToList();
            selected.AddRange(sumResult.GeneratedRootTransforms.Get.Select(trans => trans.gameObject).Cast<Object>());
            Selection.objects = selected.ToArray();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif
        }
        
        /// <summary>
        /// メッシュ形式の土地からハイトマップを生成します。
        /// 失敗時はnullを返します。
        /// </summary>
        private List<ConvertedTerrainData.HeightmapData> CreateHeightMapFromMesh(Transform landTrans, ALConfig conf)
        {
            var landMf = landTrans.GetComponent<MeshFilter>();
            if (landMf == null) return null;
            var landMesh = landMf.sharedMesh;
            if (landMesh == null) return null;
            var landModel = UnityMeshToDllModelConverter.Convert(
                new UniqueParentTransformList(landTrans),
                new UnityMeshToDllSubMeshWithEmptyMaterial(),
                false,
                VertexConverterFactory.NoopConverter());
            var terrain = new ConvertedTerrainData(
                landModel,
                new TerrainConvertOption(new GameObject[] { landTrans.gameObject }, conf.HeightmapWidth, false,
                    conf.FillEdges, conf.ApplyConvolutionFilterToHeightMap, TerrainConvertOption.ImageOutput.PNG));
            var heightmaps = terrain.GetHeightmapDataRecursive();
            if (heightmaps.Count == 0) return null;
            return heightmaps;
        }
    }

    internal class MinMax3d
    {
        public Vector3 Min { get; set; } = new Vector3(99999, 99999, 99999);
        public Vector3 Max { get; set; } = new Vector3(-99999, -99999, -99999);

        /// <summary>
        /// 指定地点がMinMaxの範囲内に収まるようにMinMaxを調整します。
        /// </summary>
        public void Include(Vector3 p)
        {
            Min = new Vector3(Mathf.Min(Min.x, p.x), Mathf.Min(Min.y, p.y), Mathf.Min(Min.z, p.z));
            Max = new Vector3(Mathf.Max(Max.x, p.x), Mathf.Max(Max.y, p.y), Mathf.Max(Max.z, p.z));
        }
    }
}