using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityGrid
{
    internal class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField] private int numGridX = 10;
        [SerializeField] private int numGridY = 10;
        
        // TODO Loadの実行中にまたLoadが実行されることを防ぐ仕組みが未実装
        // TODO 進捗を表示する機能と処理をキャンセルする機能が未実装
        public async Task Load()
        {
            if (!AreMemberVariablesOK()) return;
            string gmlAbsolutePath = Application.streamingAssetsPath + "/" + this.gmlRelativePathFromStreamingAssets;

            using var meshMerger = new MeshMerger();
            // ここの処理は 処理A と 処理B に分割されています。
            // Unityのメッシュデータを操作するのは 処理B のみであり、
            // 処理A はメッシュ構築のための準備(データを List, 配列などで保持する)を
            // するのみでメッシュデータは触らないこととしています。
            // なぜなら、メッシュデータを操作可能なのはメインスレッドのみなので、
            // 処理Aを別スレッドで実行するために必要だからです。

            // 処理A :
            // Unityでメッシュを作るためのデータを構築します。
            // 実際のメッシュデータを触らないので、Task.Run で別のスレッドで処理できます。
            var meshDataArray = await Task.Run( () =>
            {
                var plateauPolygons = LoadGmlAndMergePolygons(meshMerger, gmlAbsolutePath, this.numGridX, this.numGridY);
                var meshDataArray = ConvertToUnityMeshes(plateauPolygons);
                return meshDataArray;
            });

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能です。
            await PlaceGridMeshes(meshDataArray,
                GmlFileNameParser.FileNameWithoutExtension(this.gmlRelativePathFromStreamingAssets),
                gmlAbsolutePath);
            
            // エディター内での実行であれば、生成したメッシュ,テクスチャ等をシーンに保存したいので
            // シーンにダーティフラグを付けます。
            #if UNITY_EDITOR
            if (Application.isEditor)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            #endif
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
        
        /// <summary> gmlファイルをパースして <see cref="CityModel"/> を返します。 </summary>
        private static CityModel LoadCityModel(string gmlAbsolutePath)
        {
            var parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }

        /// <summary> <see cref="PlateauPolygon"/> の配列をUnityのメッシュに変換します。 </summary>
        private static ConvertedMeshData[] ConvertToUnityMeshes(IReadOnlyList<PlateauPolygon> plateauPolygons)
        {
            int numPolygons = plateauPolygons.Count;
            var meshDataArray = new ConvertedMeshData[numPolygons];
            for (int i = 0; i < numPolygons; i++)
            {
                meshDataArray[i] = PlateauPolygonConverter.Convert(plateauPolygons[i]);
            }

            return meshDataArray;
        }
        
        /// <summary>
        /// <see cref="ConvertedMeshData"/>(PlateauからUnity向けに変換したモデルデータ) をメッシュとして実体化してシーンに配置します。
        /// </summary>
        private static async Task PlaceGridMeshes(IEnumerable<ConvertedMeshData> unityMeshes, string parentObjName, string gmlAbsolutePath)
        {
            var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
            foreach (var uMesh in unityMeshes)
            {
                await uMesh.PlaceToScene(parentTrans, gmlAbsolutePath);
            }
        }
        
    }
}