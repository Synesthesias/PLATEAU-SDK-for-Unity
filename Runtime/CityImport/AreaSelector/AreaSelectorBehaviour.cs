using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityImport.AreaSelector
{
    /// <summary>
    /// 範囲選択画面の進行を担当するコンポーネントです。
    /// 別途 AreaSelectorBehaviourEditor も参照してください。
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
        private GeoReference geoReference;
        private CancellationTokenSource mapLoadCancel;

        public static bool IsAreaSelectEnabled { get; set; }

        public void Init(string prevScenePathArg, string dataSourcePathArg, IAreaSelectResultReceiver areaSelectResultReceiverArg, int coordinateZoneIDArg)
        {
            IsAreaSelectEnabled = true;
            this.prevScenePath = prevScenePathArg;
            this.dataSourcePath = dataSourcePathArg;
            this.areaSelectResultReceiver = areaSelectResultReceiverArg;
            this.coordinateZoneID = coordinateZoneIDArg;
        }

        private void Start()
        {
            RotateSceneViewCameraDown();
            AreaSelectorGUI.Enable(this);
            // TODO タプルで戻るのは分かりにくいのでは
            var gatherResult = GatherMeshCodesInGMLDirectory(this.dataSourcePath);
            PlaceMeshCodeDrawers(gatherResult.meshCodes, this.meshCodeDrawers, this.coordinateZoneID, out this.geoReference);
            this.availablePackageFlags = gatherResult.availablePackageFlags;
            var entireExtent = CalcExtentCoversAllMeshCodes(gatherResult.meshCodes);
            this.mapLoadCancel = new CancellationTokenSource();
            GSIMapLoader
                .DownloadAndPlaceAsync(entireExtent, this.geoReference, this.mapLoadCancel.Token)
                .ContinueWithErrorCatch();
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
            
            // カメラを下に向けます。
            RotateSceneViewCameraDown();
        }

        private void OnDisable()
        {
            this.mapLoadCancel.Cancel();
        }

        private static Extent CalcExtentCoversAllMeshCodes(IEnumerable<MeshCode> meshCodes)
        {
            var entireMin = new GeoCoordinate(90, 180, 9999);
            var entireMax = new GeoCoordinate(-90, -180, -9999);
            foreach (var meshCode in meshCodes)
            {
                var areaMin = meshCode.Extent.Min;
                var areaMax = meshCode.Extent.Max;
                entireMin = GeoCoordinate.Min(entireMin, areaMin);
                entireMax = GeoCoordinate.Max(entireMax, areaMax);
            }

            var entireExtent = new Extent(entireMin, entireMax);
            return entireExtent;
        }

        private static (ReadOnlyCollection<MeshCode> meshCodes, PredefinedCityModelPackage availablePackageFlags) GatherMeshCodesInGMLDirectory(string sourcePath)
        {
            #if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("", "データファイルを検索中です...", 0f);
            #endif
            Debug.Log(sourcePath);
            var collection = UdxFileCollection.Find(sourcePath);
            var availablePackageFlags = collection.Packages;
            var meshCodes = collection.MeshCodes;
            Debug.Log($"Area Mesh Code Selected : \n" + DebugUtil.EnumerableToString(meshCodes));
            if (meshCodes.Count <= 0)
            {
                Debug.LogError("No MeshCode found.");
            }

            return (meshCodes, availablePackageFlags);
        }

        private static void PlaceMeshCodeDrawers(
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
                var gizmoObj = new GameObject($"MeshCodeGizmo{meshCode.ToString()}");
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
                var gizmoTrans = gizmoObj.transform;
                gizmoTrans.position = center;
                gizmoTrans.localScale = size;
                gizmoTrans.parent = gizmoParent;
                var drawer = gizmoObj.AddComponent<MeshCodeGizmoDrawer>();
                drawer.MeshCode = meshCode;
                boxGizmoDrawers.Add(drawer);
            }
            #if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
            #endif
        }

        private void EndAreaSelection()
        {
            AreaSelectorGUI.Disable();
            DestroyMaterials();
            var areaSelectResult = 
                this.cursor.SelectedMeshCodes(this.meshCodeDrawers)
                    .Select(drawer => drawer.MeshCode);
            var selectedExtent = this.cursor.GetExtent(this.coordinateZoneID, this.geoReference.ReferencePoint);
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            AreaSelectorDataPass.Exec(this.prevScenePath, areaSelectResult, this.areaSelectResultReceiver, this.availablePackageFlags, selectedExtent);
        }

        public void OnSelectButtonPushed()
        {
            IsAreaSelectEnabled = false;
            EndAreaSelection();
        }
        
        public void OnCancelButtonPushed()
        {
            IsAreaSelectEnabled = false;
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

        private void RotateSceneViewCameraDown()
        {
            #if UNITY_EDITOR
            var scene = SceneView.lastActiveSceneView;
            scene.isRotationLocked = true;
            scene.rotation = Quaternion.Euler(90, 0, 0);
            #endif
        }
    }
}
