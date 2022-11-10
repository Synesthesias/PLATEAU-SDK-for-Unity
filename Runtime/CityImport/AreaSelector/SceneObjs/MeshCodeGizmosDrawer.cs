using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    internal class MeshCodeGizmosDrawer : HandlesBase
    {
        private List<MeshCodeGizmoDrawer> drawers = new List<MeshCodeGizmoDrawer>();
        private AreaSelectorCursor cursor;
        
        
        /// <summary>
        /// メッシュコードのリストを受け取り、メッシュコード1つにつき1つのギズモ描画オブジェクトを生成します。
        /// </summary>
        public void Init(
            ReadOnlyCollection<MeshCode> meshCodes,
            int coordinateZoneID, out GeoReference geoReference, AreaSelectorCursor cursorArg)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("", "範囲座標を計算中です...", 0.5f);
#endif
            if (cursorArg == null)
            {
                Debug.LogError("cursor is null.");
            }
            this.cursor = cursorArg;
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

        protected override void OnSceneGUI(SceneView sceneView)
        {
        //     foreach (var box in this.drawers) box.ApplyStyle(false);
        //     var selected = this.cursor.SelectedMeshCodes(this.drawers);
        //     foreach (var select in selected) select.ApplyStyle(true);
        //     foreach(var drawer in this.drawers) drawer.Draw();
        }

        private void OnDrawGizmos()
        {foreach (var box in this.drawers) box.ApplyStyle(false);
            var selected = this.cursor.SelectedMeshCodes(this.drawers);
            foreach (var select in selected) select.ApplyStyle(true);
            foreach(var drawer in this.drawers) drawer.Draw();
        }
    }
}
