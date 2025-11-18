using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts;
using PLATEAU.Util.Async;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Common.Tile
{
    internal class TileListElementData: IDisposable
    {
        public ObservableCollection<TileSelectionItem> ObservableSelectedTiles = new();
        public EditorWindow ParentEditorWindow { get; set; }
        public PLATEAUTileManager TileManager { get; set; }

        public bool EnableTileHierarchy { get; set; } = true;

        public TileListElementData(EditorWindow parentEditorWindow)
        {
            ParentEditorWindow = parentEditorWindow;
        }

        public void Dispose()
        {
            ObservableSelectedTiles.Clear();
            ParentEditorWindow = null;
            TileManager = null;
        }
    }

    /// <summary>
    /// タイル選択ウィンドウを開き、そこで選択されたタイルをリストに表示します。
    /// </summary>
    internal class TileListElement : Element
    {
        private TileListElementData tileListData;

        private bool IsHighlightSelectedTilesRunning = false; // ハイライト処理中かどうか

        private ScrollView scrollView = new(GUILayout.Height(160));

        public TileListElement(TileListElementData data)
        {
            tileListData = data;
        }

        public override void Dispose()
        {
            
        }

        public override void DrawContent()
        {
            DrawSelectTile();
            DrawScrollViewContent(scrollView);
        }

        public void DrawSelectTile()
        {
            GUI.enabled = (tileListData.TileManager != null);
            PlateauEditorStyle.CenterAlignHorizontal(() =>
            {
                if (PlateauEditorStyle.MiniButton("タイル選択", 150))
                {
                    var window = TileSelectWindow.Open(tileListData.TileManager, (result) =>
                    {
                        result.ToTileSelectionItems().ForEach(item =>
                        {
                            if (tileListData.ObservableSelectedTiles.Any(x => x.TilePath == item.TilePath)) return;
                            tileListData.ObservableSelectedTiles.Add(item);
                        });
                        tileListData.ParentEditorWindow.Repaint();
                    }, tileListData.EnableTileHierarchy);
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
                    if (tileListData.ObservableSelectedTiles.Count == 0)
                    {
                        EditorGUILayout.LabelField("（未選択）");
                    }

                    int indexToDelete = -1;
                    bool deleteByUserInput = false;
                    // 各選択オブジェクトのスロットを描画
                    for (var i = 0; i < tileListData.ObservableSelectedTiles.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"{i + 1}:", GUILayout.Width(30));
                            var item = tileListData.ObservableSelectedTiles[i];
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
                    if (indexToDelete >= 0 && deleteByUserInput)
                    {
                        tileListData.ObservableSelectedTiles.RemoveAt(indexToDelete);
                    }
                });// end scrollView

                PlateauEditorStyle.RightAlign(() =>
                {
                    if (PlateauEditorStyle.TinyButton("全て除く", 75))
                    {
                        tileListData.ObservableSelectedTiles.Clear();
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

        public override void Reset()
        {
            
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
                await TileConvertCommon.HighlightSelectedTiles(tileListData.ObservableSelectedTiles, tileListData.TileManager);
                tileListData.ParentEditorWindow.Repaint();
            }
            finally
            {
                IsHighlightSelectedTilesRunning = false;
            }
        }
    }
}
