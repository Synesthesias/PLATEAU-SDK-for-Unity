using PLATEAU.CityAdjust.NonLibDataHolder;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.HeightMapAlign;
using PLATEAU.PolygonMesh;
using PLATEAU.TerrainConvert;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityAdjust.AlignLand
{
    public class AlignLandExecutor
    {
        public async Task ExecAsync(ALConfig conf, IProgressDisplay progressDisplay)
        {
            progressDisplay.SetProgress("", 0f, "ハイトマップをを作成中...");
            var landTrans = conf.Lands[0]; // TODO 複数対応

            List<ConvertedTerrainData.HeightmapData> heightmaps;
            
            // テレインの場合、テレインからハイトマップを取得します。
            if (landTrans.GetComponent<Terrain>() != null)
            {
                var terrain = landTrans.GetComponent<Terrain>();
                heightmaps = new List<ConvertedTerrainData.HeightmapData>{ConvertedTerrainData.HeightmapData.CreateFromTerrain(terrain)};
            }
            // メッシュの場合、メッシュからハイトマップを生成します。
            else if (landTrans.GetComponent<MeshRenderer>() != null)
            {
                heightmaps = CreateHeightMapFromMesh(landTrans, conf.HeightmapWidth);
                if (heightmaps == null)
                {
                    Dialogue.Display("ハイトマップの生成に失敗しました。", "OK");
                    return;
                }
            }
            else
            {
                Dialogue.Display("地形を取得できませんでした。", "OK");
                return;
            }

            // 高さを変えるモデルについて情報収集します。
            progressDisplay.SetProgress("", 0f, "処理対象の情報を収集中...");
            var convertTarget = new UniqueParentTransformList();
            foreach (var cog in conf.TargetModel.GetComponentsInChildren<PLATEAUCityObjectGroup>())
            {
                if (!conf.TargetPackages.Contains(cog.Package)) continue;
                var targetMf = cog.GetComponent<MeshFilter>();
                if (targetMf == null) continue;
                var targetMesh = targetMf.sharedMesh;
                if (targetMesh == null) continue;
                convertTarget.Add(cog.transform);
            }

            var nonLibDataHolder = new NonLibData.NonLibDataHolder(
                new GmlIdToSerializedCityObj()
            );
            nonLibDataHolder.ComposeFrom(convertTarget);

            // 土地の高さをC++に送ります。
            using var heightmapAligner = HeightMapAligner.Create();
            foreach (var m in heightmaps)
            {
                heightmapAligner.AddHeightmapFrame(m.HeightData, m.textureWidth, m.textureHeight, (float)m.min.X, (float)m.max.X, (float)m.min.Z, (float)m.max.Z, (float)m.min.Y, (float)m.max.Y);
            }
            
            
            var sumResult = new GranularityConvertResult();
            var allTargets = convertTarget.Get.ToArray();
            for (int i = 0; i < allTargets.Length; i++)
            {
                var target = allTargets[i];
                progressDisplay.SetProgress(target.name, ((float)i * 100)/allTargets.Length, "高さを変換中...");
                // C++のModelに変換します
                var subMeshConverter = new UnityMeshToDllSubMeshWithGameMaterial();
                var model = UnityMeshToDllModelConverter.Convert(
                    new UniqueParentTransformList(target),
                    subMeshConverter,
                    false,
                    VertexConverterFactory.NoopConverter());
            
                // 高さ合わせをします。
                heightmapAligner.Align(model);

                var result = await PlateauToUnityModelConverter.PlateauModelToScene(
                    null, new DummyProgressDisplay(), "",
                    new PlaceToSceneConfig(new DllSubMeshToUnityMaterialByGameMaterial(subMeshConverter), true, null, null,
                        new CityObjectGroupInfoForToolkits(false, false), MeshGranularity.PerPrimaryFeatureObject),
                    model,
                    new AttributeDataHelper(
                        new SerializedCityObjectGetterFromDict(nonLibDataHolder.Get<GmlIdToSerializedCityObj>(), model),
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
            
            // 変換前の情報を復元します。
            nonLibDataHolder.RestoreTo(sumResult.GeneratedRootTransforms);

            if (conf.DoDestroySrcObj)
            {
                foreach (var target in convertTarget.Get)
                {
                    Object.DestroyImmediate(target.gameObject);
                }
            }
            
            #if UNITY_EDITOR
            var selected = Selection.objects.ToList();
            selected.AddRange(sumResult.GeneratedRootTransforms.Get.Select(trans => trans.gameObject).Cast<Object>());
            Selection.objects = selected.ToArray();
            #endif
        }
        
        /// <summary>
        /// メッシュ形式の土地からハイトマップを生成します。
        /// 失敗時はnullを返します。
        /// </summary>
        private List<ConvertedTerrainData.HeightmapData> CreateHeightMapFromMesh(Transform landTrans, int heightmapWidth)
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
                new TerrainConvertOption(new GameObject[] { landTrans.gameObject }, heightmapWidth, false,
                    true, TerrainConvertOption.ImageOutput.PNG));
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