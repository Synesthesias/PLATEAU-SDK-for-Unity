using System;
using System.Collections.Generic;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    /// <summary>
    /// DLLから受け取ったメッシュ・テクスチャ情報です。
    /// </summary>
    public class Mesh
    {
        private readonly IntPtr handle;
        internal Mesh(IntPtr handle)
        {
            this.handle = handle;
        }

        public int VerticesCount
        {
            get
            {
                int verticesCount = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_mesh_get_vertices_count);
                return verticesCount;
            }
        }

        public int IndicesCount
        {
            get
            {
                int indicesCount = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_mesh_get_indices_count);
                return indicesCount;
            }
        }

        public PlateauVector3d GetVertexAt(int index)
        {
            var vert = DLLUtil.GetNativeValue<PlateauVector3d>(this.handle, index,
                NativeMethods.plateau_mesh_get_vertex_at_index);
            return vert;
        }

        public int GetIndiceAt(int index)
        {
            int vertexId = DLLUtil.GetNativeValue<int>(this.handle, index,
                NativeMethods.plateau_mesh_get_indice_at_index);
            return vertexId;
        }

        public PlateauVector2f[] GetUv1()
        {
            var uv1 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv1(this.handle, uv1);
            DLLUtil.CheckDllError(result);
            return uv1;
        }
        
        public PlateauVector2f[] GetUv2()
        {
            var uv2 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv2(this.handle, uv2);
            DLLUtil.CheckDllError(result);
            return uv2;
        }
        
        public PlateauVector2f[] GetUv3()
        {
            var uv3 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv3(this.handle, uv3);
            DLLUtil.CheckDllError(result);
            return uv3;
        }

        public MultiTexture GetMultiTexture()
        {
            int numTexture = DLLUtil.GetNativeValue<int>(this.handle,
                NativeMethods.plateau_mesh_get_multi_texture_count);
            var vertexIndices = new int[numTexture];
            var textureUrlStrPointers = new IntPtr[numTexture];
            var textureUrlStrLengthArray = new int[numTexture];
            var result = NativeMethods.plateau_mesh_get_multi_texture(
                this.handle, vertexIndices, textureUrlStrPointers, textureUrlStrLengthArray
            );
            DLLUtil.CheckDllError(result);
            var textureUrls = DLLUtil.ReadNativeStrPtrArray(textureUrlStrPointers, textureUrlStrLengthArray);
            return new MultiTexture(vertexIndices, textureUrls);
        }
        
        public class MultiTexture
        {
            private readonly SubTexture[] subTextures;
            public int Length => this.subTextures.Length;
            public SubTexture this[int index] => this.subTextures[index];

            public MultiTexture(IReadOnlyList<int> vertexIndices, IReadOnlyList<string> textureUrlArray)
            {
                
                int num = vertexIndices.Count;
                if (num != textureUrlArray.Count)
                {
                    throw new ArgumentException($"{nameof(MultiTexture)} : argument array length do not match.");
                }
                this.subTextures = new SubTexture[num];
                for (int i = 0; i < num; i++)
                {
                    this.subTextures[i] = new SubTexture(vertexIndices[i], textureUrlArray[i]);
                }
            }
            
            public class SubTexture
            {
                public readonly int VertexIndex;
                public readonly string TextureUrl;

                public SubTexture(int vertexIndex, string textureUrl)
                {
                    this.VertexIndex = vertexIndex;
                    this.TextureUrl = textureUrl;
                }
            }
        }
    }
}