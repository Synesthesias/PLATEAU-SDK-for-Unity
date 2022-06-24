using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.Editor.Converters
{

    /// <summary>
    /// <see cref="GmlToObjConverter"/> の設定を保持するクラスです。
    /// </summary>
    internal class GmlToObjConverterConfig
    {
        /// <summary> C++側の変換器に渡すオプションです。 </summary>
        public MeshConvertOptions DllConvertOption;

        /// <summary>
        /// true の場合、変換の座標の基準点を自動で設定します。
        /// falseの場合、基準点を <see cref="ManualReferencePoint"/> に設定します。
        /// </summary>
        public bool DoAutoSetReferencePoint;

        /// <summary>
        /// <see cref="DoAutoSetReferencePoint"/> が false の場合に設定される基準点です。
        /// </summary>
        public Vector3? ManualReferencePoint;

        /// <summary> 取得するログの細かさです。 </summary>
        public DllLogLevel LogLevel;

        /// <summary> 最適化をするかどうかです。 </summary>
        public bool OptimizeFlag;

        public MeshGranularity MeshGranularity
        {
            get => this.DllConvertOption.MeshGranularity;
            set => this.DllConvertOption.MeshGranularity = value;
        }

        public AxesConversion AxesConversion
        {
            get => this.DllConvertOption.MeshAxes;
            set => this.DllConvertOption.MeshAxes = value;
        }

        public int MinLod
        {
            get => (int)this.DllConvertOption.MinLOD;
            set => this.DllConvertOption.MinLOD = (uint)value;
        }
        
        public int MaxLod
        {
            get => (int)this.DllConvertOption.MaxLOD;
            set => this.DllConvertOption.MaxLOD = (uint)value;
        }


        // TODO 次の設定を含めるようにする。　ReferencePoint, MinLOD, MaxLOD, ExportLowerLOD
        public GmlToObjConverterConfig(
            MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject,
            bool doAutoSetReferencePoint = true,
            Vector3? manualReferencePoint = null,
            DllLogLevel logLevel = DllLogLevel.Error,
            bool optimizeFlag = true,
            AxesConversion axesConversion = AxesConversion.RUF
        )
        {
            // this.MeshGranularity = meshGranularity;
            this.DoAutoSetReferencePoint = doAutoSetReferencePoint;
            this.ManualReferencePoint = manualReferencePoint;
            this.LogLevel = logLevel;
            this.OptimizeFlag = optimizeFlag;
            // this.AxesConversion = axesConversion;
            this.DllConvertOption = new MeshConvertOptions()
            {
                MeshAxes = axesConversion,
                MeshGranularity = meshGranularity
            };
        }
    }
}