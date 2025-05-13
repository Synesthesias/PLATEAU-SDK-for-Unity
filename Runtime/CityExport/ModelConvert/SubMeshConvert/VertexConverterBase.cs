using System;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityExport.ModelConvert.SubMeshConvert
{
    /// <summary>
    /// エクスポートでの座標変換を担当します。
    /// Nextで繋げることで複数の変換処理に対応できます。
    /// </summary>
    public abstract class VertexConverterBase
    {
        public VertexConverterBase Next { get; set; }

        public Vector3 Convert(Vector3 src)
        {
            var dst = ConvertInner(src);
            if (Next == null)
            {
                return dst;
            }

            return Next.Convert(dst);
        }
        protected abstract Vector3 ConvertInner(Vector3 src);
    }

    /// <summary>
    /// <see cref="CoordinateAdder"/>のあとに<see cref="SubMeshConvert.CoordinateSystemConverter"/>を適用する座標変換器
    /// </summary>
    public class AdderAndCoordinateSystemConverter : VertexConverterBase
    {
        private CoordinateAdder firstConverter; // 平行移動変換
        private CoordinateSystemConverter secondConverter; // 座標変換

        public AdderAndCoordinateSystemConverter(Vector3 addOperand, CoordinateSystem targetAxis)
        {
            firstConverter = new CoordinateAdder(addOperand);
            secondConverter = new CoordinateSystemConverter(targetAxis);
            firstConverter.Next = secondConverter;
        }
        protected override Vector3 ConvertInner(Vector3 src)
        {
            // 1番目と2番目の変換を両方適用(Nextによりチェイン)
            return firstConverter.Convert(src);
        }

        /// <summary>
        /// 平行移動変換をスキップし、座標変換のみを適用
        /// </summary>
        public Vector3 ConvertOnlyCoordinateSystem(Vector3 src)
        {
            return secondConverter.Convert(src);
        }
    }

    /// <summary>
    /// 座標変換器を作って返します。
    /// </summary>
    public static class VertexConverterFactory
    {

        public static AdderAndCoordinateSystemConverter CreateByExportOptions(MeshExportOptions options, PlateauVector3d referencePoint, Vector3 rootPos)
        {
            return options.TransformType switch
            {
                MeshExportOptions.MeshTransformType.Local => VertexConverterFactory.LocalCoordinateSystemConverter(
                    options.MeshAxis, rootPos),
                MeshExportOptions.MeshTransformType.PlaneCartesian => VertexConverterFactory
                    .PlaneCartesianCoordinateSystemConverter(options.MeshAxis, referencePoint.ToUnityVector(), rootPos),
                _ => throw new Exception("Unknown transform type.")
            };
        }
        /// <summary>
        /// 基準点からの相対座標にした上で座標軸変換するConverterを返します。
        /// </summary>
        public static AdderAndCoordinateSystemConverter LocalCoordinateSystemConverter(CoordinateSystem targetAxis, Vector3 rootPos)
        {
            return new AdderAndCoordinateSystemConverter(rootPos * -1, targetAxis);
        }

        /// <summary>
        /// 日本の基準座標系上の座標に直したうえで座標軸変換するConverterを返します。
        /// </summary>
        public static AdderAndCoordinateSystemConverter PlaneCartesianCoordinateSystemConverter(CoordinateSystem targetAxis,
            Vector3 referencePoint, Vector3 rootPos)
        {
            return new AdderAndCoordinateSystemConverter(referencePoint - rootPos, targetAxis);
        }

        /// <summary>
        /// 何もしないConverterを返します。
        /// </summary>
        /// <returns></returns>
        public static AdderAndCoordinateSystemConverter NoopConverter()
        {
            return new AdderAndCoordinateSystemConverter(Vector3.zero, CoordinateSystem.EUN/*Unity座標*/);
        }
        
    }

    
    /// <summary>
    /// 座標軸を変更する座標変換器です。
    /// </summary>
    public class CoordinateSystemConverter : VertexConverterBase
    {
        public CoordinateSystem TargetAxis { get; private set; }

        public CoordinateSystemConverter(CoordinateSystem targetAxis)
        {
            this.TargetAxis = targetAxis;
        }
        
        protected override Vector3 ConvertInner(Vector3 src)
        {
            var vert = GeoReference.ConvertAxisToENU(CoordinateSystem.EUN, src.ToPlateauVector());
            vert = GeoReference.ConvertAxisFromENUTo(TargetAxis, vert);
            return vert.ToUnityVector();
        }
    }

    /// <summary>
    /// 座標に指定の値を追加する座標変換器です。
    /// </summary>
    public class CoordinateAdder : VertexConverterBase
    {
        public Vector3 Operand { get; private set; }

        public CoordinateAdder(Vector3 operand)
        {
            this.Operand = operand;
        }

        protected override Vector3 ConvertInner(Vector3 src)
        {
            return src + Operand;
        }
    }

}