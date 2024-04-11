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
    internal class MaterialCriterionTypeGui : MaterialCriterionGuiBase
    {
        private MaterialByTypeConfGui materialConfGui;
        
        
        public override bool Search(GameObject[] selectedObjs)
        {
            adjuster = new MaterialAdjusterByType(selectedObjs); // ここで検索します。
            if (adjuster.MaterialAdjustConf.Length <= 0)
            {
                Dialogue.Display("地物型が見つかりませんでした。\n属性情報を含む都市オブジェクトかその親を選択してください。", "OK");
                adjuster = null;
                return false;
            }

            return true;
        }

        public override void DrawBeforeTargetSelect()
        {
            // 何もしません
        }
        
        public override void DrawAfterTargetSelect()
        {
            MaterialByTypeConfGui.Draw(adjuster.MaterialAdjustConf);

            PlateauEditorStyle.Separator(0);

            if (PlateauEditorStyle.MainButton("実行"))
            {
                adjuster.Exec().ContinueWithErrorCatch(); // ここで実行します。
            }
        }
    }
}