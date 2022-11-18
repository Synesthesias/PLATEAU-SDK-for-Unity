using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
{
    public class GmlFile : PInvokeDisposable
    {
        public GmlFile(IntPtr handle) : base(handle)
        {
            
        }

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
        
        /// <summary>
        /// GMLファイルとその関連ファイルをコピーします。
        /// 関連ファイルを探すために、GMLファイルの中身に対して文字列検索（テクスチャパスなどの記載を探す）が行われるため、
        /// GMLファイルの容量が増えるほど処理時間が増えます。
        /// </summary>
        /// <param name="destinationRootPath">コピー先のルートフォルダのパスです。</param>
        /// <param name="gmlFileInfo">コピー元のGMLファイルの <see cref="GmlFile"/> です。</param>
        public GmlFile Fetch(string destinationRootPath)
        {
            ThrowIfDisposed();
            var result = Create("");
            var apiResult = NativeMethods.plateau_gml_file_fetch(
                Handle, destinationRootPath, result.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_gml_file(Handle);
        }
    }
}
