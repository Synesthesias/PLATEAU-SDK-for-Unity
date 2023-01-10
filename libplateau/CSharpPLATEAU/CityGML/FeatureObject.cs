using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;

namespace PLATEAU.CityGML
{
    /// <summary>
    /// CityGMLにおける全ての地物オブジェクトのベースクラスです。
    /// <see cref="Envelope"/> (オブジェクトの存在範囲を2点の座標で示したもの)を持ちます。
    /// </summary>
    public class FeatureObject : Object
    {
        internal FeatureObject(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// 建築物の範囲を double[6] で返します。
        /// </summary>
        /// <returns> 戻り値 double[6] の配列の中身は座標 { lower_x, lower_y, lower_z, upper_x, upper_y, upper_z } です。</returns>
        public double[] Envelope
        {
            get
            {
                const int envelopeArrayLength = 6;

                double[] ret = new double[envelopeArrayLength];
                APIResult result = NativeMethods.plateau_feature_object_get_envelope(Handle, ret);
                DLLUtil.CheckDllError(result);

                return ret;
            }
        }

        /// <summary>
        /// 建物の範囲を設定します。
        /// </summary>
        public void SetEnvelope(
            double lowerX, double lowerY, double lowerZ,
            double upperX, double upperY, double upperZ)
        {
            APIResult result = NativeMethods.plateau_feature_object_set_envelope(
                Handle,
                lowerX, lowerY, lowerZ, upperX, upperY, upperZ
            );
            DLLUtil.CheckDllError(result);
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_feature_object_get_envelope(
                [In] IntPtr featureObject,
                [Out] double[] outEnvelope
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_feature_object_set_envelope(
                [In] IntPtr featureObject,
                double lowerX, double lowerY, double lowerZ,
                double upperX, double upperY, double upperZ
            );
        }
    }
}
