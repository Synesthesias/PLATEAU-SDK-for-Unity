using PLATEAU.PolygonMesh;

namespace PLATEAU.CityExport.Exporters
{
    public interface ICityExporter
    {
        /// <summary>
        /// Model(中間形式)をファイルにエクスポートします。
        /// </summary>
        /// <param name="destDir">出力先のディレクトリのパスです。</param>
        /// <param name="fileNameWithoutExtension">出力ファイル名から拡張子を除いたものです。</param>
        /// <param name="model">モデル(中間形式)です。</param>
        public void Export(string destDir, string fileNameWithoutExtension, Model model);
    }
}