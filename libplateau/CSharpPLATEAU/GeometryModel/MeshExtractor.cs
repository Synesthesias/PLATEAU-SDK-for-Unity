using System;
using System.Threading;
using PLATEAU.CityGML;
using PLATEAU.Interop;

namespace PLATEAU.GeometryModel
{
    /// <summary>
    /// <see cref="CityModel"/> の範囲をグリッド状に分割して、各グリッド内のメッシュを結合する機能を提供します。
    /// Dispose すると結合結果となるポリゴンは廃棄されます。
    /// </summary>
    public class MeshExtractor : IDisposable
    {
        private readonly IntPtr handle;
        private int disposed;

        public MeshExtractor()
        {
            var result = NativeMethods.plateau_mesh_extractor_new(out IntPtr meshMergerPtr);
            DLLUtil.CheckDllError(result);
            this.handle = meshMergerPtr;
        }

        ~MeshExtractor()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 0)
            {
                NativeMethods.plateau_mesh_extractor_delete(this.handle);
            }
            GC.SuppressFinalize(this);
        }

        public Model Extract(CityModel cityModel, MeshExtractOptions options)
        {
            var result = NativeMethods.plateau_mesh_extractor_extract(
                this.handle, cityModel.Handle, options,
                out IntPtr outModelPtr
            );
            DLLUtil.CheckDllError(result);
            return new Model(outModelPtr);
        }
    }
}