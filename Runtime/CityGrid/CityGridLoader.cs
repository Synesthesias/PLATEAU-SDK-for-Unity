using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

using Texture = UnityEngine.Texture;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// 都市を指定数のグリッドに分割し、各グリッド内のメッシュを結合し、シーンに配置します。
    /// 
    /// </summary>
    internal class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField] private MeshGranularity meshGranularity = MeshGranularity.PerCityModelArea;
        [SerializeField, Tooltip("グリッド分けした時の一辺のグリッド数です。グリッドの数はこの数値の2乗になります。")] private int gridCountOfSide = 10;
        [SerializeField] private bool doExportAppearance = true;
        [SerializeField] private uint minLOD = 2;
        [SerializeField] private uint maxLOD = 2;
        [SerializeField, Tooltip("範囲指定（緯度）です。")] private double minLatitude = -90;
        [SerializeField] private double maxLatitude = 90;
        [SerializeField, Tooltip("範囲指定（経度）です。")] private double minLongitude = -180;
        [SerializeField] private double maxLongitude = 180;
        
        /// <summary>
        /// テクスチャパス と テクスチャを紐付ける辞書です。同じテクスチャが繰り返しロードされることを防ぎます。
        /// これがないと、異なるオブジェクトが同じテクスチャURLを指していても愚直にオブジェクトごとにテクスチャをロードしてシーンに保存することになるので、最小地物を扱うときのシーン容量がゴジラのごとくになります。
        /// </summary>
        private readonly Dictionary<string, Texture> cachedTexture = new Dictionary<string, Texture>();

        // TODO Loadの実行中にまたLoadが実行されることを防ぐ仕組みが未実装
        // TODO 進捗を表示する機能と処理をキャンセルする機能が未実装
        /// <summary>
        /// GMLファイルをロードし、都市を指定数のグリッドに分割し、グリッド内のメッシュを結合し、シーンに配置します。
        /// 非同期処理です。必ずメインスレッドで呼ぶ必要があります。
        /// </summary>
        public async Task Load()
        {
            if (!AreMemberVariablesOK()) return;
            Debug.Log("load started.");
            string gmlAbsolutePath = Application.streamingAssetsPath + "/" + this.gmlRelativePathFromStreamingAssets;

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
                    new GeoCoordinate(this.minLatitude, this.minLongitude, -9999),
                    new GeoCoordinate(this.maxLatitude, this.maxLongitude, 9999)); 
                using var plateauModel = LoadGmlAndMergeMeshes(gmlAbsolutePath,this.meshGranularity, this.gridCountOfSide,this.doExportAppearance, this.minLOD, this.maxLOD, extent);
                var convertedObjData = new ConvertedGameObjData(plateauModel);
                return convertedObjData;
            });

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能なので、Loadメソッドはメインスレッドから呼ぶ必要があります。
            await meshObjsData.PlaceToScene(null, gmlAbsolutePath, this.cachedTexture);

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

        private bool AreMemberVariablesOK()
        {
            if (this.gridCountOfSide <= 0)
            {
                Debug.LogError($"{nameof(this.gridCountOfSide)} の値を1以上にしてください");
                return false;
            }

            if (this.maxLOD < this.minLOD)
            {
                Debug.LogError($"{nameof(this.minLOD)}, {nameof(this.maxLOD)} は0以上であり、 min <= max である必要があります。");
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

        /// <summary> gmlファイルをパースして <see cref="CityModel"/> を返します。 </summary>
        private static CityModel LoadCityModel(string gmlAbsolutePath)
        {
            var parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }
    }
}