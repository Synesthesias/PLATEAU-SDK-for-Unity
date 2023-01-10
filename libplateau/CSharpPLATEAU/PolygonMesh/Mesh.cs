using System;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.Util;

namespace PLATEAU.PolygonMesh
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
        public IntPtr Handle { get; }
        private bool isValid = true;

        public Mesh(IntPtr handle)
        {
            Handle = handle;
        }

        public static Mesh Create(string meshID)
        {
            var result = NativeMethods.plateau_create_mesh(out var newMeshPtr, meshID);
            DLLUtil.CheckDllError(result);
            return new Mesh(newMeshPtr);
        }

        public int VerticesCount
        {
            get
            {
                ThrowIfInvalid();
                int verticesCount = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_mesh_get_vertices_count);
                return verticesCount;
            }
        }

        public int IndicesCount
        {
            get
            {
                ThrowIfInvalid();
                int indicesCount = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_mesh_get_indices_count);
                return indicesCount;
            }
        }

        public PlateauVector3d GetVertexAt(int index)
        {
            ThrowIfInvalid();
            var vert = DLLUtil.GetNativeValue<PlateauVector3d>(Handle, index,
                NativeMethods.plateau_mesh_get_vertex_at_index);
            return vert;
        }

        public int GetIndiceAt(int index)
        {
            ThrowIfInvalid();
            int vertexId = DLLUtil.GetNativeValue<int>(Handle, index,
                NativeMethods.plateau_mesh_get_indice_at_index);
            return vertexId;
        }

        public PlateauVector2f[] GetUv1()
        {
            ThrowIfInvalid();
            var uv1 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv1(Handle, uv1);
            DLLUtil.CheckDllError(result);
            return uv1;
        }

        public PlateauVector2f[] GetUv2()
        {
            ThrowIfInvalid();
            var uv2 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv2(Handle, uv2);
            DLLUtil.CheckDllError(result);
            return uv2;
        }

        public PlateauVector2f[] GetUv3()
        {
            ThrowIfInvalid();
            var uv3 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv3(Handle, uv3);
            DLLUtil.CheckDllError(result);
            return uv3;
        }

        public int SubMeshCount
        {
            get
            {
                ThrowIfInvalid();
                int numSubMesh = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_mesh_get_sub_mesh_count);
                return numSubMesh;
            }
        }

        public SubMesh GetSubMeshAt(int index)
        {
            ThrowIfInvalid();
            var subMeshPtr = DLLUtil.GetNativeValue<IntPtr>(Handle, index,
                NativeMethods.plateau_mesh_get_sub_mesh_at_index);
            return new SubMesh(subMeshPtr);
        }

        public void MergeMesh(Mesh otherMesh, bool includeTexture)
        {
            ThrowIfInvalid();
            var result = NativeMethods.plateau_mesh_merger_merge_mesh(
                Handle, otherMesh.Handle, false, includeTexture
            );
            DLLUtil.CheckDllError(result);
        }

        public void MergeMeshInfo(PlateauVector3d[] vertices, uint[] indices, PlateauVector2f[] uv1,
            SubMesh[] subMeshes, CoordinateSystem meshAxisConvertFrom, CoordinateSystem meshAxisConvertTo,
            bool includeTexture)
        {
            ThrowIfInvalid();
            var subMeshPointers = subMeshes.Select(sm => sm.Handle).ToArray();
            var result = NativeMethods.plateau_mesh_merger_mesh_info(
                Handle,
                vertices, vertices.Length, indices, indices.Length, uv1, uv1.Length,
                subMeshPointers, subMeshes.Length,
                meshAxisConvertFrom, meshAxisConvertTo, includeTexture
            );
            DLLUtil.CheckDllError(result);
        }

        public void AddSubMesh(string texturePath, int subMeshStartIndex, int subMeshEndIndex)
        {
            var result = NativeMethods.plateau_mesh_add_sub_mesh(
                Handle, texturePath, subMeshStartIndex, subMeshEndIndex);
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 取扱注意:
        /// 通常は <see cref="Node"/> が廃棄されるときに C++側で <see cref="Mesh"/> も廃棄されるので、このメソッドを呼ぶ必要はありません。
        /// <see cref="Node"/> に属さず、C#側で明示的に Create した <see cref="Mesh"/> のみ <see cref="Dispose"/> してください。
        /// それ以外のタイミングで呼ぶとメモリ違反でUnityが落ちます。
        /// </summary>
        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_mesh);
        }

        public void MarkInvalid()
        {
            this.isValid = false;
        }

        private void ThrowIfInvalid()
        {
            if (!this.isValid) throw new Exception("Mesh is invalid.");
        }

        public void DebugString(StringBuilderWithIndent sb)
        {
            sb.AppendLine($"Mesh : vertCount = {VerticesCount}");
            sb.IncrementIndent();
            for (int i = 0; i < SubMeshCount; i++)
            {
                GetSubMeshAt(i).DebugPrint(sb);
            }

            sb.DecrementIndent();
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_create_mesh(
                out IntPtr newMeshPtr,
                string meshID);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_mesh(
                [In] IntPtr handle);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_vertices_count(
                [In] IntPtr handle,
                out int outVerticesCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_vertex_at_index(
                [In] IntPtr handle,
                out PlateauVector3d outVertPos,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_indices_count(
                [In] IntPtr handle,
                out int outIndicesCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_indice_at_index(
                [In] IntPtr handle,
                out int vertexId,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_sub_mesh_count(
                [In] IntPtr plateauMeshPtr,
                out int subMeshCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_sub_mesh_at_index(
                [In] IntPtr plateauMeshPtr,
                out IntPtr plateauSubMeshPtr,
                int index);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_uv1(
                [In] IntPtr plateauMeshPtr,
                [Out] PlateauVector2f[] outUvPosArray);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_uv2(
                [In] IntPtr plateauMeshPtr,
                [Out] PlateauVector2f[] outUvPosArray);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_uv3(
                [In] IntPtr plateauMeshPtr,
                [Out] PlateauVector2f[] outUvPosArray);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_mesh_add_sub_mesh(
                [In] IntPtr meshPtr,
                [In] string texturePath,
                int subMeshStartIndex,
                int subMeshEndIndex);
            
            // ***************
            //  mesh_merger_c.cpp
            // ***************

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_merger_merge_mesh(
                [In] IntPtr meshPtr,
                [In] IntPtr otherMeshPtr,
                [MarshalAs(UnmanagedType.U1)] bool invertMeshFrontBack,
                [MarshalAs(UnmanagedType.U1)] bool includeTexture);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_merger_mesh_info(
                [In] IntPtr meshPtr,
                [In] PlateauVector3d[] vertices,
                int verticesCount,
                [In] uint[] indices,
                int indicesCount,
                [In] PlateauVector2f[] uv1,
                int uv1Count,
                [In] IntPtr[] subMeshPointers,
                int subMeshCount,
                CoordinateSystem meshAxisConvertFrom,
                CoordinateSystem meshAxisConvertTo,
                [MarshalAs(UnmanagedType.U1)] bool includeTexture);
        }
    }
}
