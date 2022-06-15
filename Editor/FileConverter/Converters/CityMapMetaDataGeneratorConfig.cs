using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapMeta;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// <see cref="GmlToObjFileConverter"/> の設定を保持します。
    /// </summary>
    public class CityMapMetaDataGeneratorConfig
    {
        /// <summary> gmlロード時に使う設定です。 </summary>
        public CitygmlParserParams ParserParams;

        /// <summary> obj変換時の ReferencePoint が何であったかを記録します。 </summary>
        public CityModelImportConfig CityModelImportConfig;

        /// <summary> 変換時すでに CityMapInfo ファイルが存在する場合、trueならば idToGmlTable 消して作り直し、falseならば以前のデータに追加します。 </summary>
        public bool DoClearIdToGmlTable;
        

        public CityMapMetaDataGeneratorConfig()
        {
            this.ParserParams = new CitygmlParserParams();
            this.CityModelImportConfig = new CityModelImportConfig();
            this.DoClearIdToGmlTable = false;
        }
    }
}