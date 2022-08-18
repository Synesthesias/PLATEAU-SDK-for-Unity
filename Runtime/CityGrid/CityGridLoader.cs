using System.Collections.Generic;
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
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField] private int numGridX = 10;
        [SerializeField] private int numGridY = 10;
        
        public async Task Load()
        {
            if (!AreMemberVariablesOK()) return;
            string gmlAbsolutePath = Application.streamingAssetsPath + "/" + this.gmlRelativePathFromStreamingAssets;
            
            using (var meshMerger = new MeshMerger())
            {
                // この処理は別スレッドで可能
                var plateauPolygons = await Task.Run(() => LoadGmlAndMergePolygons(meshMerger, gmlAbsolutePath, this.numGridX, this.numGridY));
                
                // この処理はメインスレッドでのみ可能
                var unityMeshes = await ConvertToUnityMeshes(plateauPolygons, gmlAbsolutePath);

                PlaceGridMeshes(unityMeshes,
                    GmlFileNameParser.FileNameWithoutExtension(this.gmlRelativePathFromStreamingAssets));
            }
        }

        private bool AreMemberVariablesOK()
        {
            if (this.numGridX <= 0 || this.numGridY <= 0)
            {
                Debug.LogError("numGrid の値を1以上にしてください");
                return false;
            }

            return true;
        }

        /// <summary> gmlファイルをパースして <see cref="CityModel"/> を返します。 </summary>
        private static CityModel LoadCityModel(string gmlAbsolutePath)
        {
            CitygmlParserParams parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }

        /// <summary>
        /// gmlファイルをパースして、得られた都市をグリッドに分けて、
        /// グリッドごとにメッシュを結合して、グリッドごとの<see cref="PlateauPolygon"/> を配列で返します。
        /// メインスレッドでなくても動作します。
        /// </summary>
        private static PlateauPolygon[] LoadGmlAndMergePolygons(MeshMerger meshMerger, string gmlAbsolutePath, int numGridX, int numGridY)
        {
            var cityModel = LoadCityModel(gmlAbsolutePath);
            var logger = new DllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            var plateauPolygons = meshMerger.GridMerge(cityModel, CityObjectType.COT_All, numGridX, numGridY, logger);
            return plateauPolygons;
        }

        /// <summary> <see cref="PlateauPolygon"/> の配列をUnityのメッシュに変換します。 </summary>
        private static async Task<UnityConvertedMesh[]> ConvertToUnityMeshes(IReadOnlyList<PlateauPolygon> plateauPolygons, string gmlAbsolutePath)
        {
            int numPolygons = plateauPolygons.Count;
            var unityMeshes = new UnityConvertedMesh[numPolygons];
            for (int i = 0; i < numPolygons; i++)
            {
                unityMeshes[i] = await PlateauPolygonConverter.Convert(plateauPolygons[i], gmlAbsolutePath);
            }

            return unityMeshes;
        }
        
        /// <summary>
        /// <see cref="UnityConvertedMesh"/>(PlateauからUnityに変換したモデルデータ) をシーンに配置します。
        /// </summary>
        private static void PlaceGridMeshes(IEnumerable<UnityConvertedMesh> unityMeshes, string parentObjName)
        {
            var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
            foreach (var uMesh in unityMeshes)
            {
                uMesh.PlaceToScene(parentTrans);
            }
        }
        
    }
}