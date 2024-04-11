using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.MaterialAdjustGUI.GUIParts
{ 
    /// <summary>
    /// 地物型ごとにマテリアルを設定するGUIです。
    /// </summary>
    internal class MaterialByTypeConfGui
    {

        public static void Draw(MaterialAdjustConf conf)
        {
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