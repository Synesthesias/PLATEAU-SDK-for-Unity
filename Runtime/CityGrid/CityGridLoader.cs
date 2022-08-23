using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.GeometryModel;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace PLATEAU.CityGrid
{
    /// <summary>
    /// 都市を指定数のグリッドに分割し、各グリッド内のメッシュを結合し、シーンに配置します。
    /// 
    /// </summary>
    internal class CityGridLoader : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField] private int gridCountOfSide = 10;
        
        // TODO Loadの実行中にまたLoadが実行されることを防ぐ仕組みが未実装
        // TODO 進捗を表示する機能と処理をキャンセルする機能が未実装
        /// <summary>
        /// GMLファイルをロードし、都市を指定数のグリッドに分割し、グリッド内のメッシュを結合し、シーンに配置します。
        /// 非同期処理です。必ずメインスレッドで呼ぶ必要があります。
        /// </summary>
        public async Task Load()
        {
            if (!AreMemberVariablesOK()) return;
            string gmlAbsolutePath = Application.streamingAssetsPath + "/" + this.gmlRelativePathFromStreamingAssets;

            using var meshMerger = new MeshExtractor();
            // ここの処理は 処理A と 処理B に分割されています。
            // Unityのメッシュデータを操作するのは 処理B のみであり、
            // 処理A はメッシュ構築のための準備(データを List, 配列などで保持する)を
            // するのみでメッシュデータは触らないこととしています。
            // なぜなら、メッシュデータを操作可能なのはメインスレッドのみなので、
            // 処理Aを別スレッドで実行してメインスレッドの負荷を減らすために必要だからです。

            // 処理A :
            // Unityでメッシュを作るためのデータを構築します。
            // 実際のメッシュデータを触らないので、Task.Run で別のスレッドで処理できます。
            var meshObjsData = await Task.Run( () =>
            {
                var plateauModel = LoadGmlAndMergePolygons(meshMerger, gmlAbsolutePath, this.gridCountOfSide);
                var meshObjsData = new ConvertedGameObjData(plateauModel);
                return meshObjsData;
            });

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能なので、Loadメソッドはメインスレッドから呼ぶ必要があります。
            // await PlaceGridMeshes(meshObjsData,
            //     GmlFileNameParser.FileNameWithoutExtension(this.gmlRelativePathFromStreamingAssets),
            //     gmlAbsolutePath);
            await meshObjsData.PlaceToScene(null, gmlAbsolutePath);
            
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
            if (this.gridCountOfSide <= 0)
            {
                Debug.LogError($"{nameof(this.gridCountOfSide)} の値を1以上にしてください");
                return false;
            }

            return true;
        }

        /// <summary>
        /// gmlファイルをパースして、得られた都市をグリッドに分けて、
        /// グリッドごとにメッシュを結合して、グリッドごとの<see cref="CityGML.Mesh"/> を配列で返します。
        /// メインスレッドでなくても動作します。
        /// </summary>
        private static Model LoadGmlAndMergePolygons(MeshExtractor meshExtractor, string gmlAbsolutePath, int numGridCountOfSide)
        {
            // GMLロード
            var cityModel = LoadCityModel(gmlAbsolutePath);
            // マージ
            var logger = new DllLogger();
            logger.SetLogCallbacks(DllLogCallback.UnityLogCallbacks);
            // var plateauMeshes = meshExtractor.GridMerge(cityModel, CityObjectType.COT_All, numGridX, numGridY, logger);
            var options = new MeshExtractOptions()
            {
                // TODO ReferencePointを正しく設定できるようにする
                ReferencePoint = new PlateauVector3d(0, 0, 0),
                MeshAxes = AxesConversion.WUN,
                MeshGranularity = MeshGranularity.PerCityModelArea,
                // TODO 選択できるようにする
                MaxLod = 2,
                MinLod = 2,
                ExportAppearance = true,
                GridCountOfSide = numGridCountOfSide
            };
            var model = meshExtractor.Extract(cityModel, options, logger);
            return model;
        }
        
        /// <summary> gmlファイルをパースして <see cref="CityModel"/> を返します。 </summary>
        private static CityModel LoadCityModel(string gmlAbsolutePath)
        {
            var parserParams = new CitygmlParserParams(true, true, false);
            return CityGml.Load(gmlAbsolutePath, parserParams, DllLogCallback.UnityLogCallbacks);
        }

        /// <summary>
        /// <see cref="ConvertedMeshData"/>(PlateauからUnity向けに変換したモデルデータ) をメッシュとして実体化してシーンに配置します。
        /// </summary>
        // private static async Task PlaceGridMeshes(ConvertedGameObjData meshObjData, string parentObjName, string gmlAbsolutePath)
        // {
        //     var parentTrans = GameObjectUtil.AssureGameObject(parentObjName).transform;
        //     foreach (var uMesh in unityMeshes)
        //     {
        //         await uMesh.PlaceToScene(parentTrans, gmlAbsolutePath);
        //     }
        // }
        
    }
}