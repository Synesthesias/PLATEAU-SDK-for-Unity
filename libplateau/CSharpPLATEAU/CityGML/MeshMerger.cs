using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// <see cref="CityModel"/> の範囲をグリッド状に分割して、各グリッド内のメッシュを結合する機能を提供します。
    /// Dispose すると結合結果となるポリゴンは廃棄されます。
    /// </summary>
    public class MeshMerger : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public MeshMerger()
        {
            var result = NativeMethods.plateau_mesh_merger_new(out IntPtr meshMergerPtr);
            DLLUtil.CheckDllError(result);
            this.handle = meshMergerPtr;
        }

        ~MeshMerger()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            var result = NativeMethods.plateau_mesh_merger_delete(this.handle);
            DLLUtil.CheckDllError(result);
            this.isDisposed = true;
        }
        
        public Mesh[] GridMerge(
            CityModel cityModel,
            CityObjectType targetTypeMask,
            int gridNumX,
            int gridNumY,
            DllLogger logger)
        {
            var result = NativeMethods.plateau_mesh_merger_grid_merge(
                this.handle,
                cityModel.Handle,
                targetTypeMask,
                gridNumX,
                gridNumY,
                out int numPolygons,
                logger.Handle
            );
            DLLUtil.CheckDllError(result);
            
            var polygonPointers = new IntPtr[numPolygons];
            var result2 = NativeMethods.plateau_mesh_merger_get_last_result_of_grid_merge(
                this.handle,
                polygonPointers
            );
            DLLUtil.CheckDllError(result2);
            
            var retPolygons = new Mesh[numPolygons];
            for (int i = 0; i < numPolygons; i++)
            {
                retPolygons[i] = new Mesh(polygonPointers[i]);
            }

            return retPolygons;
        }
    }
}