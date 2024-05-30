using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{ 
    /// <summary>
    /// 地物型ごとにマテリアルを設定するGUIです。
    /// </summary>
    internal class MaterialConfGui : Element
    {
        private readonly CityMaterialAdjustGUI adjustGui;
        
        public MaterialConfGui(CityMaterialAdjustGUI adjustGui)
        {
            this.adjustGui = adjustGui;
        }

        public override void DrawContent()
        {
            var conf = adjustGui?.CurrentSearcher?.MaterialAdjustConf;
            if (conf == null) return;
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
            
            PlateauEditorStyle.Separator(0);
        }
        
        public override void Dispose(){}
    }
}