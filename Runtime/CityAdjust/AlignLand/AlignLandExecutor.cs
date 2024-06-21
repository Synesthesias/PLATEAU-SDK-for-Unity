using PLATEAU.CityAdjust.NonLibData;
using PLATEAU.CityAdjust.NonLibDataHolder;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityImport.Import.Convert.MaterialConvert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.HeightMapAlign;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.TerrainConvert;
using PLATEAU.Texture;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEngine;

namespace PLATEAU.CityAdjust.AlignLand
{
    public class AlignLandExecutor
    {
        public async Task ExecAsync(ALConfig conf)
        {
            // 高さの基準となる土地からC++のModelとハイトマップを生成します。
            var landTrans = conf.Lands[0]; // TODO 複数対応
            var landMf = landTrans.GetComponent<MeshFilter>();
            if (landMf == null) return;
            var landMesh = landMf.sharedMesh;
            if (landMesh == null) return;
            var landModel = UnityMeshToDllModelConverter.Convert(
                new UniqueParentTransformList(landTrans),
                new UnityMeshToDllSubMeshWithEmptyMaterial(),
                false,
                VertexConverterFactory.NoopConverter());
            var terrain = new ConvertedTerrainData(
                landModel,
                new TerrainConvertOption(new GameObject[] { landTrans.gameObject }, 1024, false,
                    true, TerrainConvertOption.ImageOutput.PNG));
            var heightmaps = terrain.GetHeightmapDataRecursive();
            if (heightmaps.Count == 0) return;

            // 土地に合わせるモデルについて
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

            // C++のModelに変換します
            var subMeshConverter = new UnityMeshToDllSubMeshWithGameMaterial();
            var model = UnityMeshToDllModelConverter.Convert(
                convertTarget,
                subMeshConverter,
                false,
                VertexConverterFactory.NoopConverter());
            var map = heightmaps[0]; // TODO 複数対応
            
            // 地形の情報を集めます。
            var landVerts = landMesh.vertices;
            var landMinMax = new MinMax3d();
            for (int i = 0; i < landVerts.Length; i++)
            {
                var vert = landVerts[i];
                landMinMax.Include(vert);
            }
            
            // 高さ合わせをします。
            new HeightMapAligner().Align(model, map.HeightData, map.textureWidth, map.textureHeight, landMinMax.Min.x, landMinMax.Max.x, landMinMax.Min.z, landMinMax.Max.z, landMinMax.Min.y, landMinMax.Max.y);

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
            nonLibDataHolder.RestoreTo(result.GeneratedRootTransforms);
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