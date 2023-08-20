using System;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// <see cref="CityLoadConfig"/> によって保持されます。
    /// </summary>
    internal class PackageLoadSetting
    {
        public PredefinedCityModelPackage Package { get; }
        public bool LoadPackage;
        public bool IncludeTexture;
        
        /// <summary> ユーザーが選択したLOD範囲の下限 </summary>
        public int MinLOD;
        
        /// <summary> ユーザーが選択したLODの上限 </summary>
        public int MaxLOD;

        /// <summary> ユーザーが選択した範囲のなかで存在するLODの最大値 </summary>
        public readonly int AvailableMaxLOD;
        
        public MeshGranularity MeshGranularity;
        public bool DoSetMeshCollider;
        public bool DoSetAttrInfo;

        public PackageLoadSetting(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture, int minLOD, int maxLOD, int availableMaxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo)
        {
            Package = package;
            this.LoadPackage = loadPackage;
            this.IncludeTexture = includeTexture;
            this.MinLOD = minLOD;
            this.MaxLOD = maxLOD;
            this.AvailableMaxLOD = availableMaxLOD;
            this.MeshGranularity = meshGranularity;
            this.DoSetMeshCollider = doSetMeshCollider;
            this.DoSetAttrInfo = doSetAttrInfo;
        }
    }

}
