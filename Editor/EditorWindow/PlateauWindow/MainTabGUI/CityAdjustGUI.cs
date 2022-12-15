using System;
using System.Threading.Tasks;
using PLATEAU.CityAdjust;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityAdjustGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel adjustTarget;
        private CityObjectTypeSelectGUI typeSelectGUI = new CityObjectTypeSelectGUI();
        private PackageLodSelectGUI packageLodSelectGUI = new PackageLodSelectGUI();
        private bool disableDuplicate = true;
        private static bool isFilterTaskRunning;
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("配置済みモデルデータの調整を行います。");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.adjustTarget =
                    (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                        "調整対象", this.adjustTarget,
                        typeof(PLATEAUInstancedCityModel), true);
                
                if (this.adjustTarget == null) return;
                
                if(PlateauEditorStyle.MainButton("重複した地物のうち、\nLODが最大のもののみ有効化"))
                {
                    CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);            
                }
                    
                PlateauEditorStyle.Separator(0);

                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.SubTitle("フィルタリング");
                    EditorGUILayout.LabelField("条件に応じてゲームオブジェクトのON/OFFを切り替えます。");

                    var duplicateToggleContent =
                        new GUIContent("重複する地物を非表示", "有効な場合、重複した地物オブジェクトのうちLODが最大のもののみ残してそれ以外を非表示にします。"); 
                    this.disableDuplicate = EditorGUILayout.Toggle(duplicateToggleContent, this.disableDuplicate);

                    PlateauEditorStyle.Heading("都市オブジェクトタイプ指定", null);
                    this.typeSelectGUI.Draw();
                    
                    PlateauEditorStyle.Heading("LOD指定", null);
                    this.packageLodSelectGUI.Draw();

                    using (new EditorGUI.DisabledScope(isFilterTaskRunning))
                    {
                        if (PlateauEditorStyle.MainButton(isFilterTaskRunning ? "フィルタリング中..." : "フィルタリング実行"))
                        {
                            isFilterTaskRunning = true;
                            CityFilterByCityObjectType.FilterAsync(this.adjustTarget, this.typeSelectGUI.SelectionDict)
                                .ContinueWithErrorCatch()
                                .ContinueWith(_ =>
                                {
                                    if(this.disableDuplicate) CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);    
                                    SceneView.RepaintAll();
                                    isFilterTaskRunning = false;
                                }, TaskScheduler.FromCurrentSynchronizationContext()).ContinueWithErrorCatch();
                        }
                    }

                }
            }
        }
    }
}
