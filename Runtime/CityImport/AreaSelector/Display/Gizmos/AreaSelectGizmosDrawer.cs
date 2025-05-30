using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityImport.AreaSelector.Display.Gizmos.AreaRectangles;
using PLATEAU.CityImport.AreaSelector.Display.Gizmos.LODIcons;
using PLATEAU.CityImport.AreaSelector.Display.Maps;
using PLATEAU.CityImport.Config;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using System;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos
{
    /// <summary>
    /// 範囲選択に関するギズモを表示する MonoBehaviour です。
    /// 地域メッシュコードと、範囲選択カーソルと、利用可能LOD表示を保持して表示します。
    /// </summary>
    internal class AreaSelectGizmosDrawer : HandlesBase
    {
        private const int MouseLeftButton = 0;
        /// <summary> 地域メッシュの範囲表示です。 </summary>
        private readonly List<GridCodeGizmoDrawer> gridCodeDrawers = new();
        /// <summary> 地域メッシュごとの利用可能LOD表示です。 </summary>
        private AreaLodController areaLod;
        private GeoReference geoReference;
        private AreaSelectionGizmoDrawer areaSelectionGizmoDrawer;
        #if UNITY_EDITOR
        private readonly GizmoActiveChecker gizmoActiveChecker = new GizmoActiveChecker();
        #endif
        private bool isShiftButtonPressed;
        private bool isLeftMouseButtonPressed;
        private bool isLeftMouseAndShiftButtonPressed;
        private bool isLeftMouseButtonMoved;
        private bool isLeftMouseAndShiftButtonMoved;
        
        public void Init(
            NativeVectorGridCode gridCodes, IDatasetSourceConfig datasetSourceConfig,
            int coordinateZoneID, out GeoReference outGeoReference)
        {
            int numGrids = gridCodes.Length;
            if (numGrids == 0)
            {
                throw new ArgumentException("グリッドコードの数が0です。");
            }
            using var progressBar = new ProgressBar();
            progressBar.Display("範囲座標を計算中です...", 0.5f);
            
            // 仮に (0,0,0) を referencePoint とする geoReference を作成
            using var geoReferenceTmp = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            // 中心を計算し、そこを基準点として geoReference を再設定します。
            var referencePoint = new PlateauVector3d(0, 0, 0);

            for (int i = 0; i < numGrids; i++)
            {
                using var gridCode = gridCodes.At(i);
                var geoMin = gridCode.Extent.Min;
                var geoMax = gridCode.Extent.Max;
                var min = geoReferenceTmp.Project(geoMin);
                var max = geoReferenceTmp.Project(geoMax);
                var center = (min + max) * 0.5;
                referencePoint += center;
            }
            
            referencePoint /= numGrids;
            referencePoint.Y = 0;
            outGeoReference = GeoReference.Create(referencePoint, 1f, CoordinateSystem.EUN, coordinateZoneID);
            for (int i = 0; i < numGrids; i++)
            {
                using var gridCode = gridCodes.At(i);
                // Level4以上の(細かい)MeshCodeであって、別のLevel3の範囲に含まれているものは重複のため除外します。
                bool isDuplicate = false;
                if (gridCode.IsSmallerThanNormalGml)
                {
                    for (int j = 0; j < gridCodes.Length; j++)
                    {
                        using var other = gridCodes.At(j);
                        if (other.IsNormalGmlLevel && other.IsValid)
                        {
                            var upper = gridCode.Upper();
                            while (upper.IsValid && !upper.IsNormalGmlLevel)
                            {
                                if (upper.StringCode == other.StringCode)
                                {
                                    isDuplicate = true;
                                    upper.Dispose();
                                    break;
                                }

                                var next = upper.Upper();
                                upper.Dispose();
                                upper = next;
                            }
                        }

                        if (isDuplicate) break;
                    }
                }

                if (isDuplicate) continue;
                var drawer = gridCode.StringCode.Any(char.IsLetter) 
                    ? new StandardMapGizmoDrawer() 
                    : new GridCodeGizmoDrawer();
                drawer.SetUp(GridCode.CopyFrom(gridCode.Handle), outGeoReference);
                this.gridCodeDrawers.Add(drawer);
            }

            this.areaLod = new AreaLodController(datasetSourceConfig, outGeoReference, gridCodes);
            this.geoReference = outGeoReference;
            this.areaSelectionGizmoDrawer = new AreaSelectionGizmoDrawer();
            this.areaSelectionGizmoDrawer.SetUp();
        }
        
        public GridCodeList SelectedGridCodes => GridCodeList.CreateFromGridCodeDrawers(this.gridCodeDrawers);

        public void ResetSelectedArea()
        {
            foreach (var meshCodeGizmoDrawer in this.gridCodeDrawers)
            {
                meshCodeGizmoDrawer.ResetSelectedArea();
            }
        }

        public bool IsSelectedArea()
        {
            return 0 < this.gridCodeDrawers.Where(meshCodeDrawer => meshCodeDrawer.IsSelectedArea()).ToList().Count;
        }

        public void SwitchLodIcon(int lod, bool isCheck)
        {
            this.areaLod.SwitchLodIcon(lod, isCheck);
        }

        #if UNITY_EDITOR

        protected override void OnSceneGUI(SceneView sceneView)
        {
            gizmoActiveChecker.CheckAndShow(sceneView);
            UpdateMousePressedStatus();
            
            this.areaLod.Update(GSIMapLoaderZoomSwitch.CalcCameraExtent(sceneView.camera, this.geoReference));
            this.areaLod.DrawSceneGUI(sceneView.camera);

            foreach (var meshCodeGizmoDrawer in this.gridCodeDrawers)
            {
                meshCodeGizmoDrawer.DrawSceneGUI();
            }
        }
        #endif

        private void OnDrawGizmos()
        {
            foreach (var meshCodeGizmoDrawer in this.gridCodeDrawers)
            {
                // 大きな粒度の線は優先して表示されるようにします。
                meshCodeGizmoDrawer.Priority = 999 - 
                                               (meshCodeGizmoDrawer.GridCode.IsSmallerThanNormalGml ? 1 : 0) -
                                               (meshCodeGizmoDrawer.GridCode.IsNormalGmlLevel ? 1 : 0);
            }

            var gizmosToDraw = new List<BoxGizmoDrawer>();
            gizmosToDraw.AddRange(this.gridCodeDrawers);
            gizmosToDraw.Add(this.areaSelectionGizmoDrawer);
            BoxGizmoDrawer.DrawWithPriority(gizmosToDraw);
        }

        private void UpdateMousePressedStatus()
        {
            // 左クリックのMouseUpのイベントを受け取るためにデフォルトコントロールに追加
            #if UNITY_EDITOR
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            #endif
            
            var currentEvent = Event.current;
            if (currentEvent == null)
                return;
            
            var standardMapDrawers = gridCodeDrawers.OfType<StandardMapGizmoDrawer>().ToList();
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == MouseLeftButton)
                    {
                        if (this.isLeftMouseButtonMoved || this.isLeftMouseAndShiftButtonMoved)
                            return;

                        if (this.isShiftButtonPressed)
                        {
                            this.isLeftMouseAndShiftButtonPressed = true;
                            this.areaSelectionGizmoDrawer.SetTrackingStartedPosition(currentEvent.mousePosition);
                        }
                        else
                        {
                            this.isLeftMouseButtonPressed = true;
                            this.areaSelectionGizmoDrawer.SetTrackingStartedPosition(currentEvent.mousePosition);
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (currentEvent.button == MouseLeftButton)
                    {
                        if (this.isLeftMouseButtonPressed)
                        {
                            foreach (var meshCodeGizmoDrawer in this.gridCodeDrawers) 
                            {
                                if (meshCodeGizmoDrawer is StandardMapGizmoDrawer) 
                                {
                                    continue;
                                }
                                #if UNITY_EDITOR
                                meshCodeGizmoDrawer.ToggleSelectArea(currentEvent.mousePosition, standardMapDrawers);
                                #endif
                            }
                        }
                        else if (this.isLeftMouseButtonMoved || this.isLeftMouseAndShiftButtonMoved)
                        {
                            foreach (var meshCodeGizmoDrawer in this.gridCodeDrawers) 
                            {
                                if (meshCodeGizmoDrawer is StandardMapGizmoDrawer) 
                                {
                                    continue;
                                }
                                #if UNITY_EDITOR
                                meshCodeGizmoDrawer.SetSelectArea(
                                    this.areaSelectionGizmoDrawer.AreaSelectionMin,
                                    this.areaSelectionGizmoDrawer.AreaSelectionMax,
                                    this.isLeftMouseButtonMoved,
                                    standardMapDrawers);
                                #endif
                            }
                        }
                        
                        this.areaSelectionGizmoDrawer.ClearAreaSelectionGizmo();

                        this.isLeftMouseButtonPressed = false;
                        this.isLeftMouseAndShiftButtonPressed = false;
                        this.isLeftMouseButtonMoved = false;
                        this.isLeftMouseAndShiftButtonMoved = false;
                    }

                    break;
                case EventType.MouseDrag:
                    if (currentEvent.button == MouseLeftButton)
                    {
                        if (this.isLeftMouseAndShiftButtonPressed)
                        {
                            this.isLeftMouseButtonPressed = false;
                            this.isLeftMouseAndShiftButtonPressed = false;
                            this.isLeftMouseAndShiftButtonMoved = true;
                        }
                        else if (this.isLeftMouseButtonPressed)
                        {
                            this.isLeftMouseButtonPressed = false;
                            this.isLeftMouseAndShiftButtonPressed = false;
                            this.isLeftMouseButtonMoved = true;
                        }
 

                        if (this.isLeftMouseButtonMoved || this.isLeftMouseAndShiftButtonMoved)
                        {
                            this.areaSelectionGizmoDrawer.UpdateAreaSelectionGizmo(currentEvent.mousePosition, this.isLeftMouseButtonMoved);
                        }
                    }

                    break;
                case EventType.KeyDown:
                    if (currentEvent.keyCode == KeyCode.LeftShift)
                    {
                        if (this.isLeftMouseButtonMoved || this.isLeftMouseAndShiftButtonMoved)
                            return;

                        if (this.isLeftMouseButtonPressed)
                        {
                            this.isLeftMouseAndShiftButtonPressed = true;
                        }
                        else
                        {
                            this.isShiftButtonPressed = true;
                        }
                    }

                    break;
                case EventType.KeyUp:
                    if (currentEvent.keyCode == KeyCode.LeftShift)
                    {
                        this.isShiftButtonPressed = false;
                    }

                    break;
            }
        }
    }
}
