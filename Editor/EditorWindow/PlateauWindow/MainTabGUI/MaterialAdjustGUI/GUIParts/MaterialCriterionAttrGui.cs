using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{
    /// <summary>
    /// 属性情報によるマテリアル分けのGuiです。
    /// </summary>
    internal class MaterialCriterionAttrGui : MaterialCriterionGuiBase
    {
        public string AttrKey { get; private set; }
        public override bool Search(GameObject[] selectedObjs)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawBeforeTargetSelect()
        {
            using (PlateauEditorStyle.VerticalScopeWithPadding(8, 0, 8, 8))
            {
                EditorGUIUtility.labelWidth = 100;
                AttrKey = EditorGUILayout.TextField("属性情報キー", AttrKey);
            }
        }

        public override void DrawAfterTargetSelect()
        {
            throw new System.NotImplementedException();
        }
    }
}