using System;
using System.Collections.Generic;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    /// <summary>
    /// メッシュ情報です。
    /// Unity や Unreal Engine でメッシュを生成するために必要な情報が含まれるよう意図されています。
    /// 具体的には 頂点リスト、Indicesリスト、UV、サブメッシュ（含テクスチャ）があります。
    ///
    /// 詳しくは <see cref="Model"/> クラスのコメントをご覧ください。
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

        public int SubMeshCount
        {
            get
            {
                int numSubMesh = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_mesh_get_sub_mesh_count);
                return numSubMesh;
            }
        }
        
        public SubMesh GetSubMeshAt(int index)
        {
            var subMeshPtr = DLLUtil.GetNativeValue<IntPtr>(this.handle, index,
                NativeMethods.plateau_mesh_get_sub_mesh_at_index);
            return new SubMesh(subMeshPtr);
        }
    }
}