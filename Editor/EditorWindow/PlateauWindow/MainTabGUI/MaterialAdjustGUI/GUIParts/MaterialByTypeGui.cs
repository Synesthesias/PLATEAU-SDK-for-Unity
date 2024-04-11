using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// 地物型によるマテリアル分けのGUIです。
    /// </summary>
    internal class MaterialByTypeGui : MaterialGuiBase
    {

        
        public override void Search(GameObject[] selectedObjs)
        {
            adjuster = new MaterialAdjusterByType(selectedObjs); // ここで検索します。
            if (adjuster.MaterialAdjustConf.Length <= 0)
            {
                Dialogue.Display("地物型が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                adjuster = null;
            }
            
        }

        public override void DrawBeforeTargetSelect()
        {
            // 何もしません
        }
        
        public override void DrawAfterTargetSelect()
        {
            DisplayCityObjectTypeMaterialConfGUI();

            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                adjuster.Exec().ContinueWithErrorCatch(); // ここで実行します。
            }
        }
        
        /// <summary>
        /// 地物型ごとにマテリアルを設定するGUIです。
        /// </summary>
        private void DisplayCityObjectTypeMaterialConfGUI()
        {
            var conf = adjuster.MaterialAdjustConf;
            int displayIndex = 1;

            // 存在する地物型を列挙します 
            foreach (var (typeNode, typeConf) in conf)
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.CategoryTitle(
                        $"地物型{displayIndex} : {typeNode.GetDisplayName()}");
                    typeConf.ChangeMaterial = EditorGUILayout.ToggleLeft("マテリアルを変更する", typeConf.ChangeMaterial);
                    if (typeConf.ChangeMaterial)
                    {
                        typeConf.Material = (Material)EditorGUILayout.ObjectField("マテリアル",
                            typeConf.Material, typeof(Material), false);
                    }
                }

                displayIndex++;
            }
        }
    }
}