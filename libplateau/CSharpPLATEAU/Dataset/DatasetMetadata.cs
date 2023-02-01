using System;
using System.Linq;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    public class DatasetMetadata
    {
        private IntPtr Handle { get; }
        public DatasetMetadata(IntPtr ptr)
        {
            Handle = ptr;
        }
        
        public static DatasetMetadata Create()
        {
            return new DatasetMetadata(
                DLLUtil.PtrOfNewInstance(NativeMethods.plateau_create_dataset_metadata));
        }

        public string ID => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_id);

        public string Title => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_title);

        public string Description => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_dataset_metadata_get_description);


        public string[] FeatureTypes
        {
            get
            {
                var nativeFeatureTypes = NativeVectorString.Create();
                var result = NativeMethods.plateau_dataset_metadata_get_feature_types(
                    Handle, nativeFeatureTypes.Handle);
                DLLUtil.CheckDllError(result);
                var ret = nativeFeatureTypes.ToCSharpArray();
                nativeFeatureTypes.Dispose();
                return ret;
            }
        }

        public PredefinedCityModelPackage PackageFlags
        {
            get
            {
                var packages = FeatureTypes
                    .Select(DatasetAccessor.FeatureTypeToPackage)
                    .Distinct();
                PredefinedCityModelPackage flags = 0u;
                foreach (var package in packages)
                {
                    flags |= package;
                }

                return flags;
            }
        }
        
        public void Dispose()
        {
            DLLUtil.ExecNativeVoidFunc(Handle,
                NativeMethods.plateau_delete_dataset_metadata);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_dataset_metadata(
                out IntPtr outDatasetMetadataPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_dataset_metadata(
                [In] IntPtr datasetMetadataPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_get_id(
                [In] IntPtr datasetMetadataPtr,
                out IntPtr outStrPtr,
                out int strLength);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_get_title(
                [In] IntPtr datasetMetadataPtr,
                out IntPtr outStrPtr,
                out int strLength);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_get_description(
                [In] IntPtr datasetMetadataPtr,
                out IntPtr outStrPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_dataset_metadata_get_feature_types(
                [In] IntPtr datasetMetadataPtr,
                [In,Out] IntPtr refNativeVectorStringPtr);
        }
    }
}
