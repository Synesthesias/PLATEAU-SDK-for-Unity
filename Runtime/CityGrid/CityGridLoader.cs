using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEngine;

namespace PLATEAU.CityGrid
{
    internal class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromAssets;
        [SerializeField] private int numGridX = 10;
        [SerializeField] private int numGridY = 10;

        #if UNITY_EDITOR
        public async Task Load()
        {
            Debug.Log($"GridLoader Start. threadID={Thread.CurrentThread.ManagedThreadId}");
            if (this.numGridX <= 0 || this.numGridY <= 0)
            {
                Debug.LogError("numGrid の値を1以上にしてください");
                return;
            }
            string gmlAbsolutePath = Application.dataPath + "/" + this.gmlRelativePathFromAssets;
            CitygmlParserParams parserParams = new CitygmlParserParams(true, true, false);
            var cityModel = CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
            using (var meshMerger = new MeshMerger())
            {
                var logger = new DllLogger();
                logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
                var plateauPolygons = meshMerger.GridMerge(cityModel, CityObjectType.COT_All, this.numGridX,
                    this.numGridY, logger);
                int numPolygons = plateauPolygons.Length;
                var unityMeshes = new UnityConvertedMesh[numPolygons];
                Debug.Log($"numPolygons = {numPolygons}");
                for (int i = 0; i < numPolygons; i++)
                {
                    // TODO もっと効率的な非同期方法があるはず
                    unityMeshes[i] = await PlateauPolygonConverter.Convert(plateauPolygons[i], gmlAbsolutePath);
                }

                PlaceGridMeshes(unityMeshes,
                    GmlFileNameParser.FileNameWithoutExtension(this.gmlRelativePathFromAssets));
            }
        }
        #endif

        private static void PlaceGridMeshes(UnityConvertedMesh[] unityMeshes, string parentObjName)
        {
            var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
            foreach (var uMesh in unityMeshes)
            {
                uMesh.PlaceToScene(parentTrans);
            }
        }
        
    }
}