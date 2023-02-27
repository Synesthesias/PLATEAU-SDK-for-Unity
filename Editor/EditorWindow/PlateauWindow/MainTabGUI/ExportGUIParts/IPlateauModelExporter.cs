using PLATEAU.PolygonMesh;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts
{
    /// <summary>
    /// <see cref="Model"/>(中間形式) をファイルにエクスポートするインターフェイスです。
    /// 対応ファイルフォーマットである FBX, GLTF, OBJ ごとにクラスを作ってこのインターフェイスを実装することで、
    /// ファイルフォーマットごとの設定項目の差異とエクスポート方法の差異を吸収します。
    /// </summary>
    internal interface IPlateauModelExporter
    {
        /// <summary>
        /// ファイルフォーマットに固有の設定項目のGUIを描画します。
        /// </summary>
        public void DrawConfigGUI();
        
        /// <summary>
        /// Model(中間形式)をファイルにエクスポートします。
        /// </summary>
        /// <param name="destDir">出力先のディレクトリのパスです。</param>
        /// <param name="fileNameWithoutExtension">出力ファイル名から拡張子を除いたものです。</param>
        /// <param name="model">モデル(中間形式)です。</param>
        public void Export(string destDir, string fileNameWithoutExtension, Model model);
    }
}
