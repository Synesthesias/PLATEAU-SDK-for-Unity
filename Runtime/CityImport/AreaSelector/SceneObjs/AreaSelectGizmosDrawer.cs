using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Codice.CM.Common;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    internal class AreaSelectGizmosDrawer : HandlesBase
    {
        private List<MeshCodeGizmoDrawer> drawers = new List<MeshCodeGizmoDrawer>();
        private AreaSelectorCursor cursor;
        
        
        /// <summary>
        /// メッシュコードのリストを受け取り、メッシュコード1つにつき1つのギズモ描画オブジェクトを生成します。
        /// </summary>
        public void Init(
            ReadOnlyCollection<MeshCode> meshCodes,
            int coordinateZoneID, out GeoReference geoReference)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("", "範囲座標を計算中です...", 0.5f);
#endif
            this.cursor = new AreaSelectorCursor();
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
                // var gizmoObj = new GameObject($"{meshCode}");
                // var drawer = gizmoObj.AddComponent<MeshCodeGizmoDrawer>();
                var drawer = new MeshCodeGizmoDrawer();
                drawer.SetUp(meshCode, geoReference, gizmoParent);
                this.drawers.Add(drawer);
            }
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }
        
        public IEnumerable<MeshCode> SelectedMeshCodes {
            get
            {
                return this.cursor.SelectedMeshCodes(this.drawers)
                    .Select(drawer => drawer.MeshCode);
            }
        }

        public Extent CursorExtent(int coordinateZoneID, PlateauVector3d referencePoint)
        {
            return this.cursor.GetExtent(coordinateZoneID, referencePoint);
        }

        protected override void OnSceneGUI(SceneView sceneView)
        {
            this.cursor.DrawAndUpdate();
        }

        private void OnDrawGizmos()
        {
            foreach (var box in this.drawers) box.ApplyStyle(false);
            var selected = this.cursor.SelectedMeshCodes(this.drawers);
            foreach (var select in selected) select.ApplyStyle(true);
            foreach(var drawer in this.drawers) drawer.Draw();
        }
    }
}
