using System;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using UnityEngine;
using UnityEditor;
using PLATEAU.Native;
using PLATEAU.Util;

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
        private GeoReference geoReference;

        /// <summary>
        /// メッシュコードに対応したギズモを表示するようにします。
        /// </summary>
        public void SetUp(MeshCode meshCode, GeoReference geoReference)
        {
            var extent = meshCode.Extent; // extent は緯度,経度,高さ

            this.geoReference = geoReference;

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
            Init(centerPosTmp, sizeTmp, meshCode);
            MeshCode = meshCode;
        }

        public void ApplyStyle()
        {
            switch (MeshCode.Level)
            {
                case 2:
                    LineWidth = lineWidthLevel2;
                    BoxColor = boxColorNormalLevel2;
                    break;
                default:
                    LineWidth = lineWidthLevel3;
                    BoxColor = boxColorNormalLevel3;
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

            DrawMeshCodeId();
#endif
        }

#if UNITY_EDITOR
        private void DrawMeshCodeId()
        {
            if (!MeshCode.IsValid) return;
            var meshCodeScreenWidth = CalcMeshCodeScreenWidth();

            // LODアイコンと被らないように、下:上=3:7 の位置にします。
            var textPosWorld = new Vector3(this.CenterPos.x , this.CenterPos.y, AreaMin.z * 0.7f + AreaMax.z * 0.3f);
            
            
            if(MeshCode.Level == 2 && meshCodeScreenWidth >= 160 * EditorGUIUtility.pixelsPerPoint) // 2次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                DrawString(MeshCode.ToString(), textPosWorld, boxColorNormalLevel2, ReturnFontSize());
            }
            else if(MeshCode.Level == 3 && meshCodeScreenWidth >= 80f) // 3次メッシュコードのとき、画面上の幅が大きいときだけ描画します。
            {
                    DrawString(MeshCode.ToString(), textPosWorld , boxColorNormalLevel3, ReturnFontSizeBlue());
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
