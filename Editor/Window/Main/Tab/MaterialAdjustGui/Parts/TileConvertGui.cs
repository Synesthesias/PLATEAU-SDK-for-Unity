using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    internal class TileSelectionItem
    {
        public string TileAddress { get; private set; } 
        public string TilePath { get; private set; }

        public bool IsTile => string.Equals(TileAddress, TilePath);

        public TileSelectionItem(string fullpath)
        {
            TilePath = fullpath;
            // path( 例: "xxxxx_op/LODx/xxxx-xxx") からタイルのAddress(例: "xxxxx_op")を取得する
            var splits = fullpath.Split('/');
            TileAddress =  splits[0];
        }
    }


    /// <summary>
    /// <see cref="CityMaterialAdjustPresenter"/>内のタイル選択GUI部分を担当するクラス
    /// </summary>
    internal class TileConvertGui
    {
        public bool LockChange { get; set; }

        private PLATEAUTileManager tileManager;

        private ObservableCollection<TileSelectionItem> observableSelected = new();

        private readonly EditorWindow parentEditorWindow;

        public ObservableCollection<TileSelectionItem> Selected => observableSelected;

        public bool HasTileManager => this.tileManager != null;

        public TileConvertGui(EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            SelectTileManager();
        }

        public void SelectTileManager()
        {
            this.tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>(); //デフォルトでシーン内のTileManagerをセット
        }

        public void Reset()
        {
        }

        public void ClearSelected()
        {
            observableSelected.Clear();
        }

        public void DrawSelectTile()
        {
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("調整対象:", GUILayout.Width(60));
                this.tileManager = (PLATEAUTileManager)EditorGUILayout.ObjectField(this.tileManager, typeof(PLATEAUTileManager), true);
            }

            GUI.enabled = (this.tileManager != null);
            PlateauEditorStyle.CenterAlignHorizontal(() =>
            {
                if (PlateauEditorStyle.MiniButton("タイル選択", 150))
                {
                    var window = TileSelectWindow.Open(this.tileManager, (result) =>
                    {

                        foreach (var name in result.Selection)
                        {
                            //if (observableSelected.Contains(name)) continue;
                            if (observableSelected.Any(x => x.TilePath == name)) continue;
                            observableSelected.Add(new TileSelectionItem(name));

                            Debug.Log($"Selected Tile: {name}");
                        }

                        foreach (var name in result.ChildSelection)
                        {

                            if (observableSelected.Any(x => x.TilePath == name)) continue;
                            observableSelected.Add(new TileSelectionItem(name));

                            Debug.Log($"Selected Child Tile: {name}");
                        }
                    });
                }
            });
            GUI.enabled = true;
        }

        public void DrawScrollViewContent(ScrollView scrollView)
        {
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollView.Draw(() =>
                {
                    if (observableSelected.Count == 0)
                    {
                        EditorGUILayout.LabelField("（未選択）");
                    }

                    int indexToDelete = -1;
                    bool deleteByUserInput = false;
                    // 各選択オブジェクトのスロットを描画
                    for (var i = 0; i < observableSelected.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"{i + 1}:", GUILayout.Width(30));
                            var item = observableSelected[i];
                            if (item == null)
                            {
                                indexToDelete = i;
                                continue;
                            }
                            EditorGUILayout.LabelField(item.TilePath, GUILayout.ExpandWidth(true));

                            if (PlateauEditorStyle.TinyButton("除く", 30))
                            {
                                indexToDelete = i;
                                deleteByUserInput = true;
                            }
                        }

                    }
                    // 削除ボタンが押された時
                    if (indexToDelete >= 0 && deleteByUserInput && AskUnlock())
                    {
                        observableSelected.RemoveAt(indexToDelete);
                    }
                });// end scrollView

                PlateauEditorStyle.RightAlign(() =>
                {
                    if (PlateauEditorStyle.TinyButton("全て除く", 75))
                    {
                        observableSelected.Clear();
                    }
                    GUI.enabled = !IsHighlightSelectedTilesRunning;
                    if (PlateauEditorStyle.TinyButton("対象をヒエラルキー上でハイライト", 180))
                    {
                        HighlightSelectedTiles().ContinueWithErrorCatch();
                    }
                    GUI.enabled = true;
                });
            }
        }

        private bool IsHighlightSelectedTilesRunning = false;
        private async Task HighlightSelectedTiles(bool forceLoad = true)
        {
            IsHighlightSelectedTilesRunning = true;

            var transforms = new List<Transform>();
            if (forceLoad)
            {
                var uniqueTransforms = await GetSelectionAsTransformList(); // 読み込み
                transforms = uniqueTransforms.Get.ToList();
            }
            else
            {
                transforms = GetTransformsFromTiles(GetSelectedTiles());
            }

            Selection.objects = transforms.Select(trans => trans.gameObject).Cast<Object>().ToArray();
            IsHighlightSelectedTilesRunning = false;

            parentEditorWindow.Repaint();
        }

        /// <summary>
        /// observableSelectedのAddressに含まれるタイルを読み込んで、Transformリストを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<UniqueParentTransformList> GetSelectionAsTransformList()
        {
            return await GetTransformListFromAddr(observableSelected);
        }

        /// <summary>
        /// Addressに含まれるタイルを読み込んで、Transformリストを取得する
        /// </summary>
        /// <returns></returns>
        public async Task<UniqueParentTransformList> GetTransformListFromAddr(IList<TileSelectionItem> addr)
        {
            CancellationTokenSource cancellationTokenSource = new();
            await tileManager.CancelLoadTask();

            bool loading = false;
            var selectedTiles = new List<PLATEAUDynamicTile>();
            foreach (var tile in tileManager.DynamicTiles)
            {
                //if (addr.Contains(tile.Address))
                if (addr.Any(x => x.TileAddress == tile.Address))
                    selectedTiles.Add(tile);

                if(tile.LoadedObject == null)
                {
                    // 読み込む
                    loading = true;
                    var tokenSorce = new CancellationTokenSource();
                    tile.LoadHandleCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, tokenSorce.Token);
                    var result = await tileManager.PrepareLoadTile(tile);
                }
            }

            if(loading)
            {
                // 読み込みまで待機
                var flagEvent = new ManualResetEventSlim(false);
                await Task.Run(() =>
                {
                    Thread.Sleep(500);
                    if (tileManager.IsCoroutineRunning == false)
                        flagEvent.Set(); 
                });
                flagEvent.Wait();
            }

            var transforms = new List<Transform>();
            foreach (var tile in selectedTiles)
            {
                if (tile.LoadedObject != null)
                {
                    TransformEx.GetAllChildrenWithComponent<PLATEAUCityObjectGroup>(tile.LoadedObject.transform).ForEach(t => transforms.Add(t));
                }
            }
            return new UniqueParentTransformList(transforms);
        }

        public async Task<GranularityConvertResult> ExecGranularityConvertAsync(MAExecutorConf conf, ConvertGranularity granularity)
        {
            if (observableSelected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var ct = cts.Token;
            var selectedTiles = GetSelectedTiles();
            var rebuilder = new TileRebuilder();
            var editingTile = await GetEditableTransformParent(rebuilder, ct);
            var tileTransforms = GetEditableTransforms(editingTile);
            conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

            // ここで実行します。
            var result = await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(granularity, 1), conf.TargetTransforms, conf.DoDestroySrcObjs));
            var generatedObjNames = result.GeneratedObjs.Select(x => x.name).ToList(); // 生成されたオブジェクトの名前リストを取得

            //　入れ直し 参照が切れるので
            tileTransforms.Clear();
            tileTransforms = GetEditableTransforms(editingTile, true);

            await SavePrefabAssets(tileTransforms, rebuilder, ct);
            await rebuilder.RebuildByTiles(tileManager, selectedTiles);

            Debug.Log($"粒度変換が完了しました。変換結果: 成功={result.IsSucceed}, 生成オブジェクト数={result.GeneratedRootTransforms.Count}");

            await HighlightSelectedTiles(false);
            SelectTileManager();
            return result;
        }

        public async Task<UniqueParentTransformList> ExecMaterialAdjustAsync(MAExecutorConf conf, IMAExecutorV2 maExecutor )//, bool destroySrcObjs)
        {
            //上の処理と割と一緒
            if (observableSelected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return new UniqueParentTransformList();
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var ct = cts.Token;

            var selectedTiles = GetSelectedTiles();
            var rebuilder = new TileRebuilder();

            var editingTile = await GetEditableTransformParent(rebuilder, ct);
            var tileTransforms = GetEditableTransforms(editingTile);
            conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

            // ここで実行します。
            var result = await maExecutor.ExecAsync(conf);
            var generatedObjNames = result.Get.Select(t => t.name).ToList();

            //　入れ直し 参照が切れるので
            tileTransforms.Clear();
            tileTransforms = GetEditableTransforms(editingTile, true);

            await SavePrefabAssets(tileTransforms, rebuilder, ct);
            await rebuilder.RebuildByTiles(tileManager, selectedTiles);

            Debug.Log($"マテリアル変換が完了しました。変換結果: 生成オブジェクト数={result.Count}");

            await HighlightSelectedTiles(false);
            SelectTileManager();
            return result;
        }

        /// <summary>
        /// ObservableCollection<string> から同一AddressのPLATEAUDynamicTileをtileManagerから探して返す
        /// </summary>
        /// <returns></returns>
        private List<PLATEAUDynamicTile> GetSelectedTiles()
        {
            return GetTilesFromAddr(observableSelected.ToList());
        }

        private List<PLATEAUDynamicTile> GetTilesFromAddr(List<TileSelectionItem> addresses)
        {
            var tiles = new List<PLATEAUDynamicTile>();
            foreach (var tile in tileManager.DynamicTiles)
            {
                //if (addresses.Contains(tile.Address))
                if (addresses.Any(x => x.TileAddress == tile.Address))
                    tiles.Add(tile);
            }
            return tiles;
        }

        /// <summary>
        /// タイルリストからTransformリストを取得する
        /// 読み込まれていないタイルは無視する
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        private List<Transform> GetTransformsFromTiles(List<PLATEAUDynamicTile> tiles)
        {
            var transforms = new List<Transform>();
            foreach (var tile in tiles)
            {
                if (tile.LoadedObject != null)
                {
                    transforms.Add(tile.LoadedObject.transform);
                }
            }
            return transforms;
        }

        /// <summary>
        /// Tileの編集可能Prefabルートをシーンに配置し、そのTransformを返す
        /// </summary>
        /// <returns></returns>
        private async Task<Transform> GetEditableTransformParent(TileRebuilder rebuilder, CancellationToken token)
        {
            var selectedTiles = GetSelectedTiles();

            await rebuilder.TilePrefabsToScene(tileManager, selectedTiles, token);
            token.ThrowIfCancellationRequested();
            var editingTile = tileManager.transform.Find(TileRebuilder.EditingTilesParentName);
            return editingTile;
        }

        /// <summary>
        /// Tileの編集可能Prefabルート内から選択された子のTransformリストを返す
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<Transform> GetEditableTransforms(Transform parent, bool asTile = false)
        {
            //変換用のTransformリストを作成しconfにセットします。
            var tileTransforms = new List<Transform>();
            foreach (var addr in observableSelected)
            {
                var edittrans = parent.transform.Find(addr.TileAddress)?.transform;

                if( addr.IsTile || asTile )
                {
                    // タイル直下の場合はそのまま
                    tileTransforms.Add(edittrans);

                }
                else
                {
                    // 子タイルの場合はパスを辿って同一のパスを探す
                    var children = edittrans.GetAllChildren();
                    foreach (var child in children)
                    {
                        var path = child.GetPathToParent(addr.TileAddress);
                        if (path == addr.TilePath)
                        {
                            tileTransforms.Add(child);
                            break;
                        }
                    }
                }
                
            }
            return tileTransforms;
        }

        /// <summary>
        /// 各TransformのPrefab Assetを保存する
        /// </summary>
        /// <param name="transforms">Prefabとして保存するTransform</param>
        /// <param name="rebuilder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task SavePrefabAssets(List<Transform> transforms, TileRebuilder rebuilder, CancellationToken token)
        {
            foreach (var trans in transforms)
            {
                if (trans == null) continue;
                Debug.Log($"SavePrefabAsset {trans.name}");

                await rebuilder.SavePrefabAsset(trans.gameObject);
                token.ThrowIfCancellationRequested();
            }
        }

        private bool AskUnlock()
        {
            if (!LockChange) return true;
            if (Dialogue.Display("対象を変更すると、この画面でなされた設定の一部がリセットされます。\nよろしいですか？", "変更を続行", "キャンセル"))
            {
                LockChange = false;
                return true;
            }
            return false;
        }
    }
}
