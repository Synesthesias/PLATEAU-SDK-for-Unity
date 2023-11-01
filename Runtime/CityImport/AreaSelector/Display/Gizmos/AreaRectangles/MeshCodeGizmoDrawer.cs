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
    /// <see cref="MeshCode"/> に応じた四角形のギズモを表示します。
    /// </summary>
    internal class MeshCodeGizmoDrawer : BoxGizmoDrawer
    {
        
        private int divideNumColumn;
        private int divideNumRow;
        private const int LineWidthLevel2 = 3;
        private const int LineWidthLevel3 = 2;
        private const int LineWidthLevel4 = 1;
        private const float CenterPosY = 15f;
        private const float RayCastMaxDistance = 100000.0f;
        private static readonly Color BoxColorNormalLevel2 = Color.black;
        private static readonly Color BoxColorNormalLevel3 = new(0f, 84f / 255f, 1f);
        private static readonly Color BoxColorNormalLevel4 = new(0f, 84f / 255f, 1f);
        private static readonly Color HandleColor = new(1f, 72f / 255f, 0f);
        private static readonly Color SelectedFaceColor = new(1f, 204f / 255f, 153f / 255f, 0.5f);
        private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);
        private static readonly List<string> SuffixMeshIds = new()
        {
            "11", "12", "21", "22",
            "13", "14", "23", "24",
            "31", "32", "41", "42",
            "33", "34", "43", "44",
        };
        
        public MeshCode MeshCode { get; private set; }
        private GeoReference geoReference;
        private List<bool> selectedAreaList;
        
        
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
        public void SetUp(MeshCode meshCode, GeoReference geoReferenceArg)
        {
            var extent = meshCode.Extent; // extent は緯度,経度,高さ

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
            Init(centerPosTmp, sizeTmp, meshCode);
            MeshCode = meshCode;
            
            ApplyStyle();

            // デフォルトの選択状態を設定
            ResetSelectedArea();
            
        }

        public void ResetSelectedArea()
        {
            selectedAreaList = new List<bool>();
            for (var i = 0; i < divideNumRow * divideNumColumn; i++)
            {
                selectedAreaList.Add(false);
            }
        }

        public bool IsSelectedArea()
        {
            return 0 < selectedAreaList.Where(selectedArea => selectedArea).ToList().Count;
        }
        
        private void ApplyStyle()
        {
            switch (MeshCode.Level)
            {
                case 2:
                    LineWidth = LineWidthLevel2;
                    BoxColor = BoxColorNormalLevel2;
                    divideNumColumn = 4;
                    divideNumRow = 4;
                    break;
                case 3:
                    LineWidth = LineWidthLevel3;
                    BoxColor = BoxColorNormalLevel3;
                    divideNumColumn = 4;
                    divideNumRow = 4;
                    break;
                default:
                    LineWidth = LineWidthLevel4;
                    BoxColor = BoxColorNormalLevel4;
                    divideNumColumn = 2;
                    divideNumRow = 2;
                    break;
            }
        }

        public List<string> GetSelectedMeshIds()
        {
            List<string> meshIds = new();
            switch (MeshCode.Level)
            {
                case 3:
                    for (var col = 0; col < divideNumColumn; col++)
                    {
                        for (var row = 0; row < divideNumRow; row++)
                        {
                            if (selectedAreaList[row + col * divideNumColumn])
                            {
                                meshIds.Add($"{MeshCode.ToString()}{SuffixMeshIds[row + col * divideNumColumn]}");
                            }
                        }
                    }
                    break;
                case 4:
                    for (int i = 0; i < 4; i++)
                    {
                        if (selectedAreaList[i])
                        {
                            meshIds.Add($"{MeshCode.ToString()}{(i+1).ToString()}");
                        }
                    }
                    break;
                case 2:
                    // 上の処理で十分
                    break;
                default:
                    Debug.LogError($"Not implemented for this mesh code level {MeshCode.Level}");
                    break;
            }
            

            return meshIds;
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
            var cellWidth = (max.x - min.x) / divideNumColumn;
            var cellHeight = (max.z - min.z) / divideNumRow;
            for (var col = 0; col < divideNumColumn; col++)
            {
                for (var row = 0; row < divideNumRow; row++)
                {
                    if (selectedAreaList[row + col * divideNumColumn])
                    {
                        var rectVerts = new[]
                        {
                            new Vector3(min.x + cellWidth * row, min.y, min.z + cellHeight * col),
                            new Vector3(min.x + cellWidth * row, min.y, min.z + cellHeight + cellHeight * col),
                            new Vector3(min.x + cellWidth + cellWidth * row, min.y, min.z + cellHeight + cellHeight * col),
                            new Vector3(min.x + cellWidth + cellWidth * row, min.y, min.z + cellHeight * col)
                        };

                        // 四角の中を塗りつぶしますが、輪郭線は完全に透明にします。なぜなら、四角の輪郭線の表示は親クラスが行うためです。
                        Handles.DrawSolidRectangleWithOutline(rectVerts, SelectedFaceColor, TransparentColor);
                    }
                }
            }
            
            Handles.color = prevColor;
        }
        
        /// <summary>
        /// メッシュコードを描画
        /// </summary>
        public override void DrawSceneGUI()
        {
            DrawMeshCodeId();
        }
        
        private void DrawMeshCodeId()
        {
            if (!MeshCode.IsValid) return;
            var meshCodeScreenWidth = CalcMeshCodeScreenWidth();

            // LODアイコンと被らないように、下:上=3:7 の位置にします。
            var textPosWorld = new Vector3(this.CenterPos.x , this.CenterPos.y, AreaMin.z * 0.7f + AreaMax.z * 0.3f);
            
            
            if(MeshCode.Level == 2 && meshCodeScreenWidth >= 160 * EditorGUIUtility.pixelsPerPoint) // 2次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                DrawString(MeshCode.ToString(), textPosWorld, BoxColorNormalLevel2, ReturnFontSize());
            }
            else if(MeshCode.Level == 3 && meshCodeScreenWidth >= 80f) // 3次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                DrawString(MeshCode.ToString(), textPosWorld , BoxColorNormalLevel3, ReturnFontSizeBlue());
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
            if (MeshCode.Level <= 2) return false;
            return true;
        }

        public void ToggleSelectArea(Vector2 mousePos)
        {
            if (!IsSelectable()) return;
            
            var ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out var hit, RayCastMaxDistance))
            {
                if ((AreaMin.x <= hit.point.x && hit.point.x <= AreaMax.x && AreaMin.z <= hit.point.z && hit.point.z <= AreaMax.z) == false)
                    return;
                
                var rowIndex = GetRowIndex(AreaMin.x, AreaMax.x, divideNumRow, hit.point.x);
                var columnIndex = GetColumnIndex(AreaMin.z, AreaMax.z, divideNumColumn, hit.point.z);
                // FIXME 同じ式の繰り返し
                selectedAreaList[rowIndex + columnIndex * divideNumColumn] = !selectedAreaList[rowIndex + columnIndex * divideNumColumn];
            }
        }

        public void SetSelectArea(Vector3 areaSelectionMin, Vector3 areaSelectionMax, bool selectValue)
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
                }
            }
        }
        
        int ReturnFontSize()
        {
            return (int)(Mathf.Clamp(CalcMeshCodeScreenWidth(), 15f, 30f));
        }
        int ReturnFontSizeBlue()
        {
            return (int)(Mathf.Clamp(8f+8f*(CalcMeshCodeScreenWidth() - 45f)/100f, 8f, 16f));
        }

        private float CalcMeshCodeScreenWidth()
        {
            var camera = SceneView.currentDrawingSceneView.camera;
            var extent = MeshCode.Extent;
            // geoReference is passed in constructor.
            var positionUpperLeft = this.geoReference.Project(new GeoCoordinate(extent.Max.Latitude, extent.Min.Longitude, 0)).ToUnityVector();
            var positionLowerRight = this.geoReference.Project(new GeoCoordinate(extent.Min.Latitude, extent.Max.Longitude, 0)).ToUnityVector();
            return (camera.WorldToScreenPoint(positionLowerRight) - camera.WorldToScreenPoint(positionUpperLeft)).x;
        }
#endif
    }
}
