using System;
using PLATEAU.Dataset;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Setting
{
    /// <summary>
    /// テクスチャ結合後の解像度です。
    /// </summary>
    internal enum TexturePackingResolution : int
    {
        W2048H2048 = 0,
        W4096H4096 = 1,
        W8192H8192 = 2,
    }

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
        public bool EnableTexturePacking;
        public TexturePackingResolution TexturePackingResolution;

        public PackageLoadSetting(PredefinedCityModelPackage package, bool loadPackage, bool includeTexture, int minLOD, int maxLOD, int availableMaxLOD, MeshGranularity meshGranularity, bool doSetMeshCollider, bool doSetAttrInfo, bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
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
            this.EnableTexturePacking = enableTexturePacking;
            this.TexturePackingResolution = texturePackingResolution;
        }
        
        public uint GetTexturePackingResolution()
        {
            switch (TexturePackingResolution)
            {
                case TexturePackingResolution.W2048H2048:
                    return 2048;
                case TexturePackingResolution.W4096H4096:
                    return 4096;
                case TexturePackingResolution.W8192H8192:
                    return 8192;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
