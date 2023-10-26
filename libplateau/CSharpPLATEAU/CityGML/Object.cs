using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// CityGMLにおける全てのオブジェクトのベースクラスです。
    /// ユニークIDと0個以上の属性ペアを持ち、属性ペアはすべて <see cref="NativeAttributesMap"/> に格納されています。
    /// </summary>
    public class Object
    {
        private NativeAttributesMap nativeAttributesMap; // get されるまでは null なので null許容型とします。
        private string id = "";
        
        /// <summary>
        /// セーフハンドルを取得します。
        /// </summary>
        public IntPtr Handle { get; }

        internal Object(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// オブジェクトのユニークIDを取得します。
        /// </summary>
        public string ID
        {
            get
            {
                if (this.id != "")
                {
                    return this.id;
                }
                
                this.id = DLLUtil.GetNativeString(
                    Handle,
                    NativeMethods.plateau_object_get_id);
                return this.id;
            }
            protected set => this.id = value;
        }

        /// <summary> 属性の辞書を取得します。 </summary>
        /// <returns> <see cref="NativeAttributesMap"/> 型で返します。</returns>
        public virtual NativeAttributesMap NativeAttributesMap
        {
            get
            {
                if (this.nativeAttributesMap == null)
                {
                    IntPtr mapPtr = DLLUtil.GetNativeValue<IntPtr>(Handle,
                        NativeMethods.plateau_object_get_attributes_map);
                    var map = new NativeAttributesMap(mapPtr);
                    this.nativeAttributesMap = map;
                }
                return this.nativeAttributesMap;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_object_get_id(
                [In] IntPtr objHandle,
                out IntPtr outStrPtr,
                out int strLength);


            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_object_get_attributes_map(
                [In] IntPtr objHandle,
                out IntPtr attributesMapPtr);
        }
    }
}
