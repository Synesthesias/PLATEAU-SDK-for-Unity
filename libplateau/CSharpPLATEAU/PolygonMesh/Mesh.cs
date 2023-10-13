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

        internal Mesh(IntPtr handle)
        {
            Handle = handle;
        }

        public static Mesh Create(string meshID)
        {
            var result = NativeMethods.plateau_create_mesh(out var newMeshPtr);
            DLLUtil.CheckDllError(result);
            return new Mesh(newMeshPtr);
        }

        public static Mesh Create(PlateauVector3d[] vertices, uint[] indices, PlateauVector2f[] uv1,
            PlateauVector2f[] uv4, SubMesh[] subMeshes)
        {
            var subMeshPointers = subMeshes.Select(sm => sm.Handle).ToArray();
            var result = NativeMethods.plateau_create_mesh_9(
                out var newMeshPtr,
                vertices, vertices.Length, indices, indices.Length, uv1, uv1.Length,
                uv4, uv4.Length,
                subMeshPointers, subMeshes.Length
            );
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

        public CityObjectList CityObjectList
        {
            get
            {
                ThrowIfInvalid();
                var result = NativeMethods.plateau_mesh_get_city_object_list(Handle, out var cityObjectListPtr);
                DLLUtil.CheckDllError(result);
                return new CityObjectList(cityObjectListPtr);
            }
            set
            {
                ThrowIfInvalid();
                var result = NativeMethods.plateau_mesh_set_city_object_list(Handle, value.Handle);
                DLLUtil.CheckDllError(result);
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

        public PlateauVector2f[] GetUv4()
        {
            ThrowIfInvalid();
            var uv4 = new PlateauVector2f[VerticesCount];
            var result = NativeMethods.plateau_mesh_get_uv4(Handle, uv4);
            DLLUtil.CheckDllError(result);
            return uv4;
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
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_mesh(
                out IntPtr newMeshPtr);


            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_mesh_9(
                out IntPtr meshPtr,
                [In] PlateauVector3d[] vertices,
                int verticesCount,
                [In] uint[] indices,
                int indicesCount,
                [In] PlateauVector2f[] uv1,
                int uv1Count,
                [In] PlateauVector2f[] uv4,
                int uv4Count,
                [In] IntPtr[] subMeshPointers,
                int subMeshCount
                );

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
            internal static extern APIResult plateau_mesh_get_city_object_list(
                [In] IntPtr plateauMeshPtr,
                out IntPtr cityObjectListPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_uv1(
                [In] IntPtr plateauMeshPtr,
                [Out] PlateauVector2f[] outUvPosArray);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_get_uv4(
                [In] IntPtr plateauMeshPtr,
                [Out] PlateauVector2f[] outUvPosArray);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_mesh_add_sub_mesh(
                [In] IntPtr meshPtr,
                [In] string texturePath,
                int subMeshStartIndex,
                int subMeshEndIndex);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_mesh_set_city_object_list(
                [In] IntPtr meshPtr,
                [In] IntPtr cityObjListPtr
            );

            // ***************
            //  mesh_merger_c.cpp
            // ***************

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_mesh_merger_merge_mesh(
                [In] IntPtr meshPtr,
                [In] IntPtr otherMeshPtr,
                [MarshalAs(UnmanagedType.U1)] bool invertMeshFrontBack,
                [MarshalAs(UnmanagedType.U1)] bool includeTexture);
        }
    }
}
