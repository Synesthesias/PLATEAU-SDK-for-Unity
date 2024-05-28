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
                new TerrainConvertOption(new GameObject[] { landTrans.gameObject }, 512, false,
                    TerrainConvertOption.ImageOutput.PNG));
            var heightmaps = terrain.GetHeightmapDataRecursive();
            if (heightmaps.Count == 0) return;

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

            var model = UnityMeshToDllModelConverter.Convert(
                convertTarget,
                new UnityMeshToDllSubMeshWithEmptyMaterial(),
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
            
            new HeightMapAligner().Align(model, map.HeightData, map.textureWidth, map.textureHeight, landMinMax.Min.x, landMinMax.Max.x, landMinMax.Min.z, landMinMax.Max.z, landMinMax.Min.y, landMinMax.Max.y);

            await PlateauToUnityModelConverter.PlateauModelToScene(
                null, new DummyProgressDisplay(), "",
                new PlaceToSceneConfig(new DllSubMeshToUnityMaterialByTextureMaterial(), true, null, null,
                    new CityObjectGroupInfoForToolkits(false, false), MeshGranularity.PerPrimaryFeatureObject),
                model,
                new AttributeDataHelper(new SerializedCityObjectGetterFromDict(new GmlIdToSerializedCityObj(), model), MeshGranularity.PerPrimaryFeatureObject, false)
                , true
            );
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