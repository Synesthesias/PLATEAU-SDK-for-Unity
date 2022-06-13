using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// <see cref="GmlToObjFileConverter"/> の設定を保持します。
    /// </summary>
    public class GmlToCityMapInfoConverterConfig
    {
        /// <summary> gmlロード時に使う設定です。 </summary>
        public CitygmlParserParams ParserParams;
        
        /// <summary> obj変換時の ReferencePoint が何であったかを記録します。 </summary>
        public Vector3 ReferencePoint;

        /// <summary> obj変換時の MeshGranularityが何であったかを記録します。 </summary>
        public MeshGranularity MeshGranularity;
        

        public GmlToCityMapInfoConverterConfig()
        {
            this.ParserParams = new CitygmlParserParams();
            this.ReferencePoint = Vector3.zero;
            this.MeshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        }
    }
}