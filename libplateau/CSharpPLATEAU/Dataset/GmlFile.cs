using System;
using PLATEAU.Interop;

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
            var apiResult = NativeMethods.plateau_create_gml_file(out IntPtr outPtr, path);
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

        public MeshCode MeshCode
        {
            get
            {
                ThrowIfDisposed();
                var meshCode = DLLUtil.GetNativeValue<MeshCode>(Handle,
                    NativeMethods.plateau_gml_file_get_mesh_code);
                return meshCode;
            }
        }

        public NativeVectorString SearchAllCodelistPathsInGml()
        {
            var paths = NativeVectorString.Create();
            var result = NativeMethods.plateau_gml_file_search_all_codelist_paths_in_gml(
                Handle, paths.Handle);
            DLLUtil.CheckDllError(result);
            return paths;
        }
        
        public NativeVectorString SearchAllImagePathsInGml()
        {
            var paths = NativeVectorString.Create();
            var result = NativeMethods.plateau_gml_file_search_all_image_paths_in_gml(
                Handle, paths.Handle);
            DLLUtil.CheckDllError(result);
            return paths;
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
    }
}
