using PLATEAU.CityExport.Exporters;
using PLATEAU.MeshWriter;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGUIParts
{
    /// <summary>
    /// Model(中間形式)をFBXファイルに出力します。
    /// </summary>
    internal class ExportConfigGuiFbx : IExportConfigGUI
    {
        private CityExporterFbx cityExporterFbx = new CityExporterFbx();
        
        public void Draw()
        {
            cityExporterFbx.FbxFileFormat = (FbxFileFormat)EditorGUILayout.EnumPopup("FBXフォーマット", cityExporterFbx.FbxFileFormat);
        }

        public ICityExporter GetExporter() => cityExporterFbx;

    }
}
