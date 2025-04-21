using System;
using System.IO;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// GMLファイルに関する情報を保持します。
    /// 
    /// 寿命管理について:
    /// <see cref="NativeVectorGmlFile"/> の Dispose時、中身の <see cref="GmlFile"/> も自動的に廃棄されます。
    /// その自動廃棄が呼ばれないケースでのみ、手動で <see cref="GmlFile.Dispose"/> を呼んでください。
    /// </summary>
    public class GmlFile
    {
        public IntPtr Handle { get; private set; }
        private bool isDisposed;
        public GmlFile(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// GMLファイルに関する情報を保持するためのインスタンスを生成します。
        /// 実際にGMLファイルを作るわけではありません。
        /// </summary>
        public static GmlFile Create(string path)
        {
            var pathUtf8 = DLLUtil.StrToUtf8Bytes(path);
            var apiResult = NativeMethods.plateau_create_gml_file(out IntPtr outPtr, pathUtf8);
            DLLUtil.CheckDllError(apiResult);
            return new GmlFile(outPtr);
        }

        public string Path
        {
            get
            {
                ThrowIfDisposed();
                var pathNativeStr = NativeString.Create();
                var result = NativeMethods.plateau_gml_file_get_path(Handle, pathNativeStr.Handle);
                DLLUtil.CheckDllError(result);
                string path = pathNativeStr.ToString();
                pathNativeStr.Dispose();
                return path;
            }
            set
            {
                ThrowIfDisposed();
                var pathUtf8 = DLLUtil.StrToUtf8Bytes(value);
                var result = NativeMethods.plateau_gml_file_set_path(
                    Handle, pathUtf8);
                DLLUtil.CheckDllError(result);
            }
        }

        public string FeatureType
        {
            get
            {
                ThrowIfDisposed();
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_get_feature_type_str);
            }
        }


        public PredefinedCityModelPackage Package {
            get
            {
                ThrowIfDisposed();
                var featureTypeUtf8 = DLLUtil.StrToUtf8Bytes(FeatureType);
                var apiResult = NativeMethods.plateau_udx_sub_folder_feature_type_to_package(featureTypeUtf8, out var package);
                DLLUtil.CheckDllError(apiResult);
                return package;
            }
        }

        /// <summary>
        /// GMLファイルのGridCodeを返します。
        /// ただし、誤った形式のGMLファイル名である等の理由でGridCodeを読み取れなかった場合は
        /// 戻り値の isValid が false になります。
        /// 戻り値が解放されるようにするためにusingを付けてください。
        /// </summary>
        public GridCode GridCode
        {
            get
            {
                ThrowIfDisposed();
                var gridCodePtr = DLLUtil.GetNativeValue<IntPtr>(Handle,
                    NativeMethods.plateau_gml_file_get_grid_code);
                // gridCodePtrの寿命管理はC++側に任せるのでここでは解放しませんが、copiedはC#から解放する必要があります。
                var copied =  GridCode.CopyFrom(gridCodePtr);
                return copied;
            }
        }

        /// <summary>
        /// GMLファイルのEPSGコードを返します。
        /// 取得失敗時のデフォルト値はEPSG:6697です。
        /// </summary>
        public int Epsg
        {
            get
            {
                ThrowIfDisposed();
                var epsg = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_gml_file_get_epsg);
                return epsg;
            }
        }

        /// <summary>
        /// 平面直角座標系への変換が必要なGMLファイルかどうかを返します。
        /// 取得失敗時のデフォルト値はtrueです。
        /// </summary>
        public bool isPolarCoordinateSystem
        {
            get
            {
                ThrowIfDisposed();
                var result = NativeMethods.plateau_geometry_utils_is_polar_coordinate_system(Epsg, out var isPolar);
                DLLUtil.CheckDllError(result);
                return isPolar;
            }
        }


        public string[] SearchAllCodelistPathsInGml()
        {
            var nativePaths = NativeVectorString.Create();
            var result = NativeMethods.plateau_gml_file_search_all_codelist_paths_in_gml(
                Handle, nativePaths.Handle);
            DLLUtil.CheckDllError(result);
            var ret = nativePaths.ToCSharpArray();
            nativePaths.Dispose();
            return ret;
        }
        
        public string[] SearchAllImagePathsInGml()
        {
            var nativePaths = NativeVectorString.Create();
            var result = NativeMethods.plateau_gml_file_search_all_image_paths_in_gml(
                Handle, nativePaths.Handle);
            DLLUtil.CheckDllError(result);
            var ret = nativePaths.ToCSharpArray();
            nativePaths.Dispose();
            return ret;
        }
        
        /// <summary>
        /// GmlFileのパスがローカルPCを指す場合:
        /// GMLファイルとその関連ファイルをコピーします。
        /// 関連ファイルを探すために、GMLファイルの中身に対して文字列検索（テクスチャパスなどの記載を探す）が行われるため、
        /// GMLファイルの容量が増えるほど処理時間が増えます。
        /// GmlFileのパスが http で始まる場合:
        /// GMLファイルとその関連ファイルをダウンロードします。
        /// </summary>
        /// <param name="destinationRootPath">コピー先のルートフォルダのパスです。</param>
        public GmlFile Fetch(string destinationRootPath)
        {
            ThrowIfDisposed();
            var resultGml = Create("");
            var destinationRootPathUtf8 = DLLUtil.StrToUtf8Bytes(destinationRootPath);
            var apiResult = NativeMethods.plateau_gml_file_fetch(
                Handle, destinationRootPathUtf8, resultGml.Handle
            );
            if (apiResult == APIResult.ErrorValueIsInvalid)
            {
                throw new InvalidDataException("GmlFile is invalid.");
            }
            DLLUtil.CheckDllError(apiResult);
            resultGml.Path = resultGml.Path.Replace('\\', '/');
            return resultGml;
        }

        /// <summary>
        /// ローカルの場合、GMLファイルの全文を検索して対応LODの最大を求めます。
        /// サーバーの場合、APIサーバーに問い合わせて対応LODの最大を求めます。
        /// どちらにしても時間がかかる処理になります。
        /// </summary>
        public int GetMaxLod()
        {
            return DLLUtil.GetNativeValue<int>(Handle,
                NativeMethods.plateau_gml_file_get_max_lod);
        }

        /// <summary>
        /// 都市データのルートディレクトリ、すなわち "udx"フォルダの1つ上のパスを返します。
        /// </summary>
        public string CityRootPath()
        {
            string gmlPath = Path.Replace('\\', '/');
            int udxIdx = gmlPath.LastIndexOf("/udx/", StringComparison.Ordinal);
            if (udxIdx < 0)
            {
                throw new Exception("udx folder is not found in the path.");
            }
            return gmlPath.Substring(0, udxIdx);
        }

        public void Dispose()
        {
            this.isDisposed = true;
            DLLUtil.ExecNativeVoidFunc(Handle, NativeMethods.plateau_delete_gml_file);
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("GmlFile is disposed.");
            }
        }

        internal static class NativeMethods
        {
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_create_gml_file(
                out IntPtr outGmlFilePtr,
                [In] byte[] pathUtf8);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_gml_file(
                [In] IntPtr gmlFilePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_get_path(
                [In] IntPtr handle,
                [In, Out] IntPtr refPathNativeStringPtr);
            
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_gml_file_set_path(
                [In] IntPtr gmlFilePtr,
                [In] byte[] pathUtf8);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_gml_file_get_feature_type_str(
                [In] IntPtr gmlFilePtr,
                out IntPtr strPtr,
                out int strLength);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_get_grid_code(
                [In] IntPtr gmlFilePtr,
                out IntPtr outGridCodePtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_get_epsg(
                [In] IntPtr gmlFilePtr,
                out int outEpsg);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_geometry_utils_is_polar_coordinate_system(
                int epsg,
                [MarshalAs(UnmanagedType.U1)] out bool outBool);

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult plateau_gml_file_fetch(
                [In] IntPtr gmlFilePtr,
                [In] byte[] destinationRootPathUtf8,
                [In, Out] IntPtr outGmlFileInfoPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_search_all_codelist_paths_in_gml(
                [In] IntPtr gmlFilePtr,
                [In,Out] IntPtr refNativeVectorStringPtr);
        
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_search_all_image_paths_in_gml(
                [In] IntPtr gmlFilePtr,
                [In,Out] IntPtr refNativeVectorStringPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_gml_file_get_max_lod(
                [In] IntPtr gmlFilePtr,
                out int outMaxLod);
            
            // ***************
            //  udx_sub_folder_c.cpp
            // ***************
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_udx_sub_folder_feature_type_to_package(
                [In] byte[] featureTypeStrUtf8,
                out PredefinedCityModelPackage outPackage);
        }

    }
}
