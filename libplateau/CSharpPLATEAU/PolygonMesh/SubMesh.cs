using System;
using PLATEAU.Interop;

namespace PLATEAU.PolygonMesh
{
    /// <summary>
    /// SubMesh は、 <see cref="Mesh"/> の一部 (Indices リストの中のとある範囲)がとあるテクスチャであることを表現します。
    ///
    /// 詳しくは <see cref="Model"/> クラスのコメントをご覧ください。
    /// </summary>
    public class SubMesh
    {
        private IntPtr handle;
        public SubMesh(IntPtr handle)
        {
            this.handle = handle;
        }

        public int StartIndex
        {
            get
            {
                int startIndex = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_sub_mesh_get_start_index);
                return startIndex;
            }
        }

        public int EndIndex
        {
            get
            {
                int endIndex = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_sub_mesh_get_end_index);
                return endIndex;
            }
        }

        public string TexturePath
        {
            get
            {
                string path = DLLUtil.GetNativeString(this.handle,
                    NativeMethods.plateau_sub_mesh_get_texture_path);
                return path;
            }
        }
    }
}
