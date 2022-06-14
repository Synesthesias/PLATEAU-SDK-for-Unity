using System;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMapMetaData
{
    /// <summary>
    /// <see cref="CityModelImportWindow"/> のGUIによって入力される設定であり、
    /// 変換時に利用されます。
    /// </summary>
    [Serializable]
    public class CityModelImportConfig
    {
        public bool optimizeFlag = true;
        public MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        public Vector3 referencePoint = Vector3.zero;
        public DllLogLevel logLevel = DllLogLevel.Error;
    }
}