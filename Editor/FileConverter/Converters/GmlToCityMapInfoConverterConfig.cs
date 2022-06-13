using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// <see cref="GmlToObjFileConverter"/> の設定を保持します。
    /// </summary>
    public class GmlToCityMapInfoConverterConfig
    {
        public CitygmlParserParams ParserParams;
        public Vector3 ReferencePoint;

        public GmlToCityMapInfoConverterConfig()
        {
            this.ParserParams = new CitygmlParserParams();
            this.ReferencePoint = Vector3.zero;
        }
    }
}