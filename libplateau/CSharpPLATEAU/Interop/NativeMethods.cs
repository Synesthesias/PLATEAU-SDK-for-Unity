using System;

namespace PLATEAU.Interop
{
    
    // TODO あとで消す、C++側も一緒に消す
    // public enum MeshFileFormat{OBJ, GLTF, FBX}

    
    // [StructLayout(LayoutKind.Sequential)]
    // public struct MeshConvertOptionsData
    // {
    //     public CoordinateSystem MeshAxes;
    //     public PlateauVector3d ReferencePoint;
    //     public MeshGranularity MeshGranularity;
    //     public uint MinLOD;
    //     public uint MaxLOD;
    //     [MarshalAs(UnmanagedType.U1)] public bool ExportLowerLOD;
    //     [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
    //     public float UnitScale;
    //     public MeshFileFormat MeshFileFormat;
    //     public int CoordinateZoneID;
    // }
    
    // internal static class NativeMethods
    // {
    //     public const string DllName = "plateau";
    //
    //     
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_create_mesh_converter(
    //         out IntPtr outHandle
    //     );
    //
    //     [DllImport(DllName)]
    //     internal static extern void plateau_delete_mesh_converter([In] IntPtr meshConverter);
    //
    //     [DllImport(DllName, CharSet = CharSet.Ansi)]
    //     internal static extern APIResult plateau_mesh_converter_convert(
    //         [In] IntPtr meshConverter,
    //         [In] string destinationDirectory,
    //         [In] string gmlPath,
    //         [In] IntPtr cityModel,
    //         [In] IntPtr logger);
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_mesh_converter_get_last_exported_model_file_names_count(
    //         [In] IntPtr meshConverter,
    //         out int fileCount);
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_mesh_converter_get_last_exported_model_file_names(
    //         [In] IntPtr meshConverter,
    //         [In, Out] IntPtr[] strPointers,
    //         [Out] int[] outStrSizes);
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_mesh_converter_set_options(
    //         [In] IntPtr meshConverter,
    //         MeshConvertOptionsData value);
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_mesh_converter_get_options(
    //         [In] IntPtr meshConverter,
    //         out MeshConvertOptionsData value);
    //
    //     [DllImport(DllName)]
    //     internal static extern MeshConvertOptionsData plateau_create_mesh_convert_options();
    //
    //     [DllImport(DllName)]
    //     internal static extern APIResult plateau_mesh_convert_options_set_valid_reference_point(
    //         [In, Out] ref MeshConvertOptionsData options,
    //         [In] IntPtr cityModel);
    //
    //     // ***************
    //     //  available_lod_searcher_c.cpp
    //     // ***************
    //
    //     [DllImport(DllName, CharSet = CharSet.Ansi)]
    //     internal static extern APIResult plateau_lod_searcher_search_lods_in_file(
    //         [In] byte[] filePathUtf8,
    //         out uint outLodFlags);
    //     
    // }
}
