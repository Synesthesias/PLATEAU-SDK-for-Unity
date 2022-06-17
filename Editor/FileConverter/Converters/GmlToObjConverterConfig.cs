using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter.Converters
{

    /// <summary>
    /// <see cref="GmlToObjConverter"/> の設定を保持するクラスです。
    /// </summary>
    public class GmlToObjConverterConfig
    {
        /// <summary> メッシュのオブジェクト分けの細かさです。 </summary>
        public MeshGranularity MeshGranularity;

        /// <summary>
        /// true の場合、変換の座標の基準点を自動で設定します。
        /// falseの場合、基準点を <see cref="ManualReferencePoint"/> に設定します。
        /// </summary>
        public bool DoAutoSetReferencePoint;

        /// <summary> 変換の基準点です。<see cref="DoAutoSetReferencePoint"/> が false の場合にのみ利用されます。 </summary>
        public Vector3? ManualReferencePoint;

        /// <summary> 取得するログの細かさです。 </summary>
        public DllLogLevel LogLevel;

        /// <summary> 最適化をするかどうかです。 </summary>
        public bool OptimizeFlag;

        /// <summary> 座標軸の変換方法です。Unityの場合は通常 RUF です。 </summary>
        public AxesConversion AxesConversion;


        public GmlToObjConverterConfig(
            MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject,
            bool doAutoSetReferencePoint = true,
            Vector3? manualReferencePoint = null,
            DllLogLevel logLevel = DllLogLevel.Error,
            bool optimizeFlag = true,
            AxesConversion axesConversion = AxesConversion.RUF
        )
        {
            this.MeshGranularity = meshGranularity;
            this.DoAutoSetReferencePoint = doAutoSetReferencePoint;
            this.ManualReferencePoint = manualReferencePoint;
            this.LogLevel = logLevel;
            this.OptimizeFlag = optimizeFlag;
            this.AxesConversion = axesConversion;
        }
    }
}