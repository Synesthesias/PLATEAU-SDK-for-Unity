using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.PolygonMesh;
using PLATEAU.Dataset;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Material = UnityEngine.Material;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityImport.Load.Convert
{
    /// <summary>
    /// 都市の3Dモデルを PLATEAU から Unity に変換してシーンに配置します。
    /// </summary>
    internal static class PlateauToUnityModelConverter
    {
        // TODO 処理をキャンセルする機能が未実装
        /// <summary>
        /// 引数の cityModel を Unity向けに変換し、シーンに配置します。
        /// 非同期処理です。必ずメインスレッドで呼ぶ必要があります。
        /// 成否を bool で返します。
        /// </summary>
        public static async Task<ConvertResult> CityModelToScene(
            CityModel cityModel, MeshExtractOptions meshExtractOptions, string[] selectedMeshCodes,
            Transform parentTrans, IProgressDisplay progressDisplay, string progressName,
            bool doSetMeshCollider, bool doSetAttrInfo, CancellationToken token,  UnityEngine.Material fallbackMaterial
            )
        {
            Debug.Log($"load started");

            token.ThrowIfCancellationRequested();
            AttributeDataHelper attributeDataHelper =
                new AttributeDataHelper(new SerializedCityObjectGetterFromCityModel(cityModel), meshExtractOptions.MeshGranularity, doSetAttrInfo);

            Model plateauModel;
            try
            {
                plateauModel = await Task.Run(() => ExtractMeshes(cityModel, meshExtractOptions, selectedMeshCodes, token));
            }
            catch (Exception e)
            {
                Debug.LogError("メッシュデータの抽出に失敗しました。\n" + e);
                return ConvertResult.Fail();
            }

            return await PlateauModelToScene(parentTrans, progressDisplay, progressName, doSetMeshCollider, token, fallbackMaterial, plateauModel, attributeDataHelper, true);
        }

        /// <summary>
        /// 共通ライブラリのModelをUnityのゲームオブジェクトに変換してシーンに配置します。
        /// これにより配置されたゲームオブジェクトを引数 <paramref name="outGeneratedObjs"/> に追加します。
        /// </summary>
        public static async Task<ConvertResult> PlateauModelToScene(Transform parentTrans, IProgressDisplay progressDisplay,
            string progressName, bool doSetMeshCollider, CancellationToken? token, Material fallbackMaterial, Model plateauModel, 
            AttributeDataHelper attributeDataHelper, bool skipRoot)
        {
            // ここの処理は 処理A と 処理B に分割されています。
            // Unityのメッシュデータを操作するのは 処理B のみであり、
            // 処理A はメッシュ構築のための準備(データを List, 配列などで保持する)を
            // するのみでメッシュデータは触らないこととしています。
            // なぜなら、メッシュデータを操作可能なのはメインスレッドのみなので、
            // 処理Aを別スレッドで実行してメインスレッドの負荷を減らしたいためです。

            // 処理A :
            // Unityでメッシュを作るためのデータを構築します。
            // 実際のメッシュデータを触らないので、Task.Run で別のスレッドで処理できます。
            progressDisplay.SetProgress(progressName, 60f, "3Dメッシュを変換中");
            ConvertedGameObjData meshObjsData;
            try
            {
                meshObjsData = await Task.Run(() =>
                {
                    var convertedObjData = new ConvertedGameObjData(plateauModel, attributeDataHelper);
                    return convertedObjData;
                });
            }
            catch (Exception e)
            {
                Debug.LogError("メッシュデータの取得に失敗しました。\n" + e);
                return ConvertResult.Fail();
            }

            // 処理B :
            // 実際にメッシュを操作してシーンに配置します。
            // こちらはメインスレッドでのみ実行可能なので、Loadメソッドはメインスレッドから呼ぶ必要があります。

            //GMLマテリアル、 テクスチャパス と マテリアルを紐付ける辞書です。同じマテリアルが重複して生成されることを防ぎます。
            Dictionary<MaterialSet, UnityEngine.Material> cachedMaterials = new Dictionary<MaterialSet, UnityEngine.Material>();

            progressDisplay.SetProgress(progressName, 80f, "シーンに配置中");

            var result = new ConvertResult();

            var innerResult = await meshObjsData.PlaceToScene(parentTrans, cachedMaterials, skipRoot, doSetMeshCollider,
                token, fallbackMaterial);
            if (innerResult.IsSucceed)
            {
                result.Merge(innerResult);
            }
            else
            {
                Debug.LogError("メッシュデータの配置に失敗しました。");
                return ConvertResult.Fail();
            }
            
            

            // エディター内での実行であれば、生成したメッシュ,テクスチャ等をシーンに保存したいので
            // シーンにダーティフラグを付けます。
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
#endif
            Debug.Log("Gml model placed.");
            return result;
        }

        /// <summary>
        /// 都市モデルを指定のメッシュ結合単位に分けて、
        /// メッシュを結合して、各メッシュとその名称を含んだ<see cref="Model"/> を返します。
        /// メインスレッドでなくても動作します。
        /// </summary>
        private static Model ExtractMeshes(
            CityModel cityModel, MeshExtractOptions meshExtractOptions, string[] selectedMeshCodes, CancellationToken token)
        {
            var model = Model.Create();
            if (cityModel == null) return model;
            var extents = selectedMeshCodes.Select(code => {
                var extent = MeshCode.Parse(code).Extent;
                extent.Min.Height = -999999.0;
                extent.Max.Height = 999999.0;
                return extent;
            }).ToList();
            MeshExtractor.ExtractInExtents(ref model, cityModel, meshExtractOptions, extents);
            Debug.Log("model extracted.");
            return model;
        }

        /// <summary>
        /// 変換してシーンに配置した結果です。
        /// </summary>
        public class ConvertResult
        {
            public bool IsSucceed { get; set; } = true;

            /// <summary> 変換によってシーンに配置したゲームオブジェクトのリスト </summary>
            public List<GameObject> GeneratedObjs { get; } = new ();
            
            /// <summary>
            /// 変換によってシーンに配置したゲームオブジェクトのうち、ヒエラルキーが最上位であるもののリスト
            /// </summary>
            public List<GameObject> RootObjs { get; } = new();

            /// <summary> 結果のゲームオブジェクトの一覧に追加します。</summary>
            public void Add(GameObject obj, bool isRoot)
            {
                GeneratedObjs.Add(obj);
                if(isRoot) RootObjs.Add(obj);
            }

            /// <summary> 複数の<see cref="ConvertResult"/>を統合します。 </summary>
            public void Merge(ConvertResult other)
            {
                IsSucceed &= other.IsSucceed;
                GeneratedObjs.AddRange(other.GeneratedObjs);
                RootObjs.AddRange(other.RootObjs);
            }

            public static ConvertResult Fail()
            {
                return new ConvertResult() { IsSucceed = false };
            }
            
        }
    }
}
