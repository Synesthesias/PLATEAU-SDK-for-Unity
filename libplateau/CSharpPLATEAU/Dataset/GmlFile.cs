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
                // ThrowIfDisposed();
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_get_path);
            }
            set
            {
                // ThrowIfDisposed();
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
            var result = Create("");
            var apiResult = NativeMethods.plateau_gml_file_fetch_local(
                Handle, destinationRootPath, result.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        private GmlFile FetchServer(string destinationRootPath)
        {
            using (var client = Client.Create())
            {
                client.Url = APIServerUrl;
                byte[] destinationDirUtf8 = DLLUtil.StrToUtf8Bytes(destinationRootPath);
                byte[] urlUtf8 = DLLUtil.StrToUtf8Bytes(Path);
                var result = NativeMethods.plateau_client_download(client.Handle, destinationDirUtf8, urlUtf8);
                DLLUtil.CheckDllError(result);
                string downloadedPath = System.IO.Path.Combine(destinationRootPath, System.IO.Path.GetFileName(Path));
                if (!File.Exists(downloadedPath)) throw new FileLoadException("Failed to download file.");
                if (new FileInfo(downloadedPath).Length == 0) throw new FileLoadException("Downloaded file size is zero. Maybe client.Url is wrong.");
                return Create(downloadedPath);
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
            var result = NativeMethods.plateau_delete_gml_file(Handle);
            DLLUtil.CheckDllError(result);
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
