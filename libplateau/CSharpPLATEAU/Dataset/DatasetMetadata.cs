using System;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    public class DatasetMetadata
    {
        public IntPtr Handle { get; }
        public DatasetMetadata(IntPtr ptr)
        {
            Handle = ptr;
        }
        
        public static DatasetMetadata Create()
        {
            return new DatasetMetadata(
                DLLUtil.PtrOfNewInstance(NativeMethods.plateau_create_dataset_metadata));
        }

        public string ID
        {
            get
            {
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_id);
            }
        }

        public string Title
        {
            get
            {
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_title);
            }
        }
        
        public string Description
        {
            get
            {
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_description);
            }
        }

        public int MaxLOD
        {
            get
            {
                return DLLUtil.GetNativeValue<int>(Handle, NativeMethods.plateau_dataset_metadata_get_max_lod);
            }
        }
        
        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_dataset_metadata);
        }
    }
}
