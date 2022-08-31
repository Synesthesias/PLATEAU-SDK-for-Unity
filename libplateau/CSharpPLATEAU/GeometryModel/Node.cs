using System;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    /// <summary>
    /// <see cref="Model"/> 以下の階層構造を構成するノードです。
    /// Node は 0個以上の 子Node を持つため階層構造になります。
    ///
    /// 詳しくは <see cref="Model"/> クラスのコメントをご覧ください。
    ///
    /// Name はゲームエンジン側ではゲームオブジェクトの名前として解釈されることが想定されます。
    /// Mesh はそのゲームオブジェクトの持つメッシュとして解釈されることが想定されます。
    /// </summary>
    public class Node
    {
        private readonly IntPtr handle;

        public Node(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// ノードの名称を返します。
        /// ゲームエンジン側ではゲームオブジェクトの名称となります。
        /// </summary>
        public string Name
        {
            get
            {
                string name = DLLUtil.GetNativeString(this.handle,
                    NativeMethods.plateau_node_get_name);
                return name;
            }
        }

        /// <summary>
        /// 子ノードの数を返します。
        /// </summary>
        public int ChildCount
        {
            get
            {
                int childCount = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_node_get_child_count);
                return childCount;
            }
        }

        /// <summary>
        /// <paramref name="index"/> 番目の子ノードを返します。
        /// </summary>
        public Node GetChildAt(int index)
        {
            var childNodePtr = DLLUtil.GetNativeValue<IntPtr>(
                this.handle, index,
                NativeMethods.plateau_node_get_child_at_index);
            return new Node(childNodePtr);
        }

        /// <summary>
        /// ノードが保有する <see cref="Mesh"/> を返します。
        /// なければ null を返します。
        /// </summary>
        public Mesh Mesh
        {
            get
            {
                var result = NativeMethods.plateau_node_get_mesh(
                    this.handle, out IntPtr meshPtr);
                if (result == APIResult.ErrorValueNotFound)
                {
                    return null;
                }
                DLLUtil.CheckDllError(result);
                return new Mesh(meshPtr);
            }
        }
    }
}
