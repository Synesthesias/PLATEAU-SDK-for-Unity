using System;
using System.Runtime.InteropServices;
using PLATEAU.CityGML;
using PLATEAU.Dataset;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MaterialAdjust
{
    public class MaterialAdjusterByType : PInvokeDisposable
    {
        public static MaterialAdjusterByType Create()
        {
            var result = NativeMethods.create_material_adjuster_by_type(out var adjusterPtr);
            DLLUtil.CheckDllError(result);
            return new MaterialAdjusterByType(adjusterPtr);
        }
        
        private MaterialAdjusterByType(IntPtr handle) : base(handle)
        {
        }
        
        protected override void DisposeNative()
        {
            var result = NativeMethods.delete_material_adjuster_by_type(Handle);
            DLLUtil.CheckDllError(result);
        }
        
        /// <summary>
        /// GML IDと、検索対象のパッケージの対応を登録します。
        /// 登録できたらtrueを返し、キーの重複により登録できなかったらfalseを返します。
        /// </summary>
        public bool RegisterType(string gmlID, CityObjectType type)
        {
            var result = NativeMethods.material_adjuster_by_type_register_type(Handle, gmlID, type, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }
        
        /// <summary>
        /// マテリアル分けのパターンを1つ登録します。
        /// 登録できたらtrueを返し、キーの重複により登録できなかったらfalseを返します。
        /// </summary>
        public bool RegisterMaterialPattern(CityObjectType type, int gameMaterialID)
        {
            var result = NativeMethods.material_adjuster_by_type_register_material_pattern(Handle, type, gameMaterialID, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }
        
        public void Exec(Model model)
        {
            var result = NativeMethods.material_adjuster_by_type_exec(Handle, model.Handle);
            DLLUtil.CheckDllError(result);
        }
        
        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult create_material_adjuster_by_type(
                out IntPtr adjusterPtr
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult delete_material_adjuster_by_type(
                [In] IntPtr adjusterPtr
            );
            
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult material_adjuster_by_type_register_type(
                [In] IntPtr adjusterPtr,
                [In] string gmlID,
                [In] CityObjectType type,
                [MarshalAs(UnmanagedType.U1)] out bool outIsSucceed
            );
            
            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult material_adjuster_by_type_register_material_pattern(
                [In] IntPtr adjusterPtr,
                [In] CityObjectType type,
                [In] int gameMaterialID,
                [MarshalAs(UnmanagedType.U1)] out bool outIsSucceed
            );
            
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult material_adjuster_by_type_exec(
                [In] IntPtr adjusterPtr,
                [In, Out] IntPtr modelPtr
            );
        }
    }
}
