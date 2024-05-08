using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    internal class ObjectSelectGui : Element, IPackageSelectResultReceiver
    {
        private readonly List<GameObject> selectedGameObjs = new();
        public UniqueParentTransformList UniqueSelected => new UniqueParentTransformList(selectedGameObjs.Select(obj => obj.transform));

        public bool LockChange { get; set; }
        private readonly EditorWindow parentEditorWindow;
        private readonly CityMaterialAdjustGUI materialAdjustGUI;
        private readonly ScrollView scrollView = new (GUILayout.Height(160));

        public ObjectSelectGui(CityMaterialAdjustGUI materialAdjustGUI, EditorWindow parentEditorWindow)
        {
            this.parentEditorWindow = parentEditorWindow;
            this.materialAdjustGUI = materialAdjustGUI;
        }

        public override void DrawContent()
        {
            PlateauEditorStyle.Heading("対象選択", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
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
                        else if(AskUnlock())
                        {
                            selectedGameObjs.Add(added);
                        }
                    }
                }
                
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    ButtonAddFromSelection(); // 「選択中のn個を追加」ボタン
                    
                    if (PlateauEditorStyle.MiniButton("パッケージ種から選択", 150))
                    {
                        PackageSelectWindow.Open(this);
                    }
                });
            }
            
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                scrollView.Draw(() =>
                {
                    if (selectedGameObjs.Count == 0)
                    {
                        EditorGUILayout.LabelField("（未選択）");                        
                    }
                    
                    int indexToDelete = -1;
                    // 各選択オブジェクトのスロットを描画
                    for (var i = 0; i < selectedGameObjs.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"{i+1}:", GUILayout.Width(30));
                            var obj = selectedGameObjs[i];
                            var nextObj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true, GUILayout.ExpandWidth(true));
                            
                            if (nextObj != obj && AskUnlock())
                            {
                                selectedGameObjs[i] = nextObj;
                            }
                            if (PlateauEditorStyle.TinyButton("除く", 30))
                            {
                                indexToDelete = i;
                            }
                        }
                        
                    }
                    // 削除ボタンが押された時
                    if (indexToDelete >= 0 && AskUnlock())
                    {
                        selectedGameObjs.RemoveAt(indexToDelete);
                    }
                });// end scrollView
                
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
                materialAdjustGUI.NotifyTargetChange();
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
                    selectedGameObjs.AddRange(Selection.gameObjects);
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
            selectedGameObjs.AddRange(added.Get.Select(trans => trans.gameObject));
        }
        
    }
}