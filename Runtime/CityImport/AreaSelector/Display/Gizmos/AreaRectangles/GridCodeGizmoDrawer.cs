using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector.Display.Gizmos.AreaRectangles
{
    /// <summary>
    /// <see cref="GridCode"/> に応じた四角形のギズモを表示します。
    /// </summary>
    internal class GridCodeGizmoDrawer : BoxGizmoDrawer
    {
        
        protected int divideNumColumn;
        protected int divideNumRow;
        protected const int LineWidthLevel2 = 3;
        protected const int LineWidthLevel3 = 2;
        protected const int LineWidthLevel4 = 1;
        private const float CenterPosY = 15f;
        private const float RayCastMaxDistance = 100000.0f;
        protected virtual Color BoxColorNormalLevel2 => Color.black;
        protected virtual Color BoxColorNormalLevel3 => new(0f, 84f / 255f, 1f);
        private static readonly Color BoxColorNormalLevel4 = new(0f, 84f / 255f, 1f);
        protected virtual Color HandleColor => new(1f, 72f / 255f, 0f);
        private static readonly Color SelectedFaceColor = new(1f, 204f / 255f, 153f / 255f, 0.5f);
        private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);
        private static readonly List<string> SuffixMeshIds = new()
        {
            "11", "12", "21", "22",
            "13", "14", "23", "24",
            "31", "32", "41", "42",
            "33", "34", "43", "44",
        };
        
        private GeoReference geoReference;
        private List<bool> selectedAreaList;
        private const int GridCodeDivideNum = 4;
        
        private List<Vector3[]> allRectVerts = new();
        
        // FIXME RowがXでColumnがZって直感に反する気がする。逆では？
        private int GetRowIndex(double minX, double maxX, int numGrid, double value)
        {
            var gridSize = (maxX - minX) / numGrid;
            for (var i = 0; i < numGrid; i++)
            {
                if (value <= minX + gridSize * (i + 1))
                {
                    return i;
                }
            }

            return numGrid - 1;
        }

        private int GetColumnIndex(double minZ, double maxZ, int numGrid, double value)
        {
            var gridSize = (maxZ - minZ) / numGrid;
            for (var i = 0; i < numGrid; i++)
            {
                if (value <= minZ + gridSize * (i + 1))
                {
                    return i;
                }
            }

            return numGrid - 1;
        }
            
        /// <summary>
        /// メッシュコードに対応したギズモを表示するようにします。
        /// </summary>
        public void SetUp(GridCode gridCode, GeoReference geoReferenceArg)
        {
            var extent = gridCode.Extent; // extent は緯度,経度,高さ

            this.geoReference = geoReferenceArg;

            // min, max は xyz の平面直行座標系に変換したもの
            var min = geoReference.Project(extent.Min);
            var max = geoReference.Project(extent.Max);
            var centerPosTmp = new Vector3(
                (float)(min.X + max.X) / 2.0f,
                CenterPosY,
                (float)(min.Z + max.Z) / 2.0f);
            var sizeTmp = new Vector3(
                (float)Math.Abs(max.X - min.X),
                1,
                (float)Math.Abs(max.Z - min.Z));
            Init(centerPosTmp, sizeTmp, gridCode);
            GridCode = gridCode;
            
            ApplyStyle();
            CalculateAllRectVerts();

            // デフォルトの選択状態を設定
            ResetSelectedArea();
        }

        public virtual void ResetSelectedArea()
        {
            selectedAreaList = new List<bool>();
            for (var i = 0; i < divideNumRow * divideNumColumn; i++)
            {
                selectedAreaList.Add(false);
            }
        }

        public bool IsSelectedArea()
        {
            return selectedAreaList.Any(selected => selected);
        }

        protected void SetSelectArea(int selectAreaIndex, bool isSelect)
        {
            if (0 <= selectAreaIndex && selectAreaIndex < selectedAreaList.Count)
            {
                selectedAreaList[selectAreaIndex] = isSelect;
            }
        }

        protected virtual void ApplyStyle()
        {
            if (GridCode.IsNormalGmlLevel) // 通常のメッシュコード
            {
                LineWidth = LineWidthLevel3;
                BoxColor = BoxColorNormalLevel3;
                divideNumColumn = 4;
                divideNumRow = 4;
                return;
            }
            if (GridCode.IsSmallerThanNormalGml) // 小さいメッシュコード
            {
                LineWidth = LineWidthLevel4;
                BoxColor = BoxColorNormalLevel4;
                divideNumColumn = 2;
                divideNumRow = 2;
                return;
            }

            // 大きいメッシュコード
            LineWidth = LineWidthLevel2;
            BoxColor = BoxColorNormalLevel2;
            divideNumColumn = 4;
            divideNumRow = 4;
        }

        public List<string> GetSelectedGridIds()
        {
            List<string> strGridCodes = new();
            if (GridCode.IsNormalGmlLevel)
            {
                for (var col = 0; col < divideNumColumn; col++)
                {
                    for (var row = 0; row < divideNumRow; row++)
                    {
                        if (selectedAreaList[row + col * divideNumColumn])
                        {
                            if (this is StandardMapGizmoDrawer)
                            {
                                strGridCodes.Add($"{GridCode.StringCode}");
                            }
                            else
                            {
                                strGridCodes.Add($"{GridCode.StringCode}{SuffixMeshIds[row + col * divideNumColumn]}");
                            }
                        }
                    }
                }
            }
            else if (GridCode.IsSmallerThanNormalGml)
            {
                for (int i = 0; i < GridCodeDivideNum; i++)
                {
                    if (selectedAreaList[i])
                    {
                        if (this is StandardMapGizmoDrawer)
                        {
                            strGridCodes.Add($"{GridCode.StringCode}");
                        }
                        else
                        {
                            strGridCodes.Add($"{GridCode.StringCode}{(i+1).ToString()}");
                        }
                    }
                }
            }
            else
            {
                // 上の処理で十分
            }

            return strGridCodes;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 選択範囲を描画
        /// </summary>
        protected override void AdditionalGizmo()
        {
            var prevColor = Handles.color;
            Handles.color = HandleColor;

            // 範囲内を半透明の四角で塗りつぶします。
            var min = AreaMin;
            var max = AreaMax;

            // 追加でボックスの中を分割するラインを引きます。
            var xDiff = this.Size.x / divideNumColumn;
            var linePosUp = min + Vector3.right * xDiff;
            // 縦のライン
            for (int i = 0; i < divideNumColumn - 1; i++)
            {
                UnityEngine.Gizmos.DrawLine(linePosUp, linePosUp + Vector3.forward * this.Size.z);
                linePosUp += Vector3.right * xDiff;
            }

            // 横のライン
            var zDiff = this.Size.z / divideNumRow;
            var linePosLeft = min + Vector3.forward * zDiff;
            for (int i = 0; i < divideNumRow - 1; i++)
            {
                UnityEngine.Gizmos.DrawLine(linePosLeft, linePosLeft + Vector3.right * this.Size.x);
                linePosLeft += Vector3.forward * zDiff;
            }

            // エリア塗りつぶし
            allRectVerts.Where((rect, index) => selectedAreaList[index])
                .ToList()
                .ForEach(rect =>
                {
                    Handles.DrawSolidRectangleWithOutline(rect, SelectedFaceColor, TransparentColor);
                });
            
            Handles.color = prevColor;
        }
        
        /// <summary>
        /// グリッドコードを描画
        /// </summary>
        public override void DrawSceneGUI()
        {
            DrawGridCodeId();
        }
        
        private void DrawGridCodeId()
        {
            if (!GridCode.IsValid) return;
            var gridCodeScreenWidth = CalcGridCodeScreenWidth();

            // LODアイコンと被らないように、下:上=3:7 の位置にします。
            var textPosWorld = new Vector3(this.CenterPos.x , this.CenterPos.y, AreaMin.z * 0.7f + AreaMax.z * 0.3f);
            
            
            if(!GridCode.IsNormalGmlLevel && !GridCode.IsSmallerThanNormalGml && gridCodeScreenWidth >= 160 * EditorGUIUtility.pixelsPerPoint) // 2次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                var color = BoxColorNormalLevel2;
                DrawString(GridCode.StringCode, textPosWorld, color, ReturnFontSize());
            }
            else if(GridCode.IsNormalGmlLevel && gridCodeScreenWidth >= 80f) // 3次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                var color = BoxColorNormalLevel3;
                DrawString(GridCode.StringCode, textPosWorld , color, ReturnFontSizeBlue());
            }
        }

        static void DrawString(string text, Vector3 worldPos, Color color, int fontSize = 20)
        {
            float scaler = EditorGUIUtility.pixelsPerPoint;
            UnityEditor.Handles.BeginGUI();

            var view = UnityEditor.SceneView.currentDrawingSceneView;
            var camera = view.camera;
            fontSize /= (int)scaler;
            Vector3 screenPos = camera.WorldToScreenPoint(worldPos) / scaler;
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));

            GUIStyle style = new GUIStyle();
            style.fontSize = fontSize;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color;
            style.alignment = TextAnchor.UpperCenter;

            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height - size.y, size.x , size.y), text, style);
            UnityEditor.Handles.EndGUI();
        }

        private bool IsSelectable()
        {
            return GridCode.IsNormalGmlLevel || GridCode.IsSmallerThanNormalGml;
        }

        public void ToggleSelectArea(Vector2 mousePos, List<StandardMapGizmoDrawer> standardMapGridDrawers)
        {
            if (!IsSelectable()) return;
            
            var ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out var hit, RayCastMaxDistance))
            {
                if ((AreaMin.x <= hit.point.x && hit.point.x <= AreaMax.x && AreaMin.z <= hit.point.z && hit.point.z <= AreaMax.z) == false)
                    return;
                
                var rowIndex = GetRowIndex(AreaMin.x, AreaMax.x, divideNumRow, hit.point.x);
                var columnIndex = GetColumnIndex(AreaMin.z, AreaMax.z, divideNumColumn, hit.point.z);
                var selectAreaIndex = rowIndex + columnIndex * divideNumColumn;
                selectedAreaList[selectAreaIndex] = !selectedAreaList[selectAreaIndex];

                TrySelectStandardMapGrid(standardMapGridDrawers);
            }
        }

        public void SetSelectArea(Vector3 areaSelectionMin, Vector3 areaSelectionMax, bool selectValue, List<StandardMapGizmoDrawer> standardMapGridDrawers)
        {
            if (!IsSelectable()) return;
            
            var min = AreaMin;
            var max = AreaMax;
            var cellWidth = (max.x - min.x) / divideNumColumn;
            var cellHeight = (max.z - min.z) / divideNumRow;
            for (var col = 0; col < divideNumColumn; col++)
            {
                for (var row = 0; row < divideNumRow; row++)
                {
                    var rectMinX = min.x + cellWidth * row;
                    var rectMaxX = min.x + cellWidth * (row + 1);
                    var rectMinZ = min.z + cellHeight * col;
                    var rectMaxZ = min.z + cellHeight * (col + 1);
                    if (rectMaxX < areaSelectionMin.x || rectMinX > areaSelectionMax.x || rectMaxZ < areaSelectionMin.z || rectMinZ > areaSelectionMax.z)
                    {
                        continue;
                    }

                    selectedAreaList[row + col * divideNumColumn] = selectValue;
                    TrySelectStandardMapGrid(standardMapGridDrawers);
                }
            }
        }

        private void TrySelectStandardMapGrid(List<StandardMapGizmoDrawer> standardMapDrawers)
        {
            foreach (var standardMapDrawer in standardMapDrawers)
            {
                if (!IsBoxIntersectXZ(standardMapDrawer))
                {
                    continue;
                }
                
                // 分割した矩形の頂点から、国土基本図郭と交差しているかどうか取得
                var intersectedIndices = allRectVerts
                    .Select((rect, idx) => new { rect, idx })
                    .Where(x =>
                    {
                        var min = x.rect[0]; // 左下
                        var max = x.rect[2]; // 右上
                        return standardMapDrawer.IsBoxIntersectXZ(min, max);
                    })
                    .Select(x => x.idx)
                    .ToList();

                // 1つでも選択されていれば選択状態に
                if (intersectedIndices.Any(idx => selectedAreaList[idx]))
                {
                    standardMapDrawer.SetMeshCodeSelection(GridCode.StringCode, true);
                }

                // 全て未選択なら解除
                if (intersectedIndices.All(idx => !selectedAreaList[idx]))
                {
                    standardMapDrawer.SetMeshCodeSelection(GridCode.StringCode, false);
                }
            }
        }
        
        int ReturnFontSize()
        {
            return (int)(Mathf.Clamp(CalcGridCodeScreenWidth(), 15f, 30f));
        }
        int ReturnFontSizeBlue()
        {
            return (int)(Mathf.Clamp(8f+8f*(CalcGridCodeScreenWidth() - 45f)/100f, 8f, 16f));
        }

        private float CalcGridCodeScreenWidth()
        {
            var sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                throw new Exception("シーンビューがありません。");
            }
            var camera = sceneView.camera;
            if (camera == null)
            {
                throw new Exception("カメラがありません。");
            }
            var extent = GridCode.Extent;
            // geoReference is passed in constructor.
            var positionUpperLeft = this.geoReference.Project(new GeoCoordinate(extent.Max.Latitude, extent.Min.Longitude, 0)).ToUnityVector();
            var positionLowerRight = this.geoReference.Project(new GeoCoordinate(extent.Min.Latitude, extent.Max.Longitude, 0)).ToUnityVector();
            return (camera.WorldToScreenPoint(positionLowerRight) - camera.WorldToScreenPoint(positionUpperLeft)).x;
        }

        private void CalculateAllRectVerts()
        {
            var min = AreaMin;
            var max = AreaMax;
            var cellWidth = (max.x - min.x) / divideNumColumn;
            var cellHeight = (max.z - min.z) / divideNumRow;
            allRectVerts = new List<Vector3[]>();

            for (var col = 0; col < divideNumColumn; col++)
            {
                for (var row = 0; row < divideNumRow; row++)
                {
                    var rectVerts = new[]
                    {
                        new Vector3(min.x + cellWidth * row, min.y, min.z + cellHeight * col),
                        new Vector3(min.x + cellWidth * row, min.y, min.z + cellHeight + cellHeight * col),
                        new Vector3(min.x + cellWidth + cellWidth * row, min.y, min.z + cellHeight + cellHeight * col),
                        new Vector3(min.x + cellWidth + cellWidth * row, min.y, min.z + cellHeight * col)
                    };
                    allRectVerts.Add(rectVerts);
                }
            }
        }
#endif
    }
}
