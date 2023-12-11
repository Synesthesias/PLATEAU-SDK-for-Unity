using PLATEAU.CityImport.Config.PackageImportConfigs;
using PLATEAU.PolygonMesh;
using UnityEditor;

namespace PLATEAU.Editor.CityImport.PackageImportConfigGUIs.Extendables.Components
{
    /// <summary>
    /// メッシュ粒度の設定GUIです。
    /// </summary>
    internal class MeshGranularityGUI : ExtendableConfigGUIBase
    {

        public MeshGranularityGUI(PackageImportConfigExtendable conf) : base(conf){}

        public override void Draw()
        {
            Conf.MeshGranularity = GranularityGUI.Draw("モデル結合", Conf.MeshGranularity);
        }
    }

    internal static class GranularityGUI
    {
        public static MeshGranularity Draw(string label, MeshGranularity current)
        {
            return (MeshGranularity)EditorGUILayout.Popup(label,
                (int)current, new[] { "最小地物単位(壁面,屋根面等)", "主要地物単位(建築物,道路等)", "地域単位" });
        }
    }
}