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
    internal class TileAlignLandExecutor : AlignLandExecutor
    {
        public async Task ExecAsync(ALTileConfig conf, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress("", 0f, "ハイトマップを作成中...");
            List<ConvertedTerrainData.HeightmapData> heightmaps = new();
            var landTransforms = conf.LandTransformList;

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

            var alignTransforms = conf.AlignTransformList;
            foreach (var transform in alignTransforms)
            {
                foreach(var cog in transform.GetComponentsInChildren<PLATEAUCityObjectGroup>())
                {
                    var package = conf.TileManager.CityModel.GetPackage(cog);
                    if (!package.CanAlignWithLand()) continue;
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
            }

            var nonLibDataHolder = new NonLibData.NonLibDataHolder(
                new GmlIdToSerializedCityObj(),
                new NameToAttrsDict(),
                new ContourMeshesMaker()
            );
            nonLibDataHolder.ComposeFrom(new[] { alignTarget, alignInvertTarget });

            // 土地の高さをC++に送ります。
            using var heightmapAligner = HeightMapAligner.Create(HeightOffset, CoordinateSystem.EUN);
            foreach (var m in heightmaps)
            {
                heightmapAligner.AddHeightmapFrame(m.HeightData, m.textureWidth, m.textureHeight, (float)m.min.X, (float)m.max.X, (float)m.min.Y, (float)m.max.Y, (float)m.min.Z, (float)m.max.Z, CoordinateSystem.EUN);
            }

            var resultTransforms = new UniqueParentTransformList();
            var alTargets = alignTarget.Get.ToArray();
            var alignTargetModels = new Model[alTargets.Length];
            var subMeshConverters = new GameMaterialIDRegistry[alTargets.Length];
            // 非土地の高さ合わせターゲットメッシュをC++のModelに変換します
            for (int i = 0; i < alTargets.Length; i++)
            {
                var target = alTargets[i];
                progressDisplay.SetProgress(target.name, ((float)i * 100) / alTargets.Length, "共通ライブラリのModelに変換中...");
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
                    progressDisplay.SetProgress(target.name, ((float)i * 100) / alInvertTargets.Length, "共通ライブラリのModelに変換中(alignInvert対象)...");
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
                for (int i = 0; i < landTransforms.Count; i++)
                {
                    var land = landTransforms[i];
                    var heightmap1d = heightmapAligner.GetHeightMapAt(i);

                    var terrain = land.GetComponent<Terrain>();

                    // テレイン出力ではない場合
                    if (terrain == null)
                    {
                        var meshFilter = land.GetComponent<MeshFilter>();
                        var smoothedDem = land.GetComponent<PLATEAUSmoothedDem>();
                        if (meshFilter == null || smoothedDem == null)
                        {
                            Debug.LogError("不正な平滑化済み地形モデル");
                            continue;
                        }

                        smoothedDem.HeightMapData.HeightData = heightmap1d;
                        var mesh = ConvertedTerrainData.ConvertToMesh(smoothedDem.HeightMapData, land.name);
                        meshFilter.sharedMesh = mesh;

                        continue;
                    }

                    var terrainData = terrain.terrainData;
                    var heightmap2d = HeightmapGenerator.ConvertTo2DFloatArray(heightmap1d, terrainData.heightmapResolution, terrainData.heightmapResolution);
                    terrain.terrainData.SetHeights(0, 0, heightmap2d);
                    resultTransforms.Add(land);
                }
            }
            
            if (conf.AlignLandNormal)
            {
                // 高さ合わせをします
                for (int i = 0; i < alTargets.Length; i++)
                {
                    var target = alTargets[i];
                    progressDisplay.SetProgress(target.name, ((float)i * 100) / alTargets.Length, "高さを変換中...");


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

                    resultTransforms.AddRange(result.GeneratedRootTransforms.Get);
                }

            }
            
            // 変換前の情報を復元します。
            nonLibDataHolder.RestoreTo(resultTransforms);

            // 復元後に元のオブジェクトを削除します。
            if (conf.AlignLandNormal && conf.DoDestroySrcObj)
            {
                foreach (var src in alignTarget.Get)
                {
                    Object.DestroyImmediate(src.gameObject);
                }
            }

#if UNITY_EDITOR
            var selected = Selection.objects.ToList();
            selected.AddRange(resultTransforms.Get.Select(trans => trans.gameObject).Cast<Object>());
            Selection.objects = selected.ToArray();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif

        }
    }
}
