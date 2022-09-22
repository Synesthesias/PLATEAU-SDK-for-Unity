using System;
using PLATEAU.IO;

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
        public uint minLOD;
        public uint maxLOD;
        public MeshGranularity meshGranularity;

        public PackageLoadSetting(bool loadPackage, bool includeTexture, uint minLOD, uint maxLOD, MeshGranularity meshGranularity)
        {
            this.loadPackage = loadPackage;
            this.includeTexture = includeTexture;
            this.minLOD = minLOD;
            this.maxLOD = maxLOD;
            this.meshGranularity = meshGranularity;
        }
    }
}
