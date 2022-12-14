using System;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.Geometries;
using PLATEAU.PolygonMesh;
using PLATEAU.Dataset;

// 文字列のサイズをDLLでやりとりする時の型を決めます。
using DllStrSizeT = System.Int32;

namespace PLATEAU.Interop
{
    /// <summary>
    /// メッシュの結合単位
    /// </summary>
    public enum MeshGranularity
    {
        /// <summary>
        /// 最小地物単位(LOD2, LOD3の各部品)
        /// </summary>
        PerAtomicFeatureObject,
        /// <summary>
        /// 主要地物単位(建築物、道路等)
        /// </summary>
        PerPrimaryFeatureObject,
        /// <summary>
        /// 都市モデル地域単位(GMLファイル内のすべてを結合)
        /// </summary>
        PerCityModelArea
    }

    public enum GltfFileFormat
    {
        GLB,
        GLTF
    }

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
        
        public static PlateauVector3d operator+(PlateauVector3d op1, PlateauVector3d op2)
        {
            return new PlateauVector3d(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z);
        }

        public static PlateauVector3d operator-(PlateauVector3d op1, PlateauVector3d op2)
        {
            return new PlateauVector3d(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }

        public static PlateauVector3d operator*(PlateauVector3d vec, double scalar)
        {
            return new PlateauVector3d(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);
        }

        public static PlateauVector3d operator /(PlateauVector3d vec, double scalar)
        {
            return new PlateauVector3d(vec.X / scalar, vec.Y / scalar, vec.Z / scalar);
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
    ///  GMLファイルのパース時の設定です。
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
    
    public enum MeshFileFormat{OBJ, GLTF, FBX}


    [StructLayout(LayoutKind.Sequential)]
    public struct MeshConvertOptionsData
    {
        public CoordinateSystem MeshAxes;
        public PlateauVector3d ReferencePoint;
        public MeshGranularity MeshGranularity;
        public uint MinLOD;
        public uint MaxLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportLowerLOD;
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        public float UnitScale;
        public MeshFileFormat MeshFileFormat;
        public int CoordinateZoneID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FbxWriteOptions
    {
        public FbxFileFormat FileFormat;

        public FbxWriteOptions(FbxFileFormat fileFormat)
        {
            this.FileFormat = fileFormat;
        }
    }

    public enum FbxFileFormat : UInt32
    {
        Binary, Ascii
    }

    /// <summary>
    /// 地理院地図のタイル座標です。
    /// タイル座標については国土地理院のWebサイトを参照してください。
    /// <see href="https://maps.gsi.go.jp/development/siyou.html#siyou-zm"/>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCoordinate
    {
        public int Column;
        public int Row;
        public int ZoomLevel;

        public TileCoordinate(int column, int row, int zoomLevel)
        {
            this.Column = column;
            this.Row = row;
            this.ZoomLevel = zoomLevel;
        }

        public override string ToString()
        {
            return $"TileCoordinate: (Column={this.Column}, Row={this.Row}, ZoomLevel={this.ZoomLevel})";
        }
    }

    /// <summary>
    /// GMLファイルから3Dメッシュを取り出すための設定です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshExtractOptions
    {
        /// <summary> 直交座標系における座標で、3Dモデルの原点をどこに設定するかです。 </summary>
        public PlateauVector3d ReferencePoint;
        /// <summary> 座標軸の向きです。 </summary>
        public CoordinateSystem MeshAxes;
        /// <summary> メッシュ結合の粒度です。 </summary>
        public MeshGranularity MeshGranularity;
        /// <summary> 出力するLODの範囲上限です。 </summary>
        public uint MaxLOD;
        /// <summary> 出力するLODの範囲の下限です。 </summary>
        public uint MinLOD;
        /// <summary> テクスチャを含めるかどうかです。 </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        /// <summary> メッシュ結合の粒度が「都市モデル単位」の時のみ有効で、この設定では都市を格子状のグリッドに分割するので、その1辺あたりの分割数(縦の数 = 横の数)です。</summary>
        public int GridCountOfSide;
        /// <summary>  大きさ補正です。  </summary>
        public float UnitScale;
        /// <summary>
        /// 国土交通省が規定する、日本の平面直角座標系の基準点の番号です。
        /// 詳しくは次の国土地理院のサイトをご覧ください。
        /// <see href="https://www.gsi.go.jp/sokuchikijun/jpc.html"/>
        /// </summary>
        public int CoordinateZoneID;
        /// <summary>
        /// 範囲外の3Dモデルを出力から除外するための、2つの方法のうち1つを有効にするかどうかを bool で指定します。
        /// その方法とは、都市オブジェクトの最初の頂点の位置が範囲外のとき、そのオブジェクトはすべて範囲外とみなして出力から除外します。
        /// これはビル1棟程度の大きさのオブジェクトでは有効ですが、
        /// 10km×10kmの地形のような巨大なオブジェクトでは、実際には範囲内なのに最初の頂点が遠いために除外されるということがおきます。
        /// したがって、この値は建物では true, 地形では false となるべきです。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExcludeCityObjectOutsideExtent;
        /// <summary>
        /// 範囲外の3Dモデルを出力から除外するための、2つの方法のうち1つを有効にするかどうかを bool で指定します。
        /// その方法とは、メッシュ操作によって、範囲外に存在するポリゴンを除外します。
        /// この方法であれば 10km×10km の地形など巨大なオブジェクトにも対応できます。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExcludeTrianglesOutsideExtent;
        /// <summary>  対象範囲を緯度・経度・高さで指定します。 </summary>
         public Extent Extent;
        
        /// <summary> デフォルト値の設定を返します。 </summary>
        public static MeshExtractOptions DefaultValue()
        {
            var apiResult = NativeMethods.plateau_mesh_extract_options_default_value(out var defaultOptions);
            DLLUtil.CheckDllError(apiResult);
            return defaultOptions;
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

        public override string ToString()
        {
            return $"GeoCoordinate: (Lat={this.Latitude}, Lon={this.Longitude}, Height={this.Height})";
        }

        public static GeoCoordinate Min(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                Math.Min(op1.Latitude, op2.Latitude),
                Math.Min(op1.Longitude, op2.Longitude),
                Math.Min(op1.Height, op2.Height)
            );
        }

        public static GeoCoordinate Max(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                Math.Max(op1.Latitude, op2.Latitude),
                Math.Max(op1.Longitude, op2.Longitude),
                Math.Max(op1.Height, op2.Height)
            );
        }
        
        public static GeoCoordinate operator +(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                op1.Latitude + op2.Latitude,
                op1.Longitude + op2.Longitude,
                op1.Height + op2.Height
            );
        }

        public static GeoCoordinate operator -(GeoCoordinate op1, GeoCoordinate op2)
        {
            return new GeoCoordinate(
                op1.Latitude - op2.Latitude,
                op1.Longitude - op2.Longitude,
                op1.Height - op2.Height
            );
        }

        public static GeoCoordinate operator *(GeoCoordinate geo, double scalar)
        {
            return new GeoCoordinate(
                geo.Latitude * scalar,
                geo.Longitude * scalar,
                geo.Height * scalar
            );
        }

        public static GeoCoordinate operator /(GeoCoordinate geo, double scalar)
        {
            if (Math.Abs(scalar) <= double.Epsilon) throw new DivideByZeroException();
            return geo * (1.0 / scalar);
        }

        /// <summary>
        /// 緯度、経度の値を2次元ベクトルとして見たときのベクトルの長さの2乗です。
        /// 高さは無視されます。
        /// </summary>
        public double SqrMagnitudeLatLon => this.Latitude * this.Latitude + this.Longitude * this.Longitude;
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

        public GeoCoordinate Center => new GeoCoordinate(
            (this.Min.Latitude + this.Max.Latitude) * 0.5,
            (this.Min.Longitude + this.Max.Longitude) * 0.5,
            (this.Min.Height + this.Max.Height) * 0.5);

         /// <summary>
         /// 共通部分を返します。
         /// なければ (-99, -99, -99), (-99, -99, -99)を返します。
         /// </summary>
         public static Extent Intersection(Extent op1, Extent op2)
         {
             var max = GeoCoordinate.Max(op1.Max, op2.Max);
             var min = GeoCoordinate.Min(op1.Min, op2.Min);
             var intersectSize = op1.Size() + op2.Size() - (max - min);
             if (intersectSize.Latitude <= 0 || intersectSize.Latitude <= 0 || intersectSize.Height <= 0)
                 return new Extent(new GeoCoordinate(-99,-99,-99), new GeoCoordinate(-99, -99, -99));
             var minMax = GeoCoordinate.Min(op1.Max, op2.Max);
             var maxMin = GeoCoordinate.Max(op1.Min, op2.Min);
             return new Extent(maxMin, minMax);
         }

         public GeoCoordinate Size()
         {
             return this.Max - this.Min;
         }

         public static readonly Extent All =
             new Extent(
                 new GeoCoordinate(-90, -180, -9999),
                 new GeoCoordinate(90, 180, 9999));
         
        public override string ToString()
        {
            return $"Extent: (Min={this.Min}, Max={this.Max})";
        }
    }

    public enum APIResult
    {
        Success,
        ErrorUnknown,
        ErrorValueNotFound,
        ErrorLoadingCityGml,
        ErrorIndexOutOfBounds,
        ErrorFileSystem,
        ErrorInvalidArgument
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
        internal static extern APIResult plateau_delete_city_model(
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

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_mesh(
            out IntPtr newMeshPtr,
            string meshID);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_mesh(
            [In] IntPtr handle);

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
            [Out] PlateauVector2f[] outUvPosArray);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_uv2(
            [In] IntPtr plateauMeshPtr,
            [Out] PlateauVector2f[] outUvPosArray);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_get_uv3(
            [In] IntPtr plateauMeshPtr,
            [Out] PlateauVector2f[] outUvPosArray);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_mesh_add_sub_mesh(
            [In] IntPtr meshPtr,
            [In] string texturePath,
            int subMeshStartIndex,
            int subMeshEndIndex);

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

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_sub_mesh(
            out IntPtr outSubMeshPtr,
            int startIndex,
            int endIndex,
            string texturePath);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_sub_mesh(
            [In] IntPtr subMeshPtr);

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

        /// <summary>
        /// 注意:
        /// 利用後、元の <see cref="Node"/> は利用不可になります。
        /// </summary>
        [DllImport(DllName)]
        internal static extern APIResult plateau_model_add_node_by_std_move(
            [In] IntPtr modelPtr,
            [In] IntPtr nodePtr);

        // ***************
        //  node_c.cpp
        // ***************
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_node(
            out IntPtr outNodePtr,
            string id);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_node(
            [In] IntPtr nodePtr);
        
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
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_node_set_mesh_by_std_move(
            [In] IntPtr nodePtr,
            [In] IntPtr meshPtr);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_node_add_child_node_by_std_move(
            [In] IntPtr nodePtr,
            [In] IntPtr childNodePtr);

        // ***************
        //  geometry_utils_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_geometry_utils_get_center_point(
            [In] IntPtr cityModelPtr,
            out PlateauVector3d outCenterPoint,
            int coordinateZoneID);

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

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_get_reference_point(
            [In] IntPtr handle,
            out PlateauVector3d outReferencePoint);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_get_zone_id(
            [In] IntPtr handle,
            out int zoneID);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_get_unit_scale(
            [In] IntPtr handle,
            out float unitScale);

        [DllImport(DllName)]
        internal static extern APIResult plateau_geo_reference_get_coordinate_system(
            [In] IntPtr handle,
            out CoordinateSystem outCoordinateSystem);

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
        //  i_dataset_accessor_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_i_dataset_accessor(
            [In] IntPtr accessorPtr);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_i_dataset_accessor_get_gml_files(
            [In] IntPtr accessorPtr,
            PredefinedCityModelPackage package,
            [In] IntPtr refVectorGmlFilePtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_i_dataset_accessor_get_mesh_codes(
            [In] IntPtr accessorPtr,
            [In,Out] IntPtr refVectorMeshCodePtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_i_dataset_accessor_get_packages(
            [In] IntPtr accessorPtr,
            out PredefinedCityModelPackage outPackageFlags);

        [DllImport(DllName)]
        internal static extern APIResult plateau_i_dataset_accessor_calculate_center_point(
            [In] IntPtr accessorPtr,
            [In] IntPtr geoReferencePtr,
            out PlateauVector3d outCenterPoint);

        [DllImport(DllName)]
        internal static extern APIResult plateau_i_dataset_accessor_filter_by_mesh_codes(
            [In] IntPtr accessorPtr,
            [In] IntPtr nativeVectorMeshCodePtr,
            out IntPtr outFilteredAccessorPtr);

        // ***************
        //  udx_sub_folder_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_udx_sub_folder_feature_type_to_package(
            [In] byte[] featureTypeStrUtf8,
            out PredefinedCityModelPackage outPackage);
        
        // ***************
        //  dataset_source_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_create_dataset_source_local(
            out IntPtr outDatasetSource,
            [In] byte[] sourcePathUtf8);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_dataset_source(
            [In] IntPtr datasetSourcePtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_source_get_accessor(
            [In] IntPtr datasetSourcePtr,
            out IntPtr accessorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_create_dataset_source_server(
            out IntPtr newDatasetSourcePtr,
            [In] string datasetID,
            [In] IntPtr clientPtr);


        // ***************
        //  gml_file_c.cpp
        // ***************
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_gml_file(
            out IntPtr outGmlFilePtr,
            [In] string path);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_gml_file(
            [In] IntPtr gmlFilePtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_get_path(
            [In] IntPtr handle,
            [In, Out] IntPtr refPathNativeStringPtr);
            
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_gml_file_set_path(
            [In] IntPtr gmlFilePtr,
            [In] string path);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_gml_file_get_feature_type_str(
            [In] IntPtr gmlFilePtr,
            out IntPtr strPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_get_mesh_code(
            [In] IntPtr gmlFilePtr,
            out MeshCode outMeshCode);
        
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_gml_file_fetch(
            [In] IntPtr gmlFilePtr,
            [In] byte[] destinationRootPathUtf8,
            [In, Out] IntPtr outGmlFileInfoPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_search_all_codelist_paths_in_gml(
            [In] IntPtr gmlFilePtr,
            [In,Out] IntPtr refNativeVectorStringPtr);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_search_all_image_paths_in_gml(
            [In] IntPtr gmlFilePtr,
            [In,Out] IntPtr refNativeVectorStringPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_gml_file_get_max_lod(
            [In] IntPtr gmlFilePtr,
            out int outMaxLod);
            
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
        
        // ***************
        //  mesh_extract_options_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_extract_options_default_value(
            out MeshExtractOptions outDefaultOptions);
        
        // ***************
        //  gltf_writer_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_gltf_writer(out IntPtr outHandle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_gltf_writer([In] IntPtr gltfWriter);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_gltf_writer_write(
            [In] IntPtr handle,
            out bool flg,
            [In] byte[] gltfFilePathUtf8,
            [In] IntPtr modelPtr,
            [In] byte[] texPathUtf8,
            GltfFileFormat format);

        // ***************
        //  obj_writer_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_obj_writer(out IntPtr outHandle);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_obj_writer([In] IntPtr objWriter);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_obj_writer_write(
            [In] IntPtr handle,
            out bool flg,
            [In] byte[] objFilePathUtf8,
            [In] IntPtr modelPtr);
        
        // ***************
        //  fbx_writer_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_fbx_writer_write(
            [In] byte[] fbxFilePathUtf8,
            [In] IntPtr modelPtr,
            FbxWriteOptions options,
            out bool outIsSucceed);
        
        // ***************
        //  mesh_merger_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_merger_merge_mesh(
            [In] IntPtr meshPtr,
            [In] IntPtr otherMeshPtr,
            [MarshalAs(UnmanagedType.U1)] bool invertMeshFrontBack,
            [MarshalAs(UnmanagedType.U1)] bool includeTexture);

        [DllImport(DllName)]
        internal static extern APIResult plateau_mesh_merger_mesh_info(
            [In] IntPtr meshPtr,
            [In] PlateauVector3d[] vertices,
            int verticesCount,
            [In] uint[] indices,
            int indicesCount,
            [In] PlateauVector2f[] uv1,
            int uv1Count,
            [In] IntPtr[] subMeshPointers,
            int subMeshCount,
            CoordinateSystem meshAxisConvertFrom,
            CoordinateSystem meshAxisConvertTo,
            [MarshalAs(UnmanagedType.U1)] bool includeTexture);
        
        // ***************
        //  vector_tile_downloader_c.cpp
        // ***************
        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_create_vector_tile_downloader(
            out IntPtr handle,
            string destination,
            Extent extent,
            int zoomLevel);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_tile_downloader(
            [In] IntPtr handle);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_tile_downloader_get_tile_count(
            [In] IntPtr handle,
            out int tileCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_tile_downloader_download(
            [In] IntPtr handle,
            int index);

        // ***************
        //  vector_tile_downloader_c.cpp
        // ***************
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_tile_projection_project(
            [In] GeoCoordinate geoCoordinate,
            int zoomLevel,
            out TileCoordinate outTileCoordinate);

        [DllImport(DllName)]
        internal static extern APIResult plateau_tile_projection_unproject(
            [In] TileCoordinate tileCoordinate,
            out Extent outExtent);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_tile_downloader_calc_destination_path_size(
            [In] IntPtr handle,
            out int outStrSize,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_tile_downloader_calc_destination_path(
            [In] IntPtr handle,
            [In, Out] IntPtr strPtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_tile_downloader_get_tile(
            [In] IntPtr handle,
            out TileCoordinate outTileCoordinate,
            int index);
        
        // ***************
        //  available_lod_searcher_c.cpp
        // ***************

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_lod_searcher_search_lods_in_file(
            [In] byte[] filePathUtf8,
            out uint outLodFlags);
        
        // ***************
        //  vector_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_vector_gml_file(
            out IntPtr outVectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_gml_file(
            [In] IntPtr vectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_gml_file_get_pointer(
            [In] IntPtr vectorPtr,
            out IntPtr outGmlFilePtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_gml_file_count(
            [In] IntPtr handle,
            out int outCount);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_vector_mesh_code(
            out IntPtr outVectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_mesh_code(
            [In] IntPtr vectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_mesh_code_get_value(
            [In] IntPtr vectorPtr,
            out MeshCode outMeshCode,
            int index);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_mesh_code_count(
            [In] IntPtr handle,
            out int outCount);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_mesh_code_push_back_value(
            [In] IntPtr handle,
            [In] MeshCode meshCode);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_vector_dataset_metadata_group(
            out IntPtr outVectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_dataset_metadata_group(
            [In] IntPtr vectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_dataset_metadata_group_get_pointer(
            [In] IntPtr vectorPtr,
            out IntPtr outGmlFilePtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_dataset_metadata_group_count(
            [In] IntPtr handle,
            out int outCount);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_vector_dataset_metadata(
            out IntPtr outVectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_dataset_metadata(
            [In] IntPtr vectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_dataset_metadata_get_pointer(
            [In] IntPtr vectorPtr,
            out IntPtr outDatasetMetadataPtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_dataset_metadata_count(
            [In] IntPtr handle,
            out int outCount);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_vector_string(
            out IntPtr outVectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_vector_string(
            [In] IntPtr vectorPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_string_get_pointer(
            [In] IntPtr vectorPtr,
            out IntPtr outStringPtr,
            int index);

        [DllImport(DllName)]
        internal static extern APIResult plateau_vector_string_count(
            [In] IntPtr handle,
            out int outCount);
        
        // ***************
        //  string_c.cpp
        // ***************

        [DllImport(DllName)]
        internal static extern APIResult plateau_create_string(
            out IntPtr newStringPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_string(
            [In] IntPtr stringPtr);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_string_get_size(
            [In] IntPtr nativeStringPtr,
            out int outSize);

        [DllImport(DllName)]
        internal static extern APIResult plateau_string_get_char_ptr(
            [In] IntPtr nativeStringPtr,
            out IntPtr outCharPtr);
        
        // ***************
        //  dataset_metadata_group_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_dataset_metadata_group(
            out IntPtr outDatasetMetadataGroupPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_dataset_metadata_group(
            [In] IntPtr datasetMetadataGroupPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_group_get_id(
            [In] IntPtr handle,
            out IntPtr outStrPtr,
            out int strLength);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_group_get_title(
            [In] IntPtr handle,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_group_get_datasets(
            [In] IntPtr handle,
            out IntPtr nativeVectorDatasetMetadataPtr);
        
        // ***************
        //  dataset_metadata_c.cpp
        // ***************
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_dataset_metadata(
            out IntPtr outDatasetMetadataPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_dataset_metadata(
            [In] IntPtr datasetMetadataPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_get_id(
            [In] IntPtr datasetMetadataPtr,
            out IntPtr outStrPtr,
            out int strLength);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_get_title(
            [In] IntPtr datasetMetadataPtr,
            out IntPtr outStrPtr,
            out int strLength);
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_get_description(
            [In] IntPtr datasetMetadataPtr,
            out IntPtr outStrPtr,
            out int strLength);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_get_max_lod(
            [In] IntPtr datasetMetadataPtr,
            out int outMaxLod);

        [DllImport(DllName)]
        internal static extern APIResult plateau_dataset_metadata_get_feature_types(
            [In] IntPtr datasetMetadataPtr,
            [In,Out] IntPtr refNativeVectorStringPtr);

        // ***************
        //  client_c.cpp
        // ***************
        
        [DllImport(DllName)]
        internal static extern APIResult plateau_create_client(
            out IntPtr newClientPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_delete_client(
            [In] IntPtr ptr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_client_get_metadata(
            [In] IntPtr clientPtr,
            [In, Out] IntPtr refNativeArrayDatasetMetadataGroupPtr);

        [DllImport(DllName, CharSet = CharSet.Ansi)]
        internal static extern APIResult plateau_client_set_api_server_url(
            [In] IntPtr clientPtr,
            [In] string url);

        [DllImport(DllName)]
        internal static extern APIResult plateau_client_get_api_server_url_size(
            [In] IntPtr clientPtr,
            out int outUrlSize);

        [DllImport(DllName)]
        internal static extern APIResult plateau_client_get_api_server_url(
            [In] IntPtr clientPtr,
            [In,Out] IntPtr outStrPtr );

        [DllImport(DllName)]
        internal static extern APIResult plateau_client_download(
            [In] IntPtr clientPtr,
            [In] byte[] destinationDirectoryUtf8,
            [In] byte[] urlUtf8,
            [In,Out] IntPtr refNativeStringPtr);

        [DllImport(DllName)]
        internal static extern APIResult plateau_client_get_default_url(
            [In, Out] IntPtr nativeStrPtr);

    }
}
