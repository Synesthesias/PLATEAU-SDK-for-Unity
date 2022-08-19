using System;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// LibCityGml の <see cref="Polygon"/> を継承して PLATEAU向けに機能拡張したものです。
    /// </summary>
    public class PlateauPolygon : Polygon
    {
        internal PlateauPolygon(IntPtr handle) : base(handle)
        {
        }

        public PlateauVector2f[] GetUv1()
        {
            var uv1 = new PlateauVector2f[VertexCount];
            var result = NativeMethods.plateau_polygon_get_uv1(Handle, uv1);
            DLLUtil.CheckDllError(result);
            return uv1;
        }
        
        public PlateauVector2f[] GetUv2()
        {
            var uv2 = new PlateauVector2f[VertexCount];
            var result = NativeMethods.plateau_polygon_get_uv2(Handle, uv2);
            DLLUtil.CheckDllError(result);
            return uv2;
        }
        
        public PlateauVector2f[] GetUv3()
        {
            var uv3 = new PlateauVector2f[VertexCount];
            var result = NativeMethods.plateau_polygon_get_uv3(Handle, uv3);
            DLLUtil.CheckDllError(result);
            return uv3;
        }

        public MultiTexture GetMultiTexture()
        {
            int numTexture = DLLUtil.GetNativeValue<int>(Handle,
                NativeMethods.plateau_polygon_get_multi_texture_count);
            var vertexIndices = new int[numTexture];
            var texturePtrArray = new IntPtr[numTexture];
            var result = NativeMethods.plateau_polygon_get_multi_texture(
                Handle, vertexIndices, texturePtrArray
            );
            DLLUtil.CheckDllError(result);
            return new MultiTexture(vertexIndices, texturePtrArray);
        }
        
        public class MultiTexture
        {
            private readonly SubTexture[] subTextures;
            public int Length => this.subTextures.Length;
            public SubTexture this[int index] => this.subTextures[index];

            public MultiTexture(int[] vertexIndices, IntPtr[] texturePtrArray)
            {
                int num = vertexIndices.Length;
                this.subTextures = new SubTexture[num];
                for (int i = 0; i < num; i++)
                {
                    this.subTextures[i] = new SubTexture(vertexIndices[i], new Texture(texturePtrArray[i]));
                }
            }
            
            public class SubTexture
            {
                public readonly int VertexIndex;
                public readonly Texture Texture;

                public SubTexture(int vertexIndex, Texture texture)
                {
                    this.VertexIndex = vertexIndex;
                    this.Texture = texture;
                }
            }
        }
    }
}