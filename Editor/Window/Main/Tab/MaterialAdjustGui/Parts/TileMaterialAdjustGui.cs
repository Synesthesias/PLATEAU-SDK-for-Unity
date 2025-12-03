using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Common.Tile;
using PLATEAU.GranularityConvert;
using PLATEAU.Util;
using System;
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
            if (string.IsNullOrEmpty(fullpath))
                throw new ArgumentException("fullpath は null または空にできません。", nameof(fullpath));

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

        public PLATEAUTileManager TileManager => tileListElementData?.TileManager;

        public ObservableCollection<TileSelectionItem> Selected => tileListElementData?.ObservableSelectedTiles;

        private readonly EditorWindow parentEditorWindow;
        private TileListElement tileListElement;
        private TileListElementData tileListElementData;

        private bool IsHighlightSelectedTilesRunning = false; // ハイライト処理中かどうか

        public bool HasTileManager => tileListElementData?.TileManager != null;

        public TileMaterialAdjustGui(EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;        
            tileListElementData = new(parentEditorWindow);
            tileListElement = new(tileListElementData);
            SetDefaultTileManager();
        }

        /// <summary>
        /// デフォルトでシーン内のTileManagerをセット
        /// </summary>
        public void SetDefaultTileManager()
        {
            if (tileListElementData != null)
                tileListElementData.TileManager = GameObject.FindAnyObjectByType<PLATEAUTileManager>(); 
        }

        public void Reset()
        {
            IsHighlightSelectedTilesRunning = false;
        }

        public void ClearSelected()
        {
            Selected?.Clear();
        }

        /// <summary>
        /// TileManagerが変更されたときの処理
        /// </summary>
        /// <param name="newTileManager"></param>
        private void OnChangeTargetTileManager(PLATEAUTileManager newTileManager)
        {
            ClearSelected();
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
                tileListElementData.TileManager = (PLATEAUTileManager)EditorGUILayout.ObjectField(TileManager, typeof(PLATEAUTileManager), true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                OnChangeTargetTileManager(TileManager);
            }

            tileListElement.DrawSelectTile();
        }

        /// <summary>
        /// ObjectSelectGui 内の選択タイル表示部分を描画する
        /// </summary>
        /// <param name="scrollView"></param>
        public void DrawScrollViewContent(ScrollView scrollView)
        {
            tileListElement.DrawScrollViewContent(scrollView);
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
                await TileConvertCommon.HighlightSelectedTiles(Selected, TileManager, forceLoad);
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
            return await TileConvertCommon.GetSelectionAsTransformList(Selected, TileManager);
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
            if (TileManager == null)
            {
                Dialogue.Display("タイルマネージャーを指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }

            if (Selected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return GranularityConvertResult.Fail();
            }

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var selectedTiles = TileConvertCommon.GetSelectedTiles(Selected, TileManager);
                var rebuilder = new TileRebuilder();
                var editingTile = await TileConvertCommon.GetEditableTransformParent(Selected, TileManager, rebuilder, ct);
                var tileTransforms = TileConvertCommon.GetEditableTransforms(Selected, editingTile);
                conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

                // ここで実行します。
                var result = await new CityGranularityConverter().ConvertAsync(new GranularityConvertOptionUnity(new GranularityConvertOption(granularity, 1), conf.TargetTransforms, conf.DoDestroySrcObjs));
                var generatedSelection = result.GeneratedObjs.Select(o => new TileSelectionItem(TileConvertCommon.GetTilePath(o.transform, TileRebuilder.EditingTilesParentName))).ToList(); // 生成されたオブジェクトの名前リストを取得

                //　子要素ではなくタイルのTransformを取得し直す。タイルの場合も参照が切れるため入れ直しが必要
                tileTransforms.Clear();
                tileTransforms = TileConvertCommon.GetEditableTransforms(Selected, editingTile, true);

                await TileConvertCommon.SavePrefabAssets(TileManager, tileTransforms, rebuilder, ct);
                await rebuilder.RebuildByTiles(TileManager, selectedTiles);

                Debug.Log($"粒度変換が完了しました。変換結果: 成功={result.IsSucceed}, 生成オブジェクト数={result.GeneratedRootTransforms.Count}");

                // 変換されたオブジェクトを選択リストにセットし、タイルが読み込まれていればハイライトする
                Selected.Clear();
                generatedSelection.ForEach(item => Selected.Add(item));
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
            if (TileManager == null)
            {
                Dialogue.Display("タイルマネージャーを指定してください。", "OK");
                return new UniqueParentTransformList();
            }

            if (Selected.Count == 0)
            {
                Dialogue.Display("粒度変換の対象を指定してください。", "OK");
                return new UniqueParentTransformList();
            }

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var selectedTiles = TileConvertCommon.GetSelectedTiles(Selected, TileManager);
                var rebuilder = new TileRebuilder();
                var editingTile = await TileConvertCommon.GetEditableTransformParent(Selected, TileManager, rebuilder, ct);
                var tileTransforms = TileConvertCommon.GetEditableTransforms(Selected, editingTile);
                conf.TargetTransforms = new UniqueParentTransformList(tileTransforms);

                // ここで実行します。
                var result = await maExecutor.ExecAsync(conf);
                var generatedSelection = result.Get.Select(t => new TileSelectionItem(TileConvertCommon.GetTilePath(t, TileRebuilder.EditingTilesParentName))).ToList();

                //　子要素ではなくタイルのTransformを取得し直す。タイルの場合も参照が切れるため入れ直しが必要
                tileTransforms.Clear();
                tileTransforms = TileConvertCommon.GetEditableTransforms(Selected, editingTile, true);

                await TileConvertCommon.SavePrefabAssets(TileManager, tileTransforms, rebuilder, ct);
                await rebuilder.RebuildByTiles(TileManager, selectedTiles);

                Debug.Log($"マテリアル変換が完了しました。変換結果: 生成オブジェクト数={result.Count}");

                // 変換されたオブジェクトを選択リストにセットし、タイルが読み込まれていればハイライトする
                Selected.Clear();
                generatedSelection.ForEach(item => Selected.Add(item));
                await HighlightSelectedTiles(false);
                SetDefaultTileManager();
                return result;
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
