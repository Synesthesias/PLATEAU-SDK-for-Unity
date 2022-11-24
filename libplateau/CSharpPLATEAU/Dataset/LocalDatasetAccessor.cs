using PLATEAU.Interop;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using PLATEAU.Geometries;

namespace PLATEAU.Dataset
{
    /// <summary>
    /// PLATEAUのデータファイルから、GMLファイル群を検索し、結果を保持します。
    /// 条件によりGMLファイルを絞り込む機能と、GMLと関連ファイルをコピーする機能があります。
    /// </summary>
    [Obsolete("C++側の基底クラスDatasetAccessor に移行する")]
    public class LocalDatasetAccessor// : IDisposable
    {
        public IntPtr Handle { get; private set; }
        private int disposed;

        private MeshCode[] meshCodes;

        public ReadOnlyCollection<MeshCode> MeshCodes
        {
            get
            {
                if (this.meshCodes != null)
                    return Array.AsReadOnly(this.meshCodes);

                var result = NativeMethods.plateau_local_dataset_accessor_get_mesh_code_count(Handle, out int count);
                DLLUtil.CheckDllError(result);
                this.meshCodes = new MeshCode[count];
                result = NativeMethods.plateau_local_dataset_accessor_get_mesh_codes(Handle, this.meshCodes, count);
                DLLUtil.CheckDllError(result);
                return Array.AsReadOnly(this.meshCodes);
            }
        }

        public PredefinedCityModelPackage Packages
        {
            get
            {
                var result = NativeMethods.plateau_local_dataset_accessor_get_packages(Handle, out var value);
                DLLUtil.CheckDllError(result);
                return value;
            }
        }

        public LocalDatasetAccessor()
        {
            APIResult result = NativeMethods.plateau_create_local_dataset_accessor(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            Handle = outPtr;
        }

        ~LocalDatasetAccessor()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_delete_local_dataset_accessor(Handle);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// GMLファイルを検索し、結果を保持します。
        /// </summary>
        /// <param name="source">検索元のルートフォルダのパスです。</param>
        public static LocalDatasetAccessor Find(string source)
        {
            var result = new LocalDatasetAccessor();
            var apiResult = NativeMethods.plateau_local_dataset_accessor_find(source, result.Handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルを街の緯度・経度・高さによって絞り込みます。
        /// </summary>
        public LocalDatasetAccessor Filter(Extent extent)
        {
            var result = new LocalDatasetAccessor();
            var apiResult = NativeMethods.plateau_local_dataset_accessor_filter(Handle, extent, result.Handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルを地域ID(メッシュコード)によって絞り込みます。
        /// </summary>
        public LocalDatasetAccessor FilterByMeshCodes(MeshCode[] meshCodeArray)
        {
            var result = new LocalDatasetAccessor();
            var apiResult = NativeMethods.plateau_local_dataset_accessor_filter_by_mesh_codes(
                Handle, meshCodeArray, meshCodeArray.Length, result.Handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        /// <summary>
        /// GMLファイルのうち、引数で与えられたパッケージ種に該当するもののパスを string の配列で返します。
        /// </summary>
        public string[] GetGmlFiles(PredefinedCityModelPackage package)
        {
            NativeMethods.plateau_local_dataset_accessor_get_gml_file_count(Handle, out int count, package);
            var gmlFiles = new string[count];
            for (int i = 0; i < count; i++)
            {
                var result = NativeMethods.plateau_local_dataset_accessor_get_gml_file(Handle, out IntPtr strPtr, out int strLength, package, i);
                DLLUtil.CheckDllError(result);
                gmlFiles[i] = DLLUtil.ReadUtf8Str(strPtr, strLength - 1);
            }

            return gmlFiles;
        }

        public PlateauVector3d CalcCenterPoint(GeoReference geoReference)
        {
            var apiResult = NativeMethods.plateau_local_dataset_accessor_center_point(
                Handle, out var centerPoint, geoReference.Handle);
            DLLUtil.CheckDllError(apiResult);
            return centerPoint;
        }
    }
}
