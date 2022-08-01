using PLATEAU.Interop;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// <see cref="GmlToObjConverter"/> の設定を保持するクラスです。
    /// </summary>
    
    internal class GmlToObjConverterConfig
    {
        /// <summary> C++側の変換器に渡すオプションです。 </summary>
        public MeshConvertOptions DllConvertOption { get; } = new MeshConvertOptions();

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


        public bool ExportAppearance
        {
            get => this.DllConvertOption.ExportAppearance;
            set => this.DllConvertOption.ExportAppearance = value;
        }
        
        public MeshGranularity MeshGranularity
        {
            get => this.DllConvertOption.MeshGranularity;
            set => this.DllConvertOption.MeshGranularity = value;
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

        public bool ExportLowerLod
        {
            get => this.DllConvertOption.ExportLowerLOD;
            set => this.DllConvertOption.ExportLowerLOD = value;
        }


        public GmlToObjConverterConfig(
            bool exportAppearance = true,
            MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject,
            int minLod = 0,
            int maxLod = 3,
            bool exportLowerLod = true,
            bool doAutoSetReferencePoint = true,
            Vector3? manualReferencePoint = null,
            DllLogLevel logLevel = DllLogLevel.Error
        )
        {
            ExportAppearance = exportAppearance;
            MeshGranularity = meshGranularity;
            MinLod = minLod;
            MaxLod = maxLod;
            ExportLowerLod = exportLowerLod;
            this.DoAutoSetReferencePoint = doAutoSetReferencePoint;
            this.ManualReferencePoint = manualReferencePoint;
            this.LogLevel = logLevel;
        }
    }
}