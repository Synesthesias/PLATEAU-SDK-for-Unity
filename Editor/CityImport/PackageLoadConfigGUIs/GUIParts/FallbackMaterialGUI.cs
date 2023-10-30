using PLATEAU.CityImport.Config.PackageLoadConfigs;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport.PackageLoadConfigGUIs.GUIParts
{
    /// <summary>
    /// デフォルトマテリアルの設定GUIです。
    /// </summary>
    internal class FallbackMaterialGUI : PackageLoadConfigGUIComponent
    {
        public FallbackMaterialGUI(PackageLoadConfig conf) : base(conf)
        {
        }

        public override void Draw()
        {
            Conf.FallbackMaterial = (Material)EditorGUILayout.ObjectField("デフォルトマテリアル",
                Conf.FallbackMaterial, typeof(Material), false);
        }
    }
}