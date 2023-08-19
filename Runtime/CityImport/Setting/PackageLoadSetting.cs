using System;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、パッケージごとの設定です。
    /// <see cref="CityLoadConfig"/> によって保持されます。
    /// </summary>
    internal class PackageLoadSetting
    {
        public bool LoadPackage;
        public bool IncludeTexture;
        public int MinLOD;
        public int MaxLOD;
        public MeshGranularity MeshGranularity;
        public bool DoSetMeshCollider;
        public bool DoSetAttrInfo;
        /// <summary> GUIで設定を表示する(true)か、折りたたむ(false)か </summary>
        [NonSerialized] public bool GuiFoldOutState = true;

        public PackageLoadSetting(bool loadPackage, bool includeTexture, int minLOD, int maxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo)
        {
            this.LoadPackage = loadPackage;
            this.IncludeTexture = includeTexture;
            this.MinLOD = minLOD;
            this.MaxLOD = maxLOD;
            this.MeshGranularity = meshGranularity;
            this.DoSetMeshCollider = doSetMeshCollider;
            this.DoSetAttrInfo = doSetAttrInfo;
        }
    }

}
