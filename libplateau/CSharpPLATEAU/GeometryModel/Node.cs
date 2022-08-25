using System;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    // TODO コメントを書く
    public class Node
    {
        private readonly IntPtr handle;

        public Node(IntPtr handle)
        {
            this.handle = handle;
        }

        public string Name
        {
            get
            {
                string name = DLLUtil.GetNativeString(this.handle,
                    NativeMethods.plateau_node_get_name);
                return name;
            }
        }

        public int ChildCount
        {
            get
            {
                int childCount = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_node_get_child_count);
                return childCount;
            }
        }

        public Node GetChildAt(int index)
        {
            var childNodePtr = DLLUtil.GetNativeValue<IntPtr>(
                this.handle, index,
                NativeMethods.plateau_node_get_child_at_index);
            return new Node(childNodePtr);
        }

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