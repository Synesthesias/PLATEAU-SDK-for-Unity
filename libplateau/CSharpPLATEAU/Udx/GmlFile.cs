using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
{
    public class GmlFile// : PInvokeDisposable
    {
        public IntPtr Handle { get; private set; }
        public GmlFile(IntPtr handle)// : base(handle)
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
                // ThrowIfDisposed();
                return DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_get_feature_type_str);
            }
        }


        public PredefinedCityModelPackage Package {
            get
            {
                // ThrowIfDisposed();
                var apiResult = NativeMethods.plateau_udx_sub_folder_get_package(FeatureType, out var package);
                DLLUtil.CheckDllError(apiResult);
                return package;
            }
        }

        public MeshCode MeshCode
        {
            get
            {
                // ThrowIfDisposed();
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
            // ThrowIfDisposed();
            var result = Create("");
            var apiResult = NativeMethods.plateau_gml_file_fetch(
                Handle, destinationRootPath, result.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        // protected override void DisposeNative()
        // {
            // NativeMethods.plateau_delete_gml_file(Handle);
        // }

        public void Dispose()
        {
            var result = NativeMethods.plateau_delete_gml_file(Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}
