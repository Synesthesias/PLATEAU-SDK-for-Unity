using PLATEAU.CityMeta;
using PLATEAU.Interop;

namespace PLATEAU.Editor.Converters
{

    /// <summary>
    /// <see cref="GmlToObjConverter"/> の設定を保持します。
    /// </summary>
    internal class CityMapMetaDataGeneratorConfig
    {
        /// <summary> gmlロード時に使う設定です。 </summary>
        public CitygmlParserParams ParserParams;

        /// <summary> obj変換時の ReferencePoint が何であったかを記録します。 </summary>
        public CityImportConfig CityImportConfig;

        /// <summary> 変換時すでに CityMapInfo ファイルが存在する場合、trueならば idToGmlTable 消して作り直し、falseならば以前のデータに追加します。 </summary>
        public bool DoClearIdToGmlTable;
        

        public CityMapMetaDataGeneratorConfig()
        {
            this.ParserParams = new CitygmlParserParams();
            this.CityImportConfig = new CityImportConfig();
            this.DoClearIdToGmlTable = false;
        }
    }
}