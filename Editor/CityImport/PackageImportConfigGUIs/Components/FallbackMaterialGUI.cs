using PLATEAU.CityImport.Config.PackageImportConfigs;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Components
{
    /// <summary>
    /// デフォルトマテリアルの設定GUIです。
    /// </summary>
    internal class FallbackMaterialGUI : PackageImportConfigGUIComponent
    {
        public FallbackMaterialGUI(PackageImportConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                Conf.FallbackMaterial, typeof(Material), false);
        }
    }
}