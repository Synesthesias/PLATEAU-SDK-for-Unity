﻿using PLATEAU.CityGML;
using PLATEAU.GeometryModel;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// 都市を指定数のグリッドに分割し、各グリッド内のメッシュを結合し、シーンに配置します。
    /// 
    /// </summary>
    internal class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField, Tooltip("グリッド分けした時の一辺のグリッド数です。グリッドの数はこの数値の2乗になります。")] private int gridCountOfSide = 10;
        [SerializeField] private bool doExportAppearance = true;
        [SerializeField] private int minLOD = 2;
        [SerializeField] private int maxLOD = 2;

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
                using var meshExtractor = new MeshExtractor();
                using var plateauModel = LoadGmlAndMergeMeshes(meshExtractor, gmlAbsolutePath, this.gridCountOfSide,this.doExportAppearance, this.minLOD, this.maxLOD);
                var convertedObjData = new ConvertedGameObjData(plateauModel);
                return convertedObjData;
            });

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能なので、Loadメソッドはメインスレッドから呼ぶ必要があります。
            await meshObjsData.PlaceToScene(null, gmlAbsolutePath);

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

            if (this.minLOD < 0 || this.maxLOD < 0 || this.maxLOD < this.minLOD)
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
        private static Model LoadGmlAndMergeMeshes(MeshExtractor meshExtractor, string gmlAbsolutePath,
            int numGridCountOfSide, bool doExportAppearance, int minLOD, int maxLOD)
        {
            // GMLロード
            using var cityModel = LoadCityModel(gmlAbsolutePath);

            Debug.Log("gml loaded.");
            // マージ
            var options = new MeshExtractOptions()
            {
                // TODO ReferencePoint はユーザーが設定できるようにしたほうが良い
                ReferencePoint = cityModel.CenterPoint,
                MeshAxes = AxesConversion.WUN,
                MeshGranularity = MeshGranularity.PerCityModelArea,
                MaxLOD = minLOD,
                MinLOD = maxLOD,
                ExportAppearance = doExportAppearance,
                GridCountOfSide = numGridCountOfSide,
                UnitScale = 1f
            };
            var model = meshExtractor.Extract(cityModel, options);
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