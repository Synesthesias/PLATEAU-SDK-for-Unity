using System;
using System.IO;
using PLATEAU.Interop;
using PLATEAU.Network;

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
        private const string APIServerUrl = "https://9tkm2n.deta.dev";
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
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_get_path);
            }
            set
            {
                ThrowIfDisposed();
                var result = NativeMethods.plateau_gml_file_set_path(
                    Handle, value);
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
                var apiResult = NativeMethods.plateau_udx_sub_folder_get_package(FeatureType, out var package);
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
        /// GMLファイルとその関連ファイルをコピーします。
        /// 関連ファイルを探すために、GMLファイルの中身に対して文字列検索（テクスチャパスなどの記載を探す）が行われるため、
        /// GMLファイルの容量が増えるほど処理時間が増えます。
        /// </summary>
        /// <param name="destinationRootPath">コピー先のルートフォルダのパスです。</param>
        public GmlFile Fetch(string destinationRootPath)
        {
            ThrowIfDisposed();
            bool isServer = Path.StartsWith("http");
            switch (isServer)
            {
                case false:
                    return FetchLocal(destinationRootPath);
                case true:
                    return FetchServer(destinationRootPath);
                default:
                    throw new ArgumentOutOfRangeException();    
            }
        }

        private GmlFile FetchLocal(string destinationRootPath)
        {
            var resultGml = Create("");
            var apiResult = NativeMethods.plateau_gml_file_fetch_local(
                Handle, destinationRootPath, resultGml.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            resultGml.Path = resultGml.Path.Replace('\\', '/');
            return resultGml;
        }

        /// <summary>
        /// サーバーからGMLファイルをダウンロードします。
        /// </summary>
        /// <param name="destinationRootPath">
        /// ダウンロード先の基準パスです。
        /// 実際のダウンロード先は、このパスに "/udx/(種別ディレクトリ)/(0個以上のディレクトリ)/(gmlファイル名)" を追加したものになります。
        /// この追加分のパスは、接続先URLに含まれるものとします。 
        /// </param>
        /// <returns>ダウンロード後のGMLファイルの情報を返します。</returns>
        private GmlFile FetchServer(string destinationRootPath)
        {
            // "./udx/" で始まる相対パスです。
            int udxIdx = Path.LastIndexOf("/udx/", StringComparison.Ordinal);
            if (udxIdx < 0) throw new InvalidDataException($"Path should contain '/udx/' but it does not. Path = {Path}");
            string relativePath = "." + Path.Substring(Path.LastIndexOf("/udx/", StringComparison.Ordinal));
            
            string destPath = System.IO.Path.Combine(destinationRootPath, relativePath).Replace('\\', '/');
            string destDirPath = new DirectoryInfo(destPath).Parent?.FullName.Replace('\\', '/');
            if (destDirPath == null) throw new InvalidDataException("Invalid path.");
            Directory.CreateDirectory(destDirPath);
            
            using (var client = Client.Create())
            {
                client.Url = APIServerUrl;
                string downloadedPath = client.Download(destDirPath, Path);
                return Create(downloadedPath);
            }
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
