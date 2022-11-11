﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Udx;
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
    [ExecuteAlways]
    internal class AreaSelectorBehaviour : MonoBehaviour
    {
        [SerializeField] private string prevScenePath;
        [SerializeField] private string dataSourcePath;
        private AreaSelectGizmosDrawer gizmosDrawer;
        private IAreaSelectResultReceiver areaSelectResultReceiver;
        private PredefinedCityModelPackage availablePackageFlags;
        private int coordinateZoneID;
        private GeoReference geoReference;
        private bool prevSceneCameraRotationLocked;
        private GSIMapLoaderZoomSwitch mapLoader;
        

        public static bool IsAreaSelectEnabled { get; set; }

        public void Init(string prevScenePathArg, string dataSourcePathArg, IAreaSelectResultReceiver areaSelectResultReceiverArg, int coordinateZoneIDArg)
        {
            IsAreaSelectEnabled = true;
            this.prevScenePath = prevScenePathArg;
            this.dataSourcePath = dataSourcePathArg;
            this.areaSelectResultReceiver = areaSelectResultReceiverArg;
            this.coordinateZoneID = coordinateZoneIDArg;
            #if UNITY_EDITOR
            this.prevSceneCameraRotationLocked = SceneView.lastActiveSceneView.isRotationLocked;
            #endif
        }

        private void Start()
        {
            RotateSceneViewCameraDown();
            AreaSelectorGUI.Enable(this);
            // TODO タプルで戻るのは分かりにくいのでは
            var gatherResult = GatherMeshCodesInGMLDirectory(this.dataSourcePath);
            var drawerObj = new GameObject($"{nameof(AreaSelectGizmosDrawer)}");
            this.gizmosDrawer = drawerObj.AddComponent<AreaSelectGizmosDrawer>();
            this.gizmosDrawer.Init(gatherResult.meshCodes, this.coordinateZoneID, out this.geoReference);
            this.availablePackageFlags = gatherResult.availablePackageFlags;
            var entireExtent = CalcExtentCoversAllMeshCodes(gatherResult.meshCodes);
            this.mapLoader = new GSIMapLoaderZoomSwitch(this.geoReference, entireExtent);
        }

        private void Update()
        {
            // カメラを下に向けます。
            RotateSceneViewCameraDown();
            
            #if UNITY_EDITOR
            this.mapLoader.Update(SceneView.lastActiveSceneView.camera);
            #endif
        }

        private void OnRenderObject()
        {
            // [ExecuteAlways] を付けたクラスは Editモードでも Updateが呼ばれるとはいえ、
            // その呼び出し頻度は低い（何か変更しないと呼び出されない）ため、地図読込に悪影響があります。
            // そこで描画のたびに Update を回すようにします。
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            SceneView.lastActiveSceneView.isRotationLocked = this.prevSceneCameraRotationLocked;
#endif
            this.mapLoader.Dispose();
            Debug.Log("mapLoader is disposed.");
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
            if (meshCodes.Count <= 0)
            {
                Debug.LogError("No MeshCode found.");
            }

            return (meshCodes, availablePackageFlags);
        }

        

        private void EndAreaSelection()
        {
            AreaSelectorGUI.Disable();
            var areaSelectResult = this.gizmosDrawer.SelectedMeshCodes;
            var selectedExtent = this.gizmosDrawer.CursorExtent(this.coordinateZoneID, this.geoReference.ReferencePoint);
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
