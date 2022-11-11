using System;
using PLATEAU.Geometries;
using PLATEAU.Udx;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// <see cref="MeshCode"/> に応じた四角形のギズモを表示します。
    /// </summary>
    internal class MeshCodeGizmoDrawer : BoxGizmoDrawer
    {
        public MeshCode MeshCode { get; set; }
        
        private const float crossHairSizeMultiplier = 0.2f;
        
        private static readonly Color boxColorNormalLevel2 = new Color(0f, 0f, 0f);
        private static readonly Color boxColorNormalLevel3 = new Color(0f, 84f / 255f, 1f);
        private static readonly Color boxColorSelected = new Color(1f, 162f / 255f, 62f / 255f);
        private const int lineWidthLevel2 = 3;
        private const int lineWidthLevel3 = 2;


        /// <summary>
        /// メッシュコードに対応したギズモを表示するようにします。
        /// </summary>
        public void SetUp(MeshCode meshCode, GeoReference geoReference, Transform parent)
        {
            var extent = meshCode.Extent; // extent は緯度,経度,高さ
            // min, max は xyz の平面直行座標系に変換したもの
            var min = geoReference.Project(extent.Min);
            var max = geoReference.Project(extent.Max);
            var centerPosTmp = new Vector3(
                (float)(min.X + max.X) / 2.0f,
                AreaSelectorCursor.BoxCenterHeight,
                (float)(min.Z + max.Z) / 2.0f);
            var sizeTmp = new Vector3(
                (float)Math.Abs(max.X - min.X),
                AreaSelectorCursor.BoxUpperHeight - AreaSelectorCursor.BoxBottomHeight,
                (float)Math.Abs(max.Z - min.Z));
            Init(centerPosTmp, sizeTmp);
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
            // 追加でボックスの中心にクロスヘア（十字マーク）を描きます。
            var center = this.CenterPos;
            var crossHairLength = this.Size * crossHairSizeMultiplier;
            Gizmos.DrawLine(
                center + Vector3.left    *  crossHairLength.x * 0.5f,
                center + Vector3.right   * crossHairLength.x * 0.5f);
            Gizmos.DrawLine(
                center + Vector3.forward * crossHairLength.z * 0.5f,
                center + Vector3.back    * crossHairLength.z * 0.5f);
            #endif
        }
    }
}
