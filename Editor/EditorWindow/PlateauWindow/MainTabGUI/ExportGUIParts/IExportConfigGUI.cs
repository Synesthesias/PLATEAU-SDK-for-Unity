using PLATEAU.CityExport.Exporters;
using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// <see cref="Model"/>(中間形式) をファイルにエクスポートするインターフェイスです。
    /// 対応ファイルフォーマットである FBX, GLTF, OBJ ごとにクラスを作ってこのインターフェイスを実装することで、
    /// ファイルフォーマットごとの設定項目の差異とエクスポート方法の差異を吸収します。
    /// </summary>
    internal interface IExportConfigGUI
    {
        /// <summary>
        /// ファイルフォーマットに固有の設定項目のGUIを描画します。
        /// </summary>
        public void Draw();

        public ICityExporter GetExporter();
    }
}
