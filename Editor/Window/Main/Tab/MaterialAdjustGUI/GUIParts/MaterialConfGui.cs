using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.GUIParts
{ 
    /// <summary>
    /// 地物型ごとにマテリアルを設定するGUIです。
    /// </summary>
    internal class MaterialConfGui
    {

        public static void Draw(IMaterialAdjustConf conf)
        {
            int displayIndex = 1;

            // 存在する地物型を列挙します 
            for(int i=0; i<conf.Length; i++)
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.CategoryTitle(
                        $"パターン{displayIndex} : {conf.GetKeyNameAt(i)}");
                    var matChangeConf = conf.GetMaterialChangeConfAt(i);
                    matChangeConf.ChangeMaterial = EditorGUILayout.ToggleLeft("マテリアルを変更する", matChangeConf.ChangeMaterial);
                    if (matChangeConf.ChangeMaterial)
                    {
                        matChangeConf.Material = (Material)EditorGUILayout.ObjectField("マテリアル",
                            matChangeConf.Material, typeof(Material), false);
                    }
                }

                displayIndex++;
            }
        }
    }
}