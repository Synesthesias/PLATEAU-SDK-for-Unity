using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PLATEAU.Geom;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityLoader.AreaSelector
{
    /// <summary>
    /// 範囲選択画面の進行を担当するコンポーネントです。
    /// </summary>
    [ExecuteInEditMode]
    internal class AreaSelectorBehaviour : MonoBehaviour
    {
        [SerializeField] private MeshRenderer mapPlane;
        [SerializeField] private string prevScenePath;
        [SerializeField] private string dataSourcePath;
        private readonly List<Material> mapMaterials = new List<Material>();
        [SerializeField] private AreaSelectorCursor cursor;
        private List<MeshCodeGizmoDrawer> meshCodeDrawers = new List<MeshCodeGizmoDrawer>();
        private GlobalObjectId loaderBehaviourID;

        public void Init(string prevScenePathArg, string dataSourcePathArg, GlobalObjectId loaderBehaviourIDArg)
        {
            this.prevScenePath = prevScenePathArg;
            this.dataSourcePath = dataSourcePathArg;
            this.loaderBehaviourID = loaderBehaviourIDArg;
        }

        private void Start()
        {
            AreaSelectorGUI.Enable(this);
            var photoLoadTask = GSIPhotoLoader.Load("seamlessphoto", 10, 909, 403, this.mapPlane, this.mapMaterials);
            photoLoadTask.ContinueWithErrorCatch();
            PlaceMeshCodeDrawers(this.dataSourcePath, this.meshCodeDrawers);
        }

        private void Update()
        {
            // カーソルの選択範囲に応じてメッシュコードのギズモの色を変えます。
            if (this.cursor == null)
            {
                Debug.LogError($"{nameof(AreaSelectorCursor)} is null.");
                return;
            }
            foreach (var box in this.meshCodeDrawers) box.BoxColor = Color.green;
            var selected = this.cursor.SelectedMeshCodes(this.meshCodeDrawers);
            foreach (var select in selected) select.BoxColor = Color.yellow;
        }

        private static void PlaceMeshCodeDrawers(string sourcePath, List<MeshCodeGizmoDrawer> boxGizmoDrawers)
        {
            EditorUtility.DisplayProgressBar("", "データファイルを検索中です...", 0f);
            var udx = UdxFileCollection.Find(sourcePath);
            var meshCodes = udx.MeshCodes;
            Debug.Log(DebugUtil.EnumerableToString(meshCodes));
            if (udx.MeshCodes.Count <= 0)
            {
                Debug.LogError("No MeshCode found.");
                return;
            }
            EditorUtility.DisplayProgressBar("", "範囲座標を計算中です...", 0.5f);
            // TODO geoReferenceの生成は1度で済むはず
            // 仮に (0,0,0) を referencePoint とする geoReference を作成
            using var geoReferenceTmp = new GeoReference(new PlateauVector3d(0, 0, 0), 1.0f, CoordinateSystem.EUN, 9);
            // 中心を計算し、そこを基準点として geoReference を再設定
            var referencePoint = new PlateauVector3d(0, 0, 0);
            foreach (var meshCode in meshCodes)
            {
                var min = geoReferenceTmp.Project(meshCode.Extent.Min);
                var max = geoReferenceTmp.Project(meshCode.Extent.Max);
                referencePoint = new PlateauVector3d((min.X + max.X)*0.5 + referencePoint.X, 0, (min.Z + max.Z)*0.5 + referencePoint.Z);
            }

            referencePoint =
                new PlateauVector3d(referencePoint.X / meshCodes.Count, 0, referencePoint.Z / meshCodes.Count);
            var geoReference = new GeoReference(referencePoint, 1f, CoordinateSystem.EUN, 9);
            foreach (var meshCode in meshCodes)
            {
                var gizmoObj = new GameObject($"MeshCodeGizmo");
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
                gizmoObj.transform.position = center;
                gizmoObj.transform.localScale = size;
                var drawer = gizmoObj.AddComponent<MeshCodeGizmoDrawer>();
                drawer.MeshCode = meshCode;
                boxGizmoDrawers.Add(drawer);
            }
            EditorUtility.ClearProgressBar();
        }

        private void EndAreaSelection()
        {
            AreaSelectorGUI.Disable();
            DestroyMaterials();
            var testAreaSelectResult = 
                this.cursor.SelectedMeshCodes(this.meshCodeDrawers)
                    .Select(drawer => drawer.MeshCode);
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            AreaSelectorDataPass.Exec(this.prevScenePath, testAreaSelectResult, this.dataSourcePath, this.loaderBehaviourID);
        }

        public void OnSelectButtonPushed()
        {
            EndAreaSelection();
        }
        
        public void OnCancelButtonPushed()
        {
            // TODO キャンセルになってない
            EndAreaSelection();
        }

        private void DestroyMaterials()
        {
            foreach (var mat in this.mapMaterials)
            {
                DestroyImmediate(mat);
            }
        }
    }
}
