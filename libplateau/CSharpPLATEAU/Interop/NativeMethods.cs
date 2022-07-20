using System;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.IO;

// 文字列のサイズをDLLでやりとりする時の型を決めます。
using DllStrSizeT = System.Int32;

namespace PLATEAU.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PlateauVector3d
    {
        public double X;
        public double Y;
        public double Z;

        public PlateauVector3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return $"({this.X}, {this.Y}, {this.Z})";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlateauVector2f
    {
        public float X;
        public float Y;

        public PlateauVector2f(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"({this.X}, {this.Y})";
        }
    }

    /// <summary>
    /// GMLファイルのパース時の設定です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CitygmlParserParams
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool Optimize;
        /// <summary>
        /// <see cref="Tessellate"/> を false に設定すると、 <see cref="Polygon"/> が頂点を保持する代わりに <see cref="LinearRing"/> を保持することがあります。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool Tessellate;

        public CitygmlParserParams(bool optimize, bool tessellate = true)
        {
            this.Optimize = optimize;
            this.Tessellate = tessellate;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct MeshConvertOptionsData
    {
        public AxesConversion MeshAxes;
        public PlateauVector3d ReferencePoint;
        public MeshGranularity MeshGranularity;
        public uint MinLOD;
        public uint MaxLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportLowerLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        public float UnitScale;
    }

    public enum APIResult
    {
        Success,
        ErrorUnknown,
        ErrorValueNotFound,
        ErrorLoadingCityGml
    }

    public enum DllLogLevel
    {
        Error = 4,
        Warning = 3,
        Info = 2,
        Debug = 1,
        Trace = 0
    }

    public delegate void LogCallbackFuncType(IntPtr textPtr);

    internal static class NativeMethods
    {
        private const string DllName = "plateau";

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_load_citygml(
            [In] string gmlPath,
            [In] CitygmlParserParams parserParams,
            out IntPtr cityModelHandle,
            DllLogLevel logLevel,
            IntPtr logErrorCallbackFuncPtr,
            IntPtr logWarnCallbackFuncPtr,
            IntPtr logInfoCallbackFuncPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_create_mesh_converter(
            out IntPtr outHandle
            );

        [DllImport(DllName)]
        internal static extern void plateau_delete_mesh_converter([In] IntPtr meshConverter);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_mesh_converter_convert(
            [In] IntPtr meshConverter,
            [In] string destinationDirectory,
            [In] string gmlPath,
            [In] IntPtr cityModel,
            [In] IntPtr logger);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_converter_get_last_exported_model_file_names_count(
            [In] IntPtr meshConverter,
            out int fileCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_converter_get_last_exported_model_file_names(
            [In] IntPtr meshConverter,
            [In, Out] IntPtr[] strPointers,
            [Out] int[] outStrSizes);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_converter_set_options(
            [In] IntPtr meshConverter,
            MeshConvertOptionsData value);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_converter_get_options(
            [In] IntPtr meshConverter,
            out MeshConvertOptionsData value);

        [DllImport(DllName)]
        internal static extern MeshConvertOptionsData plateau_create_mesh_convert_options();

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_convert_options_set_valid_reference_point(
            [In, Out] ref MeshConvertOptionsData options,
            [In] IntPtr cityModel);

        [DllImport(DllName)]
        internal static extern int plateau_delete_city_model(
            [In] IntPtr cityModel);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_get_root_city_objects(
            [In] IntPtr cityModel,
            [In, Out] IntPtr[] cityObjects,
            int count);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_get_root_city_object_count(
            [In] IntPtr cityModel,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_get_all_city_object_count_of_type(
            [In] IntPtr cityModel,
            out int count,
            CityObjectType type);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_get_all_city_objects_of_type(
            [In] IntPtr cityModel,
            [In, Out] IntPtr[] cityObjects,
            CityObjectType type,
            int count);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_city_model_get_city_object_by_id(
            [In] IntPtr handle,
            out IntPtr cityObjectPtr,
            [In] string id);


        // ***************
        //  city_object_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_type(
            [In] IntPtr cityObjectHandle,
            out CityObjectType outCityObjType);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_geometries_count(
            [In] IntPtr cityObjectHandle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_address(
            [In] IntPtr cityObjectHandle,
            out IntPtr addressHandle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_implicit_geometry_count(
            [In] IntPtr cityObjectHandle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_child_city_object_count(
            [In] IntPtr cityObjectHandle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_child_city_object(
            [In] IntPtr cityObjectHandle,
            out IntPtr outChildHandle,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_object_get_geometry(
            [In] IntPtr cityObjectHandle,
            out IntPtr outGeometryHandle,
            int index);

        // ***************
        //  Object_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_object_get_id(
            [In] IntPtr objHandle,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_object_get_id_str_length(
            [In] IntPtr objHandle,
            out DllStrSizeT outLength);


        [DllImport(DllName)]
        internal static extern APIResult plateau_object_get_attributes_map(
            [In] IntPtr objHandle,
            out IntPtr attributesMapPtr);


        // ***************
        //  featureobject_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_feature_object_get_envelope(
            [In] IntPtr featureObject,
            [Out] double[] outEnvelope
        );

        [DllImport(DllName)]
        internal static extern APIResult plateau_feature_object_set_envelope(
            [In] IntPtr featureObject,
            double lowerX, double lowerY, double lowerZ,
            double upperX, double upperY, double upperZ
        );


        // ***************
        //  attributesmap_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_attributes_map_get_keys_count(
            [In] IntPtr attributesMap,
            out int count);


        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_attributes_map_get_keys(
            [In] IntPtr attributesMap,
            [In, Out] IntPtr[] keyHandles,
            [Out] int[] outKeySizes);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_attributes_map_get_attribute_value(
            [In] IntPtr attributesMap,
            [In] byte[] keyUtf8,
            [Out] out IntPtr attrValuePtr);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_attributes_map_do_contains_key(
            [In] IntPtr attributesMap,
            [In] byte[] keyUtf8,
            out bool doContainsKey);

        [DllImport(DllName)]
        internal static extern APIResult plateau_attributes_map_to_string_size(
            [In] IntPtr attributesMap,
            out int size);

        [DllImport(DllName)]
        internal static extern APIResult plateau_attributes_map_to_string(
            [In] IntPtr attributesMap,
            [In,Out] IntPtr outStrPtrUtf8);


        // ***************
        //  attributevalue_c.cpp
        // ***************

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_attribute_value_get_string(
            [In] IntPtr attributeValue,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_attribute_value_get_type(
            [In] IntPtr attributeValue,
            out AttributeType attrType);

        [DllImport(DllName)]
        internal static extern APIResult plateau_attribute_as_attribute_set(
            [In] IntPtr attributeValue,
            [Out] out IntPtr attrSetPtr);


        // ***************
        //  address_c.cpp
        // ***************
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_address_get_country(
            [In] IntPtr addressHandle,
            out IntPtr outCountryNamePtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_address_get_locality(
            [In] IntPtr addressHandle,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_address_get_postal_code(
            [In] IntPtr addressHandle,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_address_get_thoroughfare_name(
            [In] IntPtr addressHandle,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_address_get_thoroughfare_number(
            [In] IntPtr addressHandle,
            out IntPtr outStrPtr,
            out int strLength);

        // ***************
        //  geometry_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_type(
            [In] IntPtr geometryHandle,
            out GeometryType type);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_geometries_count(
            [In] IntPtr geometryHandle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_polygons_count(
            [In] IntPtr geometryHandle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_child_geometry(
            [In] IntPtr geometryHandle,
            out IntPtr childGeomHandle,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_polygon(
            [In] IntPtr geometryHandle,
            out IntPtr polygonHandle,
            int index
        );

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_lod(
            [In] IntPtr geometryHandle,
            out int outLod);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_srs_name(
            [In] IntPtr geometryHandle,
            out IntPtr outNameStrPtr,
            out int outStrLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_srs_name_str_length(
            [In] IntPtr geometryHandle,
            out DllStrSizeT outLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_get_line_string_count(
            [In] IntPtr handle,
            out int outCount);

        // ***************
        //  polygon_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_vertex_count(
            [In] IntPtr polygonHandle,
            out int outVertCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_vertex(
            [In] IntPtr handle,
            out PlateauVector3d outVertex,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_indices_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_index_of_indices(
            [In] IntPtr handle,
            out int outIndex,
            int indexOfIndicesList);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_exterior_ring(
            [In] IntPtr handle,
            out IntPtr ringHandle);

        // ***************
        //  linearring_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_linear_ring_get_vertex_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_linear_ring_get_vertex(
            [In] IntPtr handle,
            out PlateauVector3d outVert3d,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_interior_ring_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_polygon_get_interior_ring(
            [In] IntPtr handle,
            out IntPtr outRingHandle,
            int index);


        // ***************
        //  appearancetarget_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names_count(
            [In] IntPtr handle,
            out int outCount,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names(
            [In] IntPtr handle,
            [In, Out] IntPtr outThemeStrArrayHandle,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_texture_theme_names_str_sizes(
            [In] IntPtr handle,
            [Out] int[] outSizeArray,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_material_theme_names_count(
            [In] IntPtr handle,
            out int outCount,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_material_theme_names(
            [In] IntPtr handle,
            [In, Out] IntPtr outThemeStrArrayHandle,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_all_material_theme_names_str_sizes(
            [In] IntPtr handle,
            [Out] int[] outSizeArray,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_appearance_target_get_texture_target_definition_by_theme_name(
            [In] IntPtr handle,
            [Out] out IntPtr outTextureTargetHandle,
            [In] byte[] themeUtf8,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_appearance_target_get_material_target_definition_by_theme_name(
            [In] IntPtr handle,
            [Out] out IntPtr outMaterialTargetHandle,
            [In] byte[] themeNameUtf8,
            [MarshalAs(UnmanagedType.U1)] bool front);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_texture_target_definitions_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_get_texture_target_definition_by_index(
            [In] IntPtr handle,
            out IntPtr outTexTargetDefHandle,
            int index);


        // ***************
        //  texturetargetdefinition_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_target_definition_get_texture_coordinates_count(
            [In] IntPtr handle,
            out int count);

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_target_definition_get_texture_coordinates(
            [In] IntPtr handle,
            out IntPtr outTexCoords,
            int index);

        // ***************
        //  texturecoordinates_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_coordinates_get_coordinate(
            [In] IntPtr handle,
            [Out] out PlateauVector2f outCoord,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_coordinates_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_coordinates_get_target_linear_ring_id(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_coordinates_is_ring_target(
            [In] IntPtr handle,
            [MarshalAs(UnmanagedType.U1)] out bool outIsTarget,
            [In] IntPtr ringHandle);

        // ***************
        //  appearancetargetdefinition_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_definition_tex_get_target_id(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_target_definition_tex_get_appearance(
            [In] IntPtr handle,
            out IntPtr outTextureHandle);

        // ***************
        //  texture_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_get_url(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_texture_get_wrap_mode(
            [In] IntPtr handle,
            out TextureWrapMode outWrapMode);

        // ***************
        //  appearance_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_get_type(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_get_is_front(
            [In] IntPtr handle,
            [MarshalAs(UnmanagedType.U1)] out bool outIsFront);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_get_themes_count(
            [In] IntPtr handle,
            out int count);

        [DllImport(DllName)]
        internal static extern APIResult plateau_appearance_get_themes(
            [In] IntPtr handle,
            [In, Out] IntPtr[] outStrPointers,
            [Out] int[] outStrSizes);


        // ***************
        //  plateau_dll_logger_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_dll_logger(
            out IntPtr outHandle
        );

        [DllImport(DllName)]
        internal static extern void plateau_delete_dll_logger([In] IntPtr dllLogger);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dll_logger_set_callbacks(
            [In] IntPtr handle,
            [In] IntPtr errorCallbackFuncPtr,
            [In] IntPtr warnCallbackPtrFuncPtr,
            [In] IntPtr infoCallbackFuncPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dll_logger_set_log_level(
            [In] IntPtr handle,
            DllLogLevel dllLogLevel);
    }
}