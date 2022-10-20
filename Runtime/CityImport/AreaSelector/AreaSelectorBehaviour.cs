using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector
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
        private IAreaSelectResultReceiver areaSelectResultReceiver;
        private PredefinedCityModelPackage availablePackageFlags;
        private int coordinateZoneID;
        private PlateauVector3d referencePoint;

        public void Init(string prevScenePathArg, string dataSourcePathArg, IAreaSelectResultReceiver areaSelectResultReceiverArg, int coordinateZoneIDArg)
        {
            this.prevScenePath = prevScenePathArg;
            this.dataSourcePath = dataSourcePathArg;
            this.areaSelectResultReceiver = areaSelectResultReceiverArg;
            this.coordinateZoneID = coordinateZoneIDArg;
        }

        private void Start()
        {
            AreaSelectorGUI.Enable(this);
            var photoLoadTask = GSIPhotoLoader.Load("seamlessphoto", 10, 909, 403, this.mapPlane, this.mapMaterials);
            photoLoadTask.ContinueWithErrorCatch();
            var gatherResult = GatherMeshCodesInGMLDirectory(this.dataSourcePath);
            PlaceMeshCodeDrawers(gatherResult.meshCodes, this.meshCodeDrawers, this.coordinateZoneID, out this.referencePoint);
            this.availablePackageFlags = gatherResult.availablePackageFlags;
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

        private static (ReadOnlyCollection<MeshCode> meshCodes, PredefinedCityModelPackage availablePackageFlags) GatherMeshCodesInGMLDirectory(string sourcePath)
        {
            EditorUtility.DisplayProgressBar("", "データファイルを検索中です...", 0f);
            Debug.Log(sourcePath);
            var collection = UdxFileCollection.Find(sourcePath);
            var availablePackageFlags = collection.Packages;
            var meshCodes = collection.MeshCodes;
            Debug.Log(DebugUtil.EnumerableToString(meshCodes));
            if (meshCodes.Count <= 0)
            {
                Debug.LogError("No MeshCode found.");
            }

            return (meshCodes, availablePackageFlags);
        }

        private static void PlaceMeshCodeDrawers(ReadOnlyCollection<MeshCode> meshCodes, ICollection<MeshCodeGizmoDrawer> boxGizmoDrawers, int coordinateZoneID, out PlateauVector3d referencePoint)
        {
            EditorUtility.DisplayProgressBar("", "範囲座標を計算中です...", 0.5f);
            // TODO geoReferenceの生成は1度で済むはず
            // 仮に (0,0,0) を referencePoint とする geoReference を作成
            using var geoReferenceTmp = CoordinatesConvertUtil.UnityStandardGeoReference(coordinateZoneID);
            // 中心を計算し、そこを基準点として geoReference を再設定
            referencePoint = new PlateauVector3d(0, 0, 0);
            foreach (var meshCode in meshCodes)
            {
                var min = geoReferenceTmp.Project(meshCode.Extent.Min);
                var max = geoReferenceTmp.Project(meshCode.Extent.Max);
                referencePoint = new PlateauVector3d((min.X + max.X)*0.5 + referencePoint.X, 0, (min.Z + max.Z)*0.5 + referencePoint.Z);
            }

            referencePoint =
                new PlateauVector3d(referencePoint.X / meshCodes.Count, 0, referencePoint.Z / meshCodes.Count);
            var geoReference = new GeoReference(referencePoint, 1f, CoordinateSystem.EUN, coordinateZoneID);
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
            var areaSelectResult = 
                this.cursor.SelectedMeshCodes(this.meshCodeDrawers)
                    .Select(drawer => drawer.MeshCode);
            var selectedExtent = this.cursor.GetExtent(this.coordinateZoneID, this.referencePoint);
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            AreaSelectorDataPass.Exec(this.prevScenePath, areaSelectResult, this.areaSelectResultReceiver, this.availablePackageFlags, selectedExtent);
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
