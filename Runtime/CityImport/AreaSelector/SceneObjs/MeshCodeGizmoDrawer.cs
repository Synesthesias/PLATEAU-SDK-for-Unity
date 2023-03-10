using System;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// <see cref="MeshCode"/> に応じた四角形のギズモを表示します。
    /// </summary>
    internal class MeshCodeGizmoDrawer : BoxGizmoDrawer
    {
        public MeshCode MeshCode { get; set; }

        /// <summary> メッシュコードの中の縦横に弾かれる細かい線について、、縦横それぞれメッシュコードを何分割するかです。 </summary>
        private const int innerDivideCount = 4;

        private static readonly Color boxColorNormalLevel2 = Color.black;
        private static readonly Color boxColorNormalLevel3 = new Color(0f, 84f / 255f, 1f);
        private static readonly Color boxColorSelected = new Color(1f, 162f / 255f, 62f / 255f);
        private const int lineWidthLevel2 = 3;
        private const int lineWidthLevel3 = 2;


        /// <summary>
        /// メッシュコードに対応したギズモを表示するようにします。
        /// </summary>
        public void SetUp(MeshCode meshCode, GeoReference geoReference)
        {
            var extent = meshCode.Extent; // extent は緯度,経度,高さ
            // min, max は xyz の平面直行座標系に変換したもの
            var min = geoReference.Project(extent.Min);
            var max = geoReference.Project(extent.Max);
            var centerPosTmp = new Vector3(
                (float)(min.X + max.X) / 2.0f,
                AreaSelectorCursor.BoxY,
                (float)(min.Z + max.Z) / 2.0f);
            var sizeTmp = new Vector3(
                (float)Math.Abs(max.X - min.X),
                1,
                (float)Math.Abs(max.Z - min.Z));
            Init(centerPosTmp, sizeTmp, meshCode.ToString());
            MeshCode = meshCode;
        }

        public void ApplyStyle(bool selected)
        {
            switch (MeshCode.Level)
            {
                case 2:
                    LineWidth = lineWidthLevel2;
                    BoxColor = selected ? boxColorSelected : boxColorNormalLevel2;
                    break;
                default:
                    LineWidth = lineWidthLevel3;
                    BoxColor = selected ? boxColorSelected : boxColorNormalLevel3;
                    break;
            }
        }

        protected override void AdditionalGizmo()
        {
#if UNITY_EDITOR
            // 追加でボックスの中を分割するラインを引きます。
            var min = AreaMin;
            var xDiff = this.Size.x / innerDivideCount;
            var linePosUp = min + Vector3.right * xDiff;
            // 縦のライン
            for (int i = 0; i < innerDivideCount-1; i++)
            {
                Gizmos.DrawLine(linePosUp, linePosUp + Vector3.forward * this.Size.z);
                linePosUp += Vector3.right * xDiff;
            }
            // 横のライン
            var zDiff = this.Size.z / innerDivideCount;
            var linePosLeft = min + Vector3.forward * zDiff;
            for (int i = 0; i < innerDivideCount - 1; i++)
            {
                Gizmos.DrawLine(linePosLeft, linePosLeft + Vector3.right * this.Size.x);
                linePosLeft += Vector3.forward * zDiff;
            }
#endif
        }
    }
}
