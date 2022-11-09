using PLATEAU.Interop;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using PLATEAU.Geometries;

namespace PLATEAU.Udx
{
    /// <summary>
    /// PLATEAUのデータファイルから、GMLファイル群を検索し、結果を保持します。
    /// 条件によりGMLファイルを絞り込む機能と、GMLと関連ファイルをコピーする機能があります。
    /// </summary>
    public class UdxFileCollection : IDisposable
    {
        private readonly IntPtr handle;
        private int disposed;

        private MeshCode[] meshCodes;

        public ReadOnlyCollection<MeshCode> MeshCodes
        {
            get
            {
                if (this.meshCodes != null)
                    return Array.AsReadOnly(this.meshCodes);

                int count = 0;
                var result = NativeMethods.plateau_udx_file_collection_get_mesh_code_count(this.handle, ref count);
                DLLUtil.CheckDllError(result);
                this.meshCodes = new MeshCode[count];
                result = NativeMethods.plateau_udx_file_collection_get_mesh_codes(this.handle, this.meshCodes, count);
                DLLUtil.CheckDllError(result);
                return Array.AsReadOnly(this.meshCodes);
            }
        }

        public PredefinedCityModelPackage Packages
        {
            get
            {
                var result = NativeMethods.plateau_udx_file_collection_get_packages(this.handle, out var value);
                DLLUtil.CheckDllError(result);
                return value;
            }
        }

        public UdxFileCollection()
        {
            APIResult result = NativeMethods.plateau_create_udx_file_collection(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
        }

        ~UdxFileCollection()
        {
            Dispose();
        }

        /// <summary>
        /// セーフハンドルを取得します。
        /// </summary>
        public IntPtr Handle => this.handle;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_delete_udx_file_collection(this.handle);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// GMLファイルを検索し、結果を保持します。
        /// </summary>
        /// <param name="source">検索元のルートフォルダのパスです。</param>
        public static UdxFileCollection Find(string source)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_find(source, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルを街の緯度・経度・高さによって絞り込みます。
        /// </summary>
        public UdxFileCollection Filter(Extent extent)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_filter(this.handle, extent, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルを地域ID(メッシュコード)によって絞り込みます。
        /// </summary>
        public UdxFileCollection FilterByMeshCodes(MeshCode[] meshCodeArray)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_filter_by_mesh_codes(
                this.handle, meshCodeArray, meshCodeArray.Length, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルとその関連ファイルをコピーします。
        /// 関連ファイルを探すために、GMLファイルの中身に対して文字列検索（テクスチャパスなどの記載を探す）が行われるため、
        /// GMLファイルの容量が増えるほど処理時間が増えます。
        /// </summary>
        /// <param name="destinationRootPath">コピー先のルートフォルダのパスです。</param>
        /// <param name="gmlFileInfo">コピー元のGMLファイルの <see cref="GmlFileInfo"/> です。</param>
        public static GmlFileInfo Fetch(string destinationRootPath, GmlFileInfo gmlFileInfo)
        {
            var result = GmlFileInfo.Create("");
            var apiResult = NativeMethods.plateau_udx_file_collection_fetch(
                destinationRootPath, gmlFileInfo.Handle, result.Handle
            );
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルのうち、引数で与えられたパッケージ種に該当するもののパスを string の配列で返します。
        /// </summary>
        public string[] GetGmlFiles(PredefinedCityModelPackage package)
        {
            NativeMethods.plateau_udx_file_collection_get_gml_file_count(this.handle, out int count, package);
            var gmlFiles = new string[count];
            for (int i = 0; i < count; i++)
            {
                var result = NativeMethods.plateau_udx_file_collection_get_gml_file(this.handle, out IntPtr strPtr, out int strLength, package, i);
                DLLUtil.CheckDllError(result);
                gmlFiles[i] = DLLUtil.ReadUtf8Str(strPtr, strLength - 1);
            }

            return gmlFiles;
        }

        public PlateauVector3d CalcCenterPoint(GeoReference geoReference)
        {
            var apiResult = NativeMethods.plateau_udx_file_collection_center_point(
                this.handle, out var centerPoint, geoReference.Handle);
            DLLUtil.CheckDllError(apiResult);
            return centerPoint;
        }
    }
}
