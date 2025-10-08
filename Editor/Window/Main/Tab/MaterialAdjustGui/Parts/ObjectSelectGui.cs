using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 対象選択のGUIであり、次の機能まとめたものです。
    /// ・選択表示、選択追加スロット、「選択から追加」ボタン、「選択から除く」ボタン、対象を選択ボタン
    /// ・パッケージ種選択ウィンドウの結果を受け取る
    /// ・変更ロック（ロック時は変更時ダイアログで確認しキャンセル可能とする）
    ///
    /// これを利用するクラスは、コンストラクタでコールバックを登録することで選択を受け取ります。
    /// </summary>
    internal class ObjectSelectGui : Element, IPackageSelectResultReceiver
    {
        private ObservableCollection<Transform> observableSelected;
            

        public bool LockChange { get; set; }
        private readonly ScrollView scrollView = new (GUILayout.Height(160));
        private bool skipCallback;
        private bool skipLockCheck;
        private List<Transform> prevSelected = new(); // 変更を取り消すためのバックアップ
        private Action<UniqueParentTransformList> onSelectionChanged;

        private PLATEAUTileManager tileManager;
        string[] options = new string[] { "シーンに配置されたオブジェクト", "動的タイル" };
        int selectedIndex = 0;

        public ObjectSelectGui(Action<UniqueParentTransformList> onSelectionChanged)
        {
            this.onSelectionChanged = onSelectionChanged;
            observableSelected = new ();
            
            observableSelected.CollectionChanged += (sender, args) =>
            {
                // 変更時のコールバックを呼びます。
                // ただし、ロック中であり、ユーザーが変更をキャンセルした場合は呼ばずにロールバックします。
                if (skipCallback) return;
                if (skipLockCheck || AskUnlock())
                {
                    // コールバックを呼ぶ
                    Callback();
                    prevSelected = observableSelected.ToList();
                }
                else
                {
                    // キャンセル操作のため、変更を元に戻します
                    skipCallback = true;
                    observableSelected.Clear();
                    foreach(var p in prevSelected)
                    {
                        observableSelected.Add(p);
                    }
                    skipCallback = false;
                }
                
            };
            Callback(); // 初期値設定
        }

        private void Callback()
        {
            onSelectionChanged(new UniqueParentTransformList(observableSelected));
        }
        

        public override void DrawContent()
        {
            // nullチェック
            if (observableSelected.Any(tran => tran == null))
            {
                var nullRemoved = observableSelected.Where(tran => tran != null);
                ForceSet(new UniqueParentTransformList(nullRemoved));
            }
            
            // 描画
            PlateauEditorStyle.Heading("対象選択", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                selectedIndex = EditorGUILayout.Popup("調整対象の種類", selectedIndex, options);

                if (selectedIndex == 0)
                { 
                  // シーンに配置されたオブジェクト
                  // 追加用のスロットを描画
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("追加:", GUILayout.Width(30));
                        var added = (GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                        if (added != null)
                        {
                            // 追加
                            if (EditorUtility.IsPersistent(added))
                            {
                                Dialogue.Display("シーン外のゲームオブジェクトは選択できません。シーン内から選択してください。", "OK");
                            }
                            else if (AskUnlock())
                            {
                                observableSelected.Add(added.transform);
                            }
                        }
                    }
                }
                else if (selectedIndex == 1)
                {
                    EditorGUI.BeginChangeCheck();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("調整対象:", GUILayout.Width(60));
                        this.tileManager = (PLATEAUTileManager)EditorGUILayout.ObjectField(this.tileManager, typeof(PLATEAUTileManager), true);
                    }

                    if (PlateauEditorStyle.TinyButton("タイル追加", 100))
                    {
                        var window = TileSelectWindow.Open(this.tileManager);
                    }

                }

                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    ButtonAddFromSelection(); // 「選択中のn個を追加」ボタン
                    
                    if (PlateauEditorStyle.MiniButton("パッケージ種から選択", 150))
                    {
                        var window = PackageSelectWindow.Open();
                        window.Init(this);
                    }
                });
            }
            
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
                            EditorGUILayout.LabelField($"{i+1}:", GUILayout.Width(30));
                            var trans = observableSelected[i];
                            if (trans == null)
                            {
                                indexToDelete = i;
                                continue;
                            }
                            var obj = trans.gameObject;
                            var nextObj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true, GUILayout.ExpandWidth(true));
                            
                            if (nextObj != obj && AskUnlock())
                            {
                                observableSelected[i] = nextObj.transform;
                            }
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
                    if(PlateauEditorStyle.TinyButton("対象をヒエラルキー上でハイライト", 180))
                    {
                        Selection.objects = new UniqueParentTransformList(observableSelected).Get.Select(trans => trans.gameObject).Cast<Object>().ToArray();
                    }
                });
                
            }
        }
        

        /// <summary>
        /// 変更がロックされているのに変更しようとする場合に呼んでください。
        /// アンロックして良いかユーザーに尋ね、OKならアンロックしてtrueを返します。そうでなければfalseを返します。
        /// </summary>
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
        
        public override void Dispose()
        {
        }

        private void ButtonAddFromSelection()
        {
            // 選択から追加ボタン
            int count = Selection.gameObjects.Length;
            using (new EditorGUI.DisabledScope(count == 0))
            {
                if (PlateauEditorStyle.MiniButton($"選択中の {count} 個を追加", 150) && AskUnlock())
                {
                    foreach (var obj in Selection.gameObjects)
                    {
                        observableSelected.Add(obj.transform);
                    }
                }
            }
        }

        /// <summary>
        /// パッケージ種選択ウィンドウの結果を反映させます。
        /// 該当Model中の該当パッケージ種に該当するものをすべて追加します。
        /// </summary>
        public void ReceivePackageSelectResult(PackageSelectResult result)
        {
            var added = new UniqueParentTransformList();
            foreach (var cog in result.Dataset.GetComponentsInChildren<PLATEAUCityObjectGroup>(true))
            {
                var package = cog.Package;
                if (result.SelectedDict.TryGetValue(package, out bool isTarget) && isTarget)
                {
                    added.Add(cog.transform);
                }
            }
            added.ParentalShift();
            foreach (var trans in added.Get)
            {
                observableSelected.Add(trans);
            }
        }

        /// <summary>
        /// 変更時チェックをスキップしながら、コレクションの内容を引数のものに置き換えます。
        /// </summary>
        public void ForceSet(UniqueParentTransformList nextTransforms)
        {
            skipLockCheck = true;
            observableSelected.Clear();
            foreach (var item in nextTransforms.Get)
            {
                if (item == null) continue;
                observableSelected.Add(item);
            }
            skipLockCheck = false;
        }
        
    }
}