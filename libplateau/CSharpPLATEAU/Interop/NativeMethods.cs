using System;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.IO;
using PLATEAU.Udx;

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

        [MarshalAs(UnmanagedType.U1)]
        public bool IgnoreGeometries;

        public CitygmlParserParams(bool optimize, bool tessellate, bool ignoreGeometries)
        {
            this.Optimize = optimize;
            this.Tessellate = tessellate;
            this.IgnoreGeometries = ignoreGeometries;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct MeshConvertOptionsData
    {
        public CoordinateSystem MeshAxes;
        public PlateauVector3d ReferencePoint;
        public MeshGranularity MeshGranularity;
        public uint MinLOD;
        public uint MaxLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportLowerLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        public float UnitScale;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshExtractOptions
    {
        public PlateauVector3d ReferencePoint;
        public CoordinateSystem MeshAxes;
        public MeshGranularity MeshGranularity;
        public uint MaxLOD;
        public uint MinLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        public int GridCountOfSide;
        public float UnitScale;
        public Extent Extent;

        public MeshExtractOptions(PlateauVector3d referencePoint, CoordinateSystem meshAxes,
            MeshGranularity meshGranularity, uint maxLOD, uint minLOD, bool exportAppearance, int gridCountOfSide,
            float unitScale, Extent extent)
        {
            this.ReferencePoint = referencePoint;
            this.MeshAxes = meshAxes;
            this.MeshGranularity = meshGranularity;
            this.MaxLOD = maxLOD;
            this.MinLOD = minLOD;
            this.ExportAppearance = exportAppearance;
            this.GridCountOfSide = gridCountOfSide;
            this.UnitScale = unitScale;
            this.Extent = extent;
        }

        /// <summary>
        /// 設定の値が正常なら true, 異常な点があれば false を返します。
        /// <param name="failureMessage">異常な点があれば、それを説明する文字列が入ります。正常なら空文字列になります。</param>
        /// </summary>
        public bool Validate(out string failureMessage)
        {
            failureMessage = "";
            if (this.MinLOD > this.MaxLOD)
            {
                failureMessage = $"Validate failed : {nameof(this.MinLOD)} should not greater than {nameof(this.MaxLOD)}.";
                return false;
            }

            if (this.GridCountOfSide <= 0)
            {
                failureMessage = $"Validate failed : {nameof(this.GridCountOfSide)} should be positive number.";
                return false;
            }

            if (Math.Abs(this.UnitScale) < 0.00000001)
            {
                failureMessage = $"Validate failed : {nameof(this.UnitScale)} is too small.";
                return false;
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GeoCoordinate
    {
        public double Latitude;
        public double Longitude;
        public double Height;

        public GeoCoordinate(double lat, double lon, double height)
        {
            this.Latitude = lat;
            this.Longitude = lon;
            this.Height = height;
        }
    }

    /// <summary>
    /// 最小・最大からなる範囲です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Extent
    {
        public GeoCoordinate Min;
        public GeoCoordinate Max;

        public Extent(GeoCoordinate min, GeoCoordinate max)
        {
            this.Min = min;
            this.Max = max;
        }
    }

    public enum APIResult
    {
        Success,
        ErrorUnknown,
        ErrorValueNotFound,
        ErrorLoadingCityGml,
        ErrorIndexOutOfBounds,
        ErrorFileSystem
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
            [In, Out] IntPtr outStrPtrUtf8);


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


        // ***************
        //  mesh_extractor_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_extractor_extract(
            [In] IntPtr cityModelPtr,
            MeshExtractOptions options,
            [In] IntPtr outModelPtr);


        // ***************
        //  mesh_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_vertices_count(
            [In] IntPtr handle,
            out int outVerticesCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_vertex_at_index(
            [In] IntPtr handle,
            out PlateauVector3d outVertPos,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_indices_count(
            [In] IntPtr handle,
            out int outIndicesCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_indice_at_index(
            [In] IntPtr handle,
            out int vertexId,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_sub_mesh_count(
            [In] IntPtr plateauMeshPtr,
            out int subMeshCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_sub_mesh_at_index(
            [In] IntPtr plateauMeshPtr,
            out IntPtr plateauSubMeshPtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_uv1(
            [In] IntPtr plateauMeshPtr,
            PlateauVector2f[] outUvPosArray);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_uv2(
            [In] IntPtr plateauMeshPtr,
            PlateauVector2f[] outUvPosArray);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_uv3(
            [In] IntPtr plateauMeshPtr,
            PlateauVector2f[] outUvPosArray);

        // ***************
        //  sub_mesh_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_sub_mesh_get_start_index(
            [In] IntPtr subMeshPtr,
            out int startIndex);

        [DllImport(DllName)]
        internal static extern APIResult plateau_sub_mesh_get_end_index(
            [In] IntPtr subMeshPtr,
            out int endIndex);

        [DllImport(DllName)]
        internal static extern APIResult plateau_sub_mesh_get_texture_path(
            [In] IntPtr subMeshPtr,
            out IntPtr strPtr,
            out int strLength);

        // ***************
        //  model_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_create_model(
            out IntPtr outModelPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_model(
            [In] IntPtr modelPtr);


        [DllImport(DllName)]
        internal static extern APIResult plateau_model_get_root_nodes_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_model_get_root_node_at_index(
            [In] IntPtr handle,
            out IntPtr outNode,
            int index);

        // ***************
        //  node_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_node_get_name(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_node_get_child_count(
            [In] IntPtr nodeHandle,
            out int outChildCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_node_get_child_at_index(
            [In] IntPtr nodeHandle,
            out IntPtr childNodePtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_node_get_mesh(
            [In] IntPtr nodeHandle,
            out IntPtr outMeshPtr);

        // ***************
        //  geometry_utils_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_utils_get_center_point(
            [In] IntPtr cityModelPtr,
            out PlateauVector3d outCenterPoint);

        // ***************
        //  geo_reference_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_geo_reference(
            out IntPtr outGeoReferencePtr,
            PlateauVector3d referencePoint,
            float unitScale,
            CoordinateSystem coordinateSystem,
            int zoneId);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_geo_reference(
            [In] IntPtr geoReferencePtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_project(
            [In] IntPtr geoReferencePtr,
            out PlateauVector3d outXyz,
            GeoCoordinate latLon);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_unproject(
            [In] IntPtr geoReferencePtr,
            out GeoCoordinate outLatlon,
            PlateauVector3d point);

        // ***************
        //  mesh_code_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern MeshCode plateau_mesh_code_parse(
            [In] string code);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_code_get_extent(
            [In] MeshCode meshCode,
            [In, Out] ref Extent outExtent);

        // ***************
        //  udx_file_collection_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_udx_file_collection(
            out IntPtr handle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_udx_file_collection(
            [In] IntPtr handle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_find(
            [In] string source, [In, Out] IntPtr handle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_filter(
            [In] IntPtr handle, [In] Extent extent, [In, Out] IntPtr out_handle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_filter_by_mesh_codes(
            [In] IntPtr handle,
            [In] MeshCode[] meshCodes,
            int meshCodeCount,
            IntPtr collectionPtrForResult
        );

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_udx_file_collection_fetch(
            [In] IntPtr handle,
            [In] string destinationRootPath,
            [In] IntPtr gmlFileInfoPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_get_packages(
            [In] IntPtr handle, out PredefinedCityModelPackage packages);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_get_mesh_code_count(
            [In] IntPtr handle, [In, Out] ref int count);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_get_mesh_codes(
            [In] IntPtr handle, [In, Out] MeshCode[] meshCodes, [In] int count);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_get_gml_file_count(
            [In] IntPtr handle, out int count, [In] PredefinedCityModelPackage package);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_udx_file_collection_get_gml_file(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength,
            [In] PredefinedCityModelPackage package,
            [In] int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_file_collection_center_point(
            [In] IntPtr handle,
            out PlateauVector3d outCenterPoint,
            [In] IntPtr geoReferencePtr);


        // ***************
        //  gml_file_info_c.cpp
        // ***************
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_gml_file_info(
            out IntPtr outGmlFileInfoPtr,
            [In] string path);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_gml_file_info(
            [In] IntPtr handle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_info_get_path(
            [In] IntPtr handle,
            out IntPtr strPtr,
            out int strLength);
            
        // ***************
        //  city_model_package_info_c.cpp
        // ***************
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_city_model_package_info(
            out IntPtr outPackageInfoPtr,
            [MarshalAs(UnmanagedType.U1)] bool hasAppearance, int minLOD, int maxLOD);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_city_model_package_info(
            [In] IntPtr packageInfoPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_package_info_get_has_appearance(
            [In] IntPtr packageInfoPtr,
            [MarshalAs(UnmanagedType.U1)] out bool outHasAppearance);

        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_package_info_get_min_lod(
            [In] IntPtr packageInfoPtr,
            out int outMinLOD);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_package_info_get_max_lod(
            [In] IntPtr packageInfoPtr,
            out int outMaxLOD);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_city_model_package_info_get_predefined(
            PredefinedCityModelPackage package,
            [MarshalAs(UnmanagedType.U1)] out bool outHasAppearance,
            out int outMinLOD, out int outMaxLOD);
    }
}
