using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{

    /// <summary>
    /// リストに表示するタイル選択アイテム
    /// </summary>
    internal class TileSelectionItem
    {
        public string TileAddress { get; private set; } 
        public string TilePath { get; private set; }

        public bool IsTile => string.Equals(TileAddress, TilePath); // Tileでない場合はタイル内のオブジェクトを指す

        public TileSelectionItem(string fullpath)
        {
            TilePath = fullpath;
            // path( 例: "xxxxx_op/LODx/xxxx-xxx") からタイルのAddress(例: "xxxxx_op")を取得する
            var splits = fullpath.Split('/');
            TileAddress =  splits[0];
        }
    }

    /// <summary>
    /// <see cref="CityMaterialAdjustPresenter"/>内のタイル選択時の動作やUIを担当するクラス
    /// </summary>
    internal class TileMaterialAdjustGui
    {
        public bool LockChange { get; set; }

        private PLATEAUTileManager tileManager;

        private ObservableCollection<TileSelectionItem> observableSelected = new();

        private readonly EditorWindow parentEditorWindow;

        private bool IsHighlightSelectedTilesRunning = false; // ハイライト処理中かどうか

        public ObservableCollection<TileSelectionItem> Selected => observableSelected;

        public bool HasTileManager => this.tileManager != null;

        public TileMaterialAdjustGui(EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            SetDefaultTileManager();
        }

        /// <summary>
        /// デフォルトでシーン内のTileManagerをセット
        /// </summary>
        public void SetDefaultTileManager()
        {
            this.tileManager = GameObject.FindObjectOfType<PLATEAUTileManager>(); 
        }

        public void Reset()
        {
            IsHighlightSelectedTilesRunning = false;
        }

        public void ClearSelected()
        {
            observableSelected.Clear();
        }

        /// <summary>
        /// ObjectSelectGui 内のタイル選択部分を描画する
        /// </summary>
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
                        result.ToTileSelectionItems().ForEach(item =>
                        {
                            if (observableSelected.Any(x => x.TilePath == item.TilePath)) return;
                            observableSelected.Add(item);
                        });
                        this.parentEditorWindow.Repaint();
                    });
                }
            });
            GUI.enabled = true;
        }

        /// <summary>
        /// ObjectSelectGui 内の選択タイル表示部分を描画する
        /// </summary>
        /// <param name="scrollView"></param>
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

        /// <summary>
        /// observableSelectedに含まれるタイルをハイライトする
        /// </summary>
        /// <param name="forceLoad"></param>
        /// <returns></returns>
        private async Task HighlightSelectedTiles(bool forceLoad = true)
        {
            if (IsHighlightSelectedTilesRunning) return;
            IsHighlightSelectedTilesRunning = true;
            try
            {
                var transforms = new List<Transform>();
                if (forceLoad)
                {
                    var allTransforms = await GetTransformListFromAddr<Component>(observableSelected);
                    transforms = allTransforms.Where(trans => observableSelected
                        .Any(selected => trans.GetPathToParent(selected.TileAddress) == selected.TilePath || trans.FindChildOfNamedParent(PLATEAUTileManager.TileParentName)?.name == selected.TilePath)).ToList();
                }
                else
                {
                    transforms = GetTransformsFromTiles(GetSelectedTiles());
                }

                Selection.objects = transforms.Select(trans => trans.gameObject).Cast<UnityEngine.Object>().ToArray();
                parentEditorWindow.Repaint();
            }
            finally
            {
                IsHighlightSelectedTilesRunning = false;
            }
        }

        /// <summary>
        /// observableSelectedのAddressに含まれるタイルを読み込んで、Transformリストを取得する
        /// LODにPLATEAUCityObjectGroupが設定されていないので、全てのコンポーネントを対象とする
        /// </summary>
        /// <returns></returns>
        public async Task<UniqueParentTransformList> GetSelectionAsTransformList()
        {
            var transforms = await GetTransformListFromAddr<Component>(observableSelected);
            return new UniqueParentTransformList(transforms);
        }

        /// <summary>
        /// Addressに含まれるタイルを読み込んで、Transformリストを取得する
        /// </summary>
        /// <typeparam name="T">指定したタイプのコンポーネントを持つGameObjectのみが対象</typeparam>
        /// <returns></returns>
        public async Task<List<Transform>> GetTransformListFromAddr<T>(IList<TileSelectionItem> addr) where T : Component
        {
            var cts = new CancellationTokenSource();
            try
            {
                var addresses = addr.ToList().Select(x => x.TileAddress).Distinct().ToList();
                var selectedTiles = await tileManager.ForceLoadTiles(addresses, cts.Token);
                var transforms = new List<Transform>();
                foreach (var tile in selectedTiles)
                {
                    if (tile.LoadedObject != null)
                    {
                        TransformEx.GetAllChildrenWithComponent<T>(tile.LoadedObject.transform).ForEach(t => transforms.Add(t));
                    }
                }
                return transforms;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("タイルの読み込みがキャンセルされました。");
                return new List<Transform>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"タイルの読み込み中にエラーが発生しました: {ex.Message}");
                return new List<Transform>();
            }
        }

        /// <summary>
        /// 分割結合実行
        /// CityMaterialAdjustPresenterから呼ばれます。
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="granularity"></param>
        /// <returns></returns>
        public async Task<GranularityConvertResult> ExecGranularityConvertAsync(MAExecutorConf conf, ConvertGranularity granularity)
        {
            if (tileManager == null)
            {
                Dialogue.Display("タイルマネージャーを指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }

            if (observableSelected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var selectedTiles = GetSelectedTiles();
                var rebuilder = new TileRebuilder();
                var editingTile = await GetEditableTransformParent(rebuilder, ct);
                var tileTransforms = GetEditableTransforms(editingTile);
                conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

                // ここで実行します。
                var result = await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(granularity, 1), conf.TargetTransforms, conf.DoDestroySrcObjs));
                var generatedSelection = result.GeneratedObjs.Select(o => new TileSelectionItem(GetTilePath(o.transform, TileRebuilder.EditingTilesParentName))).ToList(); // 生成されたオブジェクトの名前リストを取得

                //　子要素ではなくタイルのTransformを取得し直す。タイルの場合も参照が切れるため入れ直しが必要
                tileTransforms.Clear();
                tileTransforms = GetEditableTransforms(editingTile, true);

                await SavePrefabAssets(tileTransforms, rebuilder, ct);
                await rebuilder.RebuildByTiles(tileManager, selectedTiles);

                Debug.Log($"粒度変換が完了しました。変換結果: 成功={result.IsSucceed}, 生成オブジェクト数={result.GeneratedRootTransforms.Count}");

                // 変換されたオブジェクトを選択リストにセットし、タイルが読み込まれていればハイライトする
                observableSelected.Clear();
                generatedSelection.ForEach(item => observableSelected.Add(item));
                await HighlightSelectedTiles(false);
                SetDefaultTileManager();
                return result;
            }
        }

        /// <summary>
        /// マテリアル分け実行
        /// CityMaterialAdjustPresenterから呼ばれます。
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="maExecutor"></param>
        /// <returns></returns>
        public async Task<UniqueParentTransformList> ExecMaterialAdjustAsync(MAExecutorConf conf, IMAExecutorV2 maExecutor )//, bool destroySrcObjs)
        {
            if (tileManager == null)
            {
                Dialogue.Display("タイルマネージャーを指定してください。", "OK");
                return new UniqueParentTransformList();
            }

            if (observableSelected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return new UniqueParentTransformList();
            }

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var selectedTiles = GetSelectedTiles();
                var rebuilder = new TileRebuilder();
                var editingTile = await GetEditableTransformParent(rebuilder, ct);
                var tileTransforms = GetEditableTransforms(editingTile);
                conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

                // ここで実行します。
                var result = await maExecutor.ExecAsync(conf);
                var generatedSelection = result.Get.Select(t => new TileSelectionItem(GetTilePath(t, TileRebuilder.EditingTilesParentName))).ToList();

                //　子要素ではなくタイルのTransformを取得し直す。タイルの場合も参照が切れるため入れ直しが必要
                tileTransforms.Clear();
                tileTransforms = GetEditableTransforms(editingTile, true);

                await SavePrefabAssets(tileTransforms, rebuilder, ct);
                await rebuilder.RebuildByTiles(tileManager, selectedTiles);

                Debug.Log($"マテリアル変換が完了しました。変換結果: 生成オブジェクト数={result.Count}");

                // 変換されたオブジェクトを選択リストにセットし、タイルが読み込まれていればハイライトする
                observableSelected.Clear();
                generatedSelection.ForEach(item => observableSelected.Add(item));
                await HighlightSelectedTiles(false);
                SetDefaultTileManager();
                return result;
            }
        }

        /// <summary>
        /// ObservableCollection<string> から同一AddressのPLATEAUDynamicTileをtileManagerから探して返す
        /// </summary>
        /// <returns></returns>
        private List<PLATEAUDynamicTile> GetSelectedTiles()
        {
            return GetTilesFromAddr(observableSelected.ToList());
        }

        /// <summary>
        /// Addressリストから同一AddressのPLATEAUDynamicTileをtileManagerから探して返す
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        private List<PLATEAUDynamicTile> GetTilesFromAddr(List<TileSelectionItem> addresses)
        {
            var tiles = new List<PLATEAUDynamicTile>();
            foreach (var tile in tileManager.DynamicTiles)
            {
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
            if (editingTile == null)
                throw new InvalidOperationException($"{TileRebuilder.EditingTilesParentName} が見つかりませんでした。");
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
                if (edittrans == null)
                {
                    Debug.LogWarning($"タイル {addr.TileAddress} が見つかりませんでした。");
                    continue;
                }

                if ( addr.IsTile || asTile )
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
        /// Transformからタイルパスを取得する
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parentName"> PLATEAUTileManager.TileParentName / TileRebuilder.EditingTilesParentName </param>
        /// <returns></returns>
        private string GetTilePath(Transform obj, string parentName)
        {
            var pathToRoot = obj.GetPathToParent(parentName);
            if (pathToRoot == null)
                return obj.name; // 親が見つからなかった場合はオブジェクト名のみを返す
            var prefix = parentName + "/";
            if (!pathToRoot.StartsWith(prefix))
                    return pathToRoot;
            return pathToRoot.Substring(prefix.Length); // "DynamicTileRoot/" の分をスキップ
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
