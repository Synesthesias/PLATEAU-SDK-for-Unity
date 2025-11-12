using PLATEAU.CityAdjust.AlignLand;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common.Tile;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using PLATEAU.TerrainConvert;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.TerrainConvertGui
{
    internal class TileTerrainConvert:IDisposable
    {
        private CityTerrainConvertParam localParam;
        private TileListElementData tileListElementData;
        private EditorWindow parentEditorWindow;
        private TileRebuilder tileRebuilder;
        private Action ConversionFinishCallback;

        /// <summary>
        /// 変換対象の都市モデルに土地が含まれているかチェック
        /// </summary>
        public bool IsReliefSelected
        {
            get
            {
                if (tileListElementData.TileManager == null) return false;
                var selectedTiles = TileConvertCommon.GetSelectedTiles(tileListElementData.ObservableSelectedTiles, tileListElementData.TileManager);
                var hasRelief = selectedTiles.Any(x => x.Package == Dataset.PredefinedCityModelPackage.Relief); // 選択Tileが地形を含むか
                return hasRelief;
            }
        }

        /// <summary>
        /// 土地をLOD3道路に合わせる機能を利用するには、平滑化された土地モデルが必要
        /// </summary>
        public bool IsAlignLandToLod3RoadNotWorking
        {
            get
            {
                if (tileListElementData.TileManager == null) return true;
                return !(localParam.AlignLandInvert && !IsReliefSelected && (!localParam.ConvertToTerrain));
            }
        }

        public TileTerrainConvert(CityTerrainConvertParam param, TileListElementData tileListData, EditorWindow parent)
        {
            localParam = param;
            tileListElementData = tileListData;
            parentEditorWindow = parent;
        }

        public void SetConversionFinishCallback(Action conversionFinishCallback)
        {
            ConversionFinishCallback = conversionFinishCallback;
        }

        public void Dispose()
        {
            ConversionFinishCallback = null;
        }

        public void Repaint()
        {
            parentEditorWindow?.Repaint();
        }

        public async Task Exec()
        {
            if (!CanExecOrNotify()) return;

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    var ct = cts.Token;

                    var doAlignLand = (localParam.AlignLand || ((!localParam.AlignLandNormal) && (!localParam.AlignLandInvert))); // 高さ合わせを実行するか
                    var selectedTiles = TileConvertCommon.GetSelectedTiles(tileListElementData.ObservableSelectedTiles, tileListElementData.TileManager);

                    tileRebuilder = new TileRebuilder();
                    var editingTile = await TileConvertCommon.GetEditableTransformParent(tileListElementData.ObservableSelectedTiles, tileListElementData.TileManager, tileRebuilder, ct);
                    var tileTransforms = TileConvertCommon.GetEditableTransforms(tileListElementData.ObservableSelectedTiles, editingTile);

                    // Terrainに変換します
                    if (localParam.ConvertToTerrain)
                    {
                        var converter = new CityTerrainConverter();
                        var convertOption = new TerrainConvertOption(
                            GetDemGameObjectsFromTransforms(tileTransforms),
                            localParam.GetHeightmapWidth(),
                            localParam.PreserveOrDestroy == PreserveOrDestroy.Destroy,
                            localParam.FillEdges,
                            localParam.ApplyConvolutionFilterToHeightMap,
                            localParam.EnableTerrainConversion,
                            CityTerrainConvertParam.HEIGHTMAP_IMAGE_OUTPUT
                        );

                        var landResult = await converter.ConvertAsync(convertOption);
                        if (landResult != null && landResult.IsSucceed)
                        {
                            tileTransforms = TileConvertCommon.GetEditableTransforms(tileListElementData.ObservableSelectedTiles, editingTile); // 取得し直す
                            var generatedTrans = landResult.GeneratedObjs.Distinct().Select(x => x.transform).ToList();
                            tileTransforms.AddRange(generatedTrans); // 生成された土地をリストに追加
                        }
                    }

                    // 高さ合わせを実行します
                    if (doAlignLand)
                    {
                        await Task.Yield();
                        await Task.Delay(100);

                        var alignTransforms = GetAlignTransforms(selectedTiles, tileTransforms);
                        var landTransforms = GetLandTransforms(selectedTiles, tileTransforms);
                        await ExecAlignLand(selectedTiles, alignTransforms, landTransforms);
                    }

                    await TileConvertCommon.SavePrefabAssets(tileTransforms, tileRebuilder, ct);
                    await tileRebuilder.RebuildByTiles(tileListElementData.TileManager, selectedTiles);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    tileRebuilder?.CancelRebuild();
                }
            }

            ConversionFinishCallback?.Invoke();
        }

        // 地形のタイルTransform取得( 地形の場合は、Tileの下層は扱わない　）
        private List<Transform> GetLandTransforms(List<PLATEAUDynamicTile> tiles, List<Transform> trans)
        {
            // 地形取得
            var landTileAddrs = tiles.Where(x => x.Package == PredefinedCityModelPackage.Relief).Select(x => x.Address).ToList();
            var landTileTransforms = trans.Where(t => t != null && landTileAddrs.Contains(t.FindChildOfNamedParent(TileRebuilder.EditingTilesParentName)?.name)).Distinct().ToList(); // LandTileの子要素をフィルタ

            // Tileの子要素からMeshRendererかTerrainのコンポーネントを持つ子を抽出
            var landTransformsHash = new HashSet<Transform>();
            foreach (var t in landTileTransforms)
            {
                // Mesh と TerrainのComponentを持つ子要素を取得
                var meshes = TransformEx.GetAllChildrenWithComponent<PLATEAUSmoothedDem>(t);
                foreach (var item in meshes)
                    landTransformsHash.Add(item);
                var terrains = TransformEx.GetAllChildrenWithComponent<Terrain>(t);
                foreach (var item in terrains)
                    landTransformsHash.Add(item);
            }
            var landTransforms = landTransformsHash.ToList();

            return landTransforms;
        }

        // 高さ合わせ用のTransform取得
        private List<Transform> GetAlignTransforms(List<PLATEAUDynamicTile> tiles, List<Transform> trans)
        {
            var alignTileAddrs = tiles.Where(x => x.Package != PredefinedCityModelPackage.Relief && x.Package.CanAlignWithLand()).Select(x => x.Address).ToList(); //高さ合わせ可能なTileのみ
            var alignTileTransforms = trans.Where(t => t != null && alignTileAddrs.Contains(t.FindChildOfNamedParent(TileRebuilder.EditingTilesParentName)?.name)).Distinct().ToList(); // 高さ合わせ可能なTileの子要素をフィルタ
            var alignTransforms = new List<Transform>();
            foreach (var t in alignTileTransforms)
            {
                alignTransforms.AddRange(TransformEx.GetAllChildrenWithComponent<PLATEAUCityObjectGroup>(t)); // PLATEAUCityObjectGroupを持つ子をリストに追加
            }
            return alignTransforms;
        }

        /// <summary>
        /// 高さ合わせ実行
        /// </summary>
        /// <param name="selectedTiles">選択されたDynamicTile</param>
        /// <param name="alignTransforms">変換対象のTransformリスト(editingTile下の選択されたTransform)</param>
        /// <returns></returns>
        private async Task ExecAlignLand(List<PLATEAUDynamicTile> selectedTiles , List<Transform> alignTransforms, List<Transform> landTransforms)
        {
            if( alignTransforms.Count <= 0)
            {
                Dialogue.Display("高さ合わせ可能なタイルが選択されていません。", "OK");
                return;
            }

            if (landTransforms.Count <= 0)
            {
                Dialogue.Display("変換対象のタイルに土地が含まれていません。", "OK");
                return;
            }

            int heightmapWidth = localParam.GetHeightmapWidth();
            bool doDestroySrcObj = localParam.PreserveOrDestroy == PreserveOrDestroy.Destroy;
            bool fillEdges = localParam.FillEdges; 
            bool applyConvolutionFilterToHeightMap = localParam.ApplyConvolutionFilterToHeightMap; 
            bool alignLandNormal = localParam.AlignLandNormal; 
            bool alignInvert = localParam.AlignLandInvert;

            var tileManager = tileListElementData.TileManager;
            ALTileConfig conf = new ALTileConfig(tileManager, alignTransforms, landTransforms, doDestroySrcObj, heightmapWidth, fillEdges, applyConvolutionFilterToHeightMap, alignLandNormal, alignInvert);

            // 高さ合わせを実行します
            using var progressDisplay = new ProgressDisplayDialogue();
            await new TileAlignLandExecutor().ExecAsync(conf, progressDisplay);
        }

        public bool CanExecOrNotify()
        {
            if (tileListElementData.TileManager == null || tileListElementData.ObservableSelectedTiles.Count <= 0)
            {
                Dialogue.Display("変換対象のタイルが選択されていません。", "OK");
                return false;
            }
            if (!(localParam.AlignLand || localParam.ConvertToTerrain))
            {
                Dialogue.Display("処理がすべてオフになっています。処理を選択してください。", "OK");
                return false;
            }
            if (!IsReliefSelected)
            {
                Dialogue.Display("変換対象のタイルに土地が含まれていません。", "OK");
                return false;
            } 
            return true;
        }

        private GameObject[] GetDemGameObjectsFromTransforms(List<Transform> list)
        {
            var gameObjects = list.Select(x => x?.gameObject).ToArray();
            var selectionList = new HashSet<GameObject>();
            foreach (var gameObj in gameObjects)
            {
                var components = gameObj.transform.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var grp in components)
                {
                    if (grp.CityObjects.rootCityObjects.Exists(x => x.type == CityGML.CityObjectType.COT_TINRelief))
                        selectionList.Add(grp.gameObject);
                }
            }
            return selectionList.ToArray();
        }
    }
}
