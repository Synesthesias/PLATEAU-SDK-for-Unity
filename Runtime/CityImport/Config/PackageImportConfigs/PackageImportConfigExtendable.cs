using System;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityImport.Config.PackageImportConfigs
{
    /// <summary>
    /// <see cref="PackageImportConfig"/>のうち、一括設定から設定を引き継ぐことが可能な部分の設定値クラスです。
    /// </summary>
    public class PackageImportConfigExtendable
    {
        public bool IncludeTexture { get; set; }
        public MeshGranularity MeshGranularity { get; set; }
        public bool DoSetMeshCollider { get; set; }
        public bool DoSetAttrInfo { get; set; }
        public bool EnableTexturePacking { get; set; }
        public TexturePackingResolution TexturePackingResolution { get; set; }

        public PackageImportConfigExtendable()
            : this(true, MeshGranularity.PerPrimaryFeatureObject, true, true, true,
                TexturePackingResolution.W2048H2048){}

        public PackageImportConfigExtendable(
            bool includeTexture, MeshGranularity meshGranularity,
            bool doSetMeshCollider, bool doSetAttrInfo,
            bool enableTexturePacking, TexturePackingResolution texturePackingResolution)
        {
            IncludeTexture = includeTexture;
            MeshGranularity = meshGranularity;
            DoSetMeshCollider = doSetMeshCollider;
            DoSetAttrInfo = doSetAttrInfo;
            EnableTexturePacking = enableTexturePacking;
            TexturePackingResolution = texturePackingResolution;
        }

        public void CopyFrom(PackageImportConfigExtendable other)
        {
            IncludeTexture = other.IncludeTexture;
            MeshGranularity = other.MeshGranularity;
            DoSetMeshCollider = other.DoSetMeshCollider;
            DoSetAttrInfo = other.DoSetAttrInfo;
            EnableTexturePacking = other.EnableTexturePacking;
            TexturePackingResolution = other.TexturePackingResolution;
        }
        

        public uint GetTexturePackingResolution()
        {
            return TexturePackingResolution switch
            {
                TexturePackingResolution.W2048H2048 => 2048,
                TexturePackingResolution.W4096H4096 => 4096,
                TexturePackingResolution.W8192H8192 => 8192,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    /// <summary>
    /// テクスチャ結合後の解像度です。
    /// </summary>
    public enum TexturePackingResolution : int
    {
        W2048H2048 = 0,
        W4096H4096 = 1,
        W8192H8192 = 2,
    }
    
    internal static class TexturePackingResolutionExtension
    {
        public static int ToPixelCount(this TexturePackingResolution op)
        {
            return op switch
            {
                TexturePackingResolution.W2048H2048 => 2048,
                TexturePackingResolution.W4096H4096 => 4096,
                TexturePackingResolution.W8192H8192 => 8192,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}