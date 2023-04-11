using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.CityImport.AreaSelector.SceneObjs
{
    /// <summary>
    /// 範囲選択に関するギズモを表示する MonoBehaviour です。
    /// 地域メッシュコードと、範囲選択カーソルと、利用可能LOD表示を保持して表示します。
    /// </summary>
    internal class AreaSelectGizmosDrawer : HandlesBase
    {
        /// <summary> 地域メッシュの範囲表示です。 </summary>
        private readonly List<MeshCodeGizmoDrawer> meshCodeDrawers = new List<MeshCodeGizmoDrawer>();
        /// <summary> 範囲選択カーソルです。 </summary>
        private AreaSelectorCursor cursor;
        /// <summary> 地域メッシュごとの利用可能LOD表示です。 </summary>
        private AreaLodController areaLod;
        private GeoReference geoReference;
        
        public void Init(
            ReadOnlyCollection<MeshCode> meshCodes, DatasetSourceConfig datasetSourceConfig,
            int coordinateZoneID, out GeoReference outGeoReference)
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
            outGeoReference = GeoReference.Create(referencePoint, 1f, CoordinateSystem.EUN, coordinateZoneID);
            foreach (var meshCode in meshCodes)
            {
                var drawer = new MeshCodeGizmoDrawer();
                drawer.SetUp(meshCode, outGeoReference);
                this.meshCodeDrawers.Add(drawer);
            }

            this.areaLod = new AreaLodController(datasetSourceConfig, outGeoReference, meshCodes);
            this.geoReference = outGeoReference;
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }
        
        public IEnumerable<MeshCode> SelectedMeshCodes {
            get
            {
                return this.cursor.SelectedMeshCodes(this.meshCodeDrawers)
                    .Select(drawer => drawer.MeshCode);
            }
        }

        public Extent CursorExtent(int coordinateZoneID, PlateauVector3d referencePoint)
        {
            return this.cursor.GetExtent(coordinateZoneID, referencePoint);
        }

        #if UNITY_EDITOR
        protected override void OnSceneGUI(SceneView sceneView)
        {
            this.cursor.DrawSceneGUI();
            this.areaLod.Update(GSIMapLoaderZoomSwitch.CalcCameraExtent(sceneView.camera, this.geoReference));
            this.areaLod.DrawSceneGUI(sceneView.camera);
        }
        #endif

        private void OnDrawGizmos()
        {
            foreach (var mcDrawer in this.meshCodeDrawers)
            {
                mcDrawer.ApplyStyle();
                // 大きな粒度の線は優先して表示されるようにします。
                mcDrawer.Priority = mcDrawer.MeshCode.Level switch
                {
                    2 => 1,
                    3 => 0,
                    _ => 0
                };
            }

            var gizmosToDraw = new List<BoxGizmoDrawer>();
            gizmosToDraw.AddRange(this.meshCodeDrawers);
            gizmosToDraw.Add(this.cursor);
            BoxGizmoDrawer.DrawWithPriority(gizmosToDraw);
        }
    }
}
