using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// <see cref="MeshCode"/> に応じた四角形のギズモを表示します。
    /// </summary>
    internal class MeshCodeGizmoDrawer : BoxGizmoDrawer
    {
        public MeshCode MeshCode { get; set; }

        protected override float SizeMultiplier => 1f;
        private const float crossHairSizeMultiplier = 0.2f;
        
        private static readonly Color boxColorNormalLevel2 = new Color(0f, 0f, 0f);
        private static readonly Color boxColorNormalLevel3 = new Color(0f, 84f / 255f, 1f);
        private static readonly Color boxColorSelected = new Color(1f, 162f / 255f, 62f / 255f);
        private const int lineWidthLevel2 = 3;
        private const int lineWidthLevel3 = 2;
        
        /// <summary>
        /// メッシュコードのリストを受け取り、メッシュコード1つにつき1つのギズモ描画オブジェクトを生成します。
        /// </summary>
        public static void PlaceMeshCodeDrawers(
            ReadOnlyCollection<MeshCode> meshCodes, ICollection<MeshCodeGizmoDrawer> boxGizmoDrawers,
            int coordinateZoneID, out GeoReference geoReference)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("", "範囲座標を計算中です...", 0.5f);
#endif
            // 仮に (0,0,0) を referencePoint とする geoReference を作成
            using var geoReferenceTmp = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            // 中心を計算し、そこを基準点として geoReference を再設定します。
            var referencePoint = new PlateauVector3d(0, 0, 0);
            
            foreach (var meshCode in meshCodes)
            {
                var geoMin = meshCode.Extent.Min;
                var geoMax = meshCode.Extent.Max;
                var min = geoReferenceTmp.Project(geoMin);
                var max = geoReferenceTmp.Project(geoMax);
                var center = (min + max) * 0.5;
                referencePoint += center;
            }
            referencePoint /= meshCodes.Count;
            referencePoint.Y = 0;
            geoReference = new GeoReference(referencePoint, 1f, CoordinateSystem.EUN, coordinateZoneID);
            var gizmoParent = new GameObject("MeshCodeGizmos").transform;
            foreach (var meshCode in meshCodes)
            {
                var gizmoObj = new GameObject($"{meshCode}");
                var drawer = gizmoObj.AddComponent<MeshCodeGizmoDrawer>();
                drawer.SetUp(meshCode, geoReference, gizmoParent);
                boxGizmoDrawers.Add(drawer);
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// メッシュコードに対応したギズモを表示するようにします。
        /// </summary>
        public void SetUp(MeshCode meshCode, GeoReference geoReference, Transform parent)
        {
            var extent = meshCode.Extent; // extent は緯度,経度,高さ
            // min, max は xyz の平面直行座標系に変換したもの
            var min = geoReference.Project(extent.Min);
            var max = geoReference.Project(extent.Max);
            var center = new Vector3(
                (float)(min.X + max.X) / 2.0f,
                AreaSelectorCursor.BoxCenterHeight,
                (float)(min.Z + max.Z) / 2.0f);
            var size = new Vector3(
                (float)Math.Abs(max.X - min.X),
                AreaSelectorCursor.BoxUpperHeight - AreaSelectorCursor.BoxBottomHeight,
                (float)Math.Abs(max.Z - min.Z));
            var gizmoTrans = transform;
            gizmoTrans.position = center;
            gizmoTrans.localScale = size;
            gizmoTrans.parent = parent;
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
            var trans = transform;
            var center = trans.position;
            var crossHairLength = trans.localScale * crossHairSizeMultiplier;
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
