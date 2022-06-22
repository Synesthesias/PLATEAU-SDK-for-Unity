using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using PLATEAU.CityGML.Util;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// The <see cref="ObjWriter"/> class exports .obj file from .gml.
    /// </summary>
    public class ObjWriter : IDisposable
    {
        private readonly IntPtr handle;
        private int disposed;

        /// <summary>
        /// 書き出されるメッシュの平面直角座標系での基準点を取得または設定します。
        /// </summary>
        public PlateauVector3d ReferencePoint
        {
            get
            {
                APIResult result = NativeMethods.plateau_obj_writer_get_reference_point(this.handle, out PlateauVector3d vector3);
                DLLUtil.CheckDllError(result);
                return vector3;
            }
            set
            {
                APIResult result = NativeMethods.plateau_obj_writer_set_reference_point(this.handle, value);
                DLLUtil.CheckDllError(result);
            }
        }

        /// <summary>
        /// <see cref="ObjWriter"/>クラスのインスタンスを初期化します。
        /// </summary>
        public ObjWriter(DllLogLevel logLevel = DllLogLevel.Error)
        {
            APIResult result = NativeMethods.plateau_create_obj_writer(out IntPtr outPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outPtr;
            var logger = GetDllLogger();
            logger.SetCallbacksToStdOut();
            logger.SetLogLevel(logLevel);
        }

        ~ObjWriter()
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
                NativeMethods.plateau_delete_obj_writer(this.handle);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// .gmlを.objに変換します。
        /// </summary>
        /// <param name="objPath"></param>
        /// <param name="cityModel"></param>
        /// <param name="gmlPath"></param>
        public void Write(string objPath, CityModel cityModel, string gmlPath)
        {
            APIResult result = NativeMethods.plateau_obj_writer_write(this.handle, objPath, cityModel.Handle, gmlPath);
            DLLUtil.CheckDllError(result);
        }
        
        /// <summary>
        /// 変換時のDLLのログ出力の細かさを変更するには、
        /// 変換前にこのメソッドで <see cref="DllLogger"/> を取得して設定を変更します。
        /// </summary>
        public DllLogger GetDllLogger()
        {
            APIResult result = NativeMethods.plateau_obj_writer_get_dll_logger(
                Handle,
                out IntPtr loggerHandle
            );
            DLLUtil.CheckDllError(result);
            return new DllLogger(loggerHandle);
        }

        /// <summary>
        /// MeshGranularity（objの結合単位）を設定します。
        /// </summary>
        public void SetMeshGranularity(MeshGranularity value)
        {
            APIResult result = NativeMethods.plateau_obj_writer_set_mesh_granularity(this.handle, value);
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 現在のMeshGranularity（objの結合単位）を返します。
        /// </summary>
        public MeshGranularity GetMeshGranularity()
        {
            MeshGranularity meshGranularity = DLLUtil.GetNativeValue<MeshGranularity>(this.handle,
                NativeMethods.plateau_obj_writer_get_mesh_granularity);
            return meshGranularity;
        }

        /// <summary>
        /// 座標軸を設定します。
        /// </summary>
        public void SetDestAxes(AxesConversion value)
        {
            APIResult result = NativeMethods.plateau_obj_writer_set_dest_axes(this.handle, value);
            DLLUtil.CheckDllError(result);
        }

        /// <summary>
        /// 現在の座標軸設定を返します。
        /// </summary>
        public AxesConversion GetDestAxes()
        {
            AxesConversion axesConversion = DLLUtil.GetNativeValue<AxesConversion>(this.handle,
                NativeMethods.plateau_obj_writer_get_dest_axes);
            return axesConversion;
        }
        
        /// <summary>
        /// <see cref="ReferencePoint"/>を<paramref name="cityModel"/>の Envelope の中心に設定します。
        /// </summary>
        /// <param name="cityModel"></param>
        public void SetValidReferencePoint(CityModel cityModel)
        {
            APIResult result = NativeMethods.plateau_obj_writer_set_valid_reference_point(this.handle, cityModel.Handle);
            DLLUtil.CheckDllError(result);
        }
    }
}