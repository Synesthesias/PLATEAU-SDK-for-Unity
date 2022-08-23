using System;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    public class Model
    {
        private IntPtr handle;
        
        public Model(IntPtr handle)
        {
            this.handle = handle;
        }

        public int RootNodesCount
        {
            get
            {
                int childCount = DLLUtil.GetNativeValue<int>(this.handle,
                    NativeMethods.plateau_model_get_root_nodes_count);
                return childCount;
            }
        }

        public Node GetRootNodeAt(int index)
        {
            var nodePtr = DLLUtil.GetNativeValue<IntPtr>(
                this.handle, index,
                NativeMethods.plateau_model_get_root_node_at_index);
            return new Node(nodePtr);
        }
    }
}