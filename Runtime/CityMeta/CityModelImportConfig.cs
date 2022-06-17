using System;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMeta
{
    /// <summary>
    /// 複数gmlファイルの変換時に利用されます。
    /// </summary>
    [Serializable]
    public class CityModelImportConfig
    {
        public GmlSelectorConfig gmlSelectorConfig = new GmlSelectorConfig();
        public string sourceUdxFolderPath = "";
        public string exportFolderPath = "";
        public bool optimizeFlag = true;
        public MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        public Vector3 referencePoint = Vector3.zero;
        public DllLogLevel logLevel = DllLogLevel.Error;
    }
}