using System;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.CityLoader.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// </summary>
    [Serializable]
    public class PackageLoadSetting
    {
        public bool loadPackage;
        public bool includeTexture;
        public int minLOD;
        public int maxLOD;
        public MeshGranularity meshGranularity;

        public PackageLoadSetting(bool loadPackage, bool includeTexture, int minLOD, int maxLOD, MeshGranularity meshGranularity)
        {
            this.loadPackage = loadPackage;
            this.includeTexture = includeTexture;
            this.minLOD = minLOD;
            this.maxLOD = maxLOD;
            this.meshGranularity = meshGranularity;
        }
    }
}
