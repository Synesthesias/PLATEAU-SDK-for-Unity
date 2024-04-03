using PLATEAU.Geometries;
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

            return Next.Convert(src);
        }
        protected abstract Vector3 ConvertInner(Vector3 src);
    }

    /// <summary>
    /// 座標変換器を作って返します。
    /// </summary>
    public static class VertexConverterFactory
    {
        /// <summary>
        /// 基準点からの相対座標にした上で座標軸変換するConverterを返します。
        /// </summary>
        public static VertexConverterBase LocalCoordinateSystemConverter(CoordinateSystem targetAxis, Vector3 rootPos)
        {
            var relativeMaker = new CoordinateAdder(rootPos * -1);
            var coordinateSystemConverter = new CoordinateSystemConverter(targetAxis);
            relativeMaker.Next = coordinateSystemConverter;
            return relativeMaker;
        }

        /// <summary>
        /// 日本の基準座標系上の座標に直したうえで座標軸変換するConverterを返します。
        /// </summary>
        public static VertexConverterBase PlaneCartesianCoordinateSystemConverter(CoordinateSystem targetAxis,
            Vector3 referencePoint, Vector3 rootPos)
        {
            var adder = new CoordinateAdder(referencePoint - rootPos);
            var coordinateSystemConverter = new CoordinateSystemConverter(targetAxis);
            adder.Next = coordinateSystemConverter;
            return adder;
        }
    }

    
    /// <summary>
    /// 座標軸を変更する座標変換器です。
    /// </summary>
    public class CoordinateSystemConverter : VertexConverterBase
    {
        private CoordinateSystem targetAxis;

        public CoordinateSystemConverter(CoordinateSystem targetAxis)
        {
            this.targetAxis = targetAxis;
        }
        
        protected override Vector3 ConvertInner(Vector3 src)
        {
            var vert = GeoReference.ConvertAxisToENU(CoordinateSystem.EUN, src.ToPlateauVector());
            vert = GeoReference.ConvertAxisFromENUTo(targetAxis, vert);
            return vert.ToUnityVector();
        }
    }

    /// <summary>
    /// 座標に指定の値を追加する座標変換器です。
    /// </summary>
    public class CoordinateAdder : VertexConverterBase
    {
        private Vector3 operand;

        public CoordinateAdder(Vector3 operand)
        {
            this.operand = operand;
        }

        protected override Vector3 ConvertInner(Vector3 src)
        {
            return src + operand;
        }
    }

    /// <summary>
    /// 何もしない座標変換器です。
    /// </summary>
    public class NoopConverter : VertexConverterBase
    {
        protected override Vector3 ConvertInner(Vector3 src)
        {
            return src;
        }
    }
    
}