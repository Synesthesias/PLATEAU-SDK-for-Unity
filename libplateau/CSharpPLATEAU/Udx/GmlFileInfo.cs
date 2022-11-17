using System;
using PLATEAU.Interop;

namespace PLATEAU.Udx
{
    public class GmlFileInfo : PInvokeDisposable
    {
        public GmlFileInfo(IntPtr handle) : base(handle)
        {
            
        }

        public static GmlFileInfo Create(string path)
        {
            var apiResult = NativeMethods.plateau_create_gml_file_info(out IntPtr outPtr, path);
            DLLUtil.CheckDllError(apiResult);
            return new GmlFileInfo(outPtr);
        }

        public string Path
        {
            get => DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_info_get_path);
            set
            {
                var result = NativeMethods.plateau_gml_file_info_set_path(
                    Handle, value);
                DLLUtil.CheckDllError(result);
            }
        }

        public string FeatureType =>
            DLLUtil.GetNativeString(Handle, NativeMethods.plateau_gml_file_info_get_feature_type_str);
        
        public PredefinedCityModelPackage Package {
            get
            {
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
        /// <param name="gmlFileInfo">コピー元のGMLファイルの <see cref="GmlFileInfo"/> です。</param>
        public GmlFileInfo Fetch(string destinationRootPath)
        {
            var result = Create("");
            var apiResult = NativeMethods.plateau_gml_file_info_fetch(
                Handle, destinationRootPath, result.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_gml_file_info(Handle);
        }
    }
}
