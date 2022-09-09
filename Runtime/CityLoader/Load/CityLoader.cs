using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Texture = UnityEngine.Texture;

namespace PLATEAU.CityLoader.Load
{
    internal static class CityLoader
    {

        // TODO Loadの実行中にまたLoadが実行されることを防ぐ仕組みが未実装
        // TODO 進捗を表示する機能と処理をキャンセルする機能が未実装
        /// <summary>
        /// GMLファイルをロードし、都市を指定数のグリッドに分割し、グリッド内のメッシュを結合し、シーンに配置します。
        /// 非同期処理です。必ずメインスレッドで呼ぶ必要があります。
        /// </summary>
        public static async Task Load(
            string gmlRelativePathFromStreamingAssets,
            MeshGranularity meshGranularity,
            uint minLOD, uint maxLOD,
            bool doExportAppearance,
            int gridCountOfSide,
            double minLatitude, double minLongitude, double maxLatitude, double maxLongitude
            )
        {
            if (!AreArgumentsOK(gridCountOfSide, minLOD, maxLOD)) return;
            string gmlAbsolutePath = Application.streamingAssetsPath + "/" + gmlRelativePathFromStreamingAssets;
            if (!File.Exists(gmlAbsolutePath))
            {
                Debug.LogError($"File not found on {gmlAbsolutePath}");
                return;
            }
            Debug.Log("load started.");

            ConvertedGameObjData meshObjsData;
            // ここの処理は 処理A と 処理B に分割されています。
            // Unityのメッシュデータを操作するのは 処理B のみであり、
            // 処理A はメッシュ構築のための準備(データを List, 配列などで保持する)を
            // するのみでメッシュデータは触らないこととしています。
            // なぜなら、メッシュデータを操作可能なのはメインスレッドのみなので、
            // 処理Aを別スレッドで実行してメインスレッドの負荷を減らすために必要だからです。

            // 処理A :
            // Unityでメッシュを作るためのデータを構築します。
            // 実際のメッシュデータを触らないので、Task.Run で別のスレッドで処理できます。
            meshObjsData = await Task.Run(() =>
            {
                Extent extent = new Extent(
                    new GeoCoordinate(minLatitude, minLongitude, -9999),
                    new GeoCoordinate(maxLatitude, maxLongitude, 9999)); 
                using var plateauModel = LoadGmlAndMergeMeshes(gmlAbsolutePath,meshGranularity, gridCountOfSide, doExportAppearance, minLOD, maxLOD, extent);
                var convertedObjData = new ConvertedGameObjData(plateauModel);
                return convertedObjData;
            });

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能なので、Loadメソッドはメインスレッドから呼ぶ必要があります。
            
            // テクスチャパス と テクスチャを紐付ける辞書です。同じテクスチャが繰り返しロードされることを防ぎます。
            Dictionary<string, Texture> cachedTexture = new Dictionary<string, Texture>();
            
            await meshObjsData.PlaceToScene(null, gmlAbsolutePath, cachedTexture);

            // エディター内での実行であれば、生成したメッシュ,テクスチャ等をシーンに保存したいので
            // シーンにダーティフラグを付けます。
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
#endif

            Debug.Log("Load complete!");
        }

        private static bool AreArgumentsOK(int gridCountOfSide, uint minLOD, uint maxLOD)
        {
            if (gridCountOfSide <= 0)
            {
                Debug.LogError($"{nameof(gridCountOfSide)} の値を1以上にしてください");
                return false;
            }

            if (maxLOD < minLOD)
            {
                Debug.LogError($"{nameof(minLOD)}, {nameof(maxLOD)} は0以上であり、 min <= max である必要があります。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// gmlファイルをパースして、得られた都市をグリッドに分けて、
        /// グリッドごとにメッシュを結合して、グリッドごとの<see cref="GeometryModel.Model"/> で返します。
        /// メインスレッドでなくても動作します。
        /// </summary>
        private static Model LoadGmlAndMergeMeshes(
            string gmlAbsolutePath, MeshGranularity meshGranularity, int numGridCountOfSide,
            bool doExportAppearance, uint minLOD, uint maxLOD, Extent extent)
        {
            // GMLロード
            using var cityModel = LoadCityModel(gmlAbsolutePath);

            Debug.Log("gml loaded.");
            // マージ
            var options = new MeshExtractOptions()
            {
                // TODO ReferencePoint はユーザーが設定できるようにしたほうが良い
                ReferencePoint = cityModel.CenterPoint,
                MeshAxes = CoordinateSystem.WUN,
                MeshGranularity = meshGranularity,
                MaxLOD = maxLOD,
                MinLOD = minLOD,
                ExportAppearance = doExportAppearance,
                GridCountOfSide = numGridCountOfSide,
                UnitScale = 1f,
                Extent = extent
            };
            var model = new Model();
            MeshExtractor.Extract(ref model, cityModel, options);
            Debug.Log("model extracted.");
            return model;
        }

        /// <summary> gmlファイルをパースして <see cref="CityGML.CityModel"/> を返します。 </summary>
        private static CityModel LoadCityModel(string gmlAbsolutePath)
        {
            var parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }
    }
}
