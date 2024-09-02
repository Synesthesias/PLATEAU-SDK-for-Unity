using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;

namespace PLATEAU.MaterialAdjust
{
    public class MaterialAdjusterByAttr : PInvokeDisposable
    {
        public static MaterialAdjusterByAttr Create()
        {
            var result = NativeMethods.create_material_adjuster_by_attr(out var adjusterPtr);
            DLLUtil.CheckDllError(result);
            return new MaterialAdjusterByAttr(adjusterPtr);
        }
        
        private MaterialAdjusterByAttr(IntPtr handle) : base(handle)
        {
        }

        protected override void DisposeNative()
        {
            var result = NativeMethods.delete_material_adjuster_by_attr(Handle);
            DLLUtil.CheckDllError(result);
        }
        
        /// <summary>
        /// GML IDと、検索対象の属性の値を登録します。
        /// 登録できたらtrueを返し、キーの重複により登録できなかったらfalseを返します。
        /// </summary>
        public bool RegisterAttribute(string gmlID, string attribute)
        {
            var result = NativeMethods.material_adjuster_by_attr_register_attribute(Handle, gmlID, attribute, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }
        
        /// <summary>
        /// マテリアル分けのパターンを1つ登録します。
        /// 登録できたらtrueを返し、キーの重複により登録できなかったらfalseを返します。
        /// </summary>
        public bool RegisterMaterialPattern(string attribute, int gameMaterialID)
        {
            var result = NativeMethods.material_adjuster_by_attr_register_material_pattern(Handle, attribute, gameMaterialID, out bool isSucceed);
            DLLUtil.CheckDllError(result);
            return isSucceed;
        }

        public void Exec(Model model)
        {
            var result = NativeMethods.material_adjuster_by_attr_exec(Handle, model.Handle);
            DLLUtil.CheckDllError(result);
        }
        
        
        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult create_material_adjuster_by_attr(
                out IntPtr adjuster
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult delete_material_adjuster_by_attr(
                [In] IntPtr adjusterPtr
            );

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult material_adjuster_by_attr_register_attribute(
                [In] IntPtr adjusterPtr,
                [In] string gmlID,
                [In] string attribute,
                [MarshalAs(UnmanagedType.U1)] out bool outIsSucceed
            );

            [DllImport(DLLUtil.DllName, CharSet = CharSet.Ansi)]
            internal static extern APIResult material_adjuster_by_attr_register_material_pattern(
                [In] IntPtr adjusterPtr,
                [In] string attribute,
                [In] int gameMaterialID,
                [MarshalAs(UnmanagedType.U1)] out bool outIsSucceed
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult material_adjuster_by_attr_exec(
                [In] IntPtr adjusterPtr,
                [In, Out] IntPtr modelPtr
            );
        }
    }
}
