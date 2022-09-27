using PLATEAU.Interop;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace PLATEAU.Udx
{
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

        public static UdxFileCollection Find(string source)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_find(source, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        public UdxFileCollection Filter(Extent extent)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_filter(this.handle, extent, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        public UdxFileCollection FilterByMeshCodes(MeshCode[] meshCodeArray)
        {
            var result = new UdxFileCollection();
            var apiResult = NativeMethods.plateau_udx_file_collection_filter_by_mesh_codes(
                this.handle, meshCodeArray, meshCodeArray.Length, result.handle);
            DLLUtil.CheckDllError(apiResult);
            return result;
        }

        public void Fetch(string destinationRootPath, GmlFileInfo gmlFileInfo)
        {
            var apiResult = NativeMethods.plateau_udx_file_collection_fetch(
                this.handle, destinationRootPath, gmlFileInfo.Handle 
            );
            DLLUtil.CheckDllError(apiResult);
        }

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
    }
}
