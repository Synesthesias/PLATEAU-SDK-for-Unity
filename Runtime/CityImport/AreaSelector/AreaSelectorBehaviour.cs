using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityImport.AreaSelector.Display.Gizmos;
using PLATEAU.CityImport.AreaSelector.Display.Maps;
using PLATEAU.CityImport.AreaSelector.Display.Windows;
using PLATEAU.CityImport.Config;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.Util;
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
        private ConfigBeforeAreaSelect confBeforeAreaSelect;
        private AreaSelectGizmosDrawer gizmosDrawer;
        private IAreaSelectResultReceiver areaSelectResultReceiver;
        private GeoReference geoReference;
        private bool prevSceneCameraRotationLocked;
        private GSIMapLoaderZoomSwitch mapLoader;
        private Extent entireExtent;
        #if UNITY_EDITOR
        private EditorWindow prevEditorWindow;
        private GridCodeSearchWindow gridSearchWindow;
        #endif

        public static bool IsAreaSelectEnabled { get; set; }

#if UNITY_EDITOR
        public void Init(string prevScenePathArg, ConfigBeforeAreaSelect confBeforeAreaSelectArg, IAreaSelectResultReceiver areaSelectResultReceiverArg, EditorWindow prevEditorWindowArg)
        {
            IsAreaSelectEnabled = true;
            prevScenePath = prevScenePathArg;
            confBeforeAreaSelect = confBeforeAreaSelectArg;
            areaSelectResultReceiver = areaSelectResultReceiverArg;
            prevSceneCameraRotationLocked = SceneView.lastActiveSceneView.isRotationLocked;
            prevEditorWindow = prevEditorWindowArg;
        }
#endif

        private void Start()
        {
            AreaSelectorMenuWindow.Enable(this);
            AreaSelectorGuideWindow.Enable();
            LodLegendGUI.Enable(this);
            using var progressBar = new ProgressBar();
            progressBar.Display("データファイルを検索中です...", 0f);
            NativeVectorGridCode gridCodes;
            try
            {
                gridCodes = GatherGridCodes(this.confBeforeAreaSelect.DatasetSourceConfig);
            }
            catch (Exception e)
            {
                const string ErrorMessage = "範囲の取得に失敗しました。";
                Debug.LogError($"{ErrorMessage}\n{e}");
                Dialogue.Display(ErrorMessage, "OK");
                CancelAreaSelection();
                return;
            }

            if (gridCodes.Length == 0)
            {
                Dialogue.Display("PLATEAU", "該当のデータがありません。", "OK");
                
                CancelAreaSelection();
                return;
            }

            var drawerObj = new GameObject($"{nameof(AreaSelectGizmosDrawer)}");
            this.gizmosDrawer = drawerObj.AddComponent<AreaSelectGizmosDrawer>();
            this.gizmosDrawer.Init(gridCodes, this.confBeforeAreaSelect.DatasetSourceConfig, this.confBeforeAreaSelect.CoordinateZoneID, out this.geoReference);
            entireExtent = CalcExtentCoversAllGridCodes(gridCodes);
            this.mapLoader = new GSIMapLoaderZoomSwitch(this.geoReference, entireExtent);
            SetInitialCamera(entireExtent);
#if (UNITY_EDITOR && UNITY_2019_2_OR_NEWER)
            SceneVisibilityManager.instance.DisableAllPicking();
#endif
        }

        private void SetInitialCamera(Extent entireExtentArg)
        {
            #if UNITY_EDITOR
            RotateSceneViewCameraDown();
            var centerPos = this.geoReference.Project(entireExtentArg.Center).ToUnityVector();
            var initialCameraPos = new Vector3(centerPos.x, 0, centerPos.z);
            SceneView.lastActiveSceneView.pivot = initialCameraPos;
            // シーンビューのカメラが全体を映すようにします。
            SceneView.lastActiveSceneView.size = Mathf.Abs((float)(this.geoReference.Project(entireExtentArg.Max).Z - this.geoReference.Project(entireExtentArg.Min).Z) / 2f);
            #endif
        }

        private void Update()
        {
            // カメラを下に向けます。
            RotateSceneViewCameraDown();

#if UNITY_EDITOR
            var sceneView = SceneView.lastActiveSceneView;
            mapLoader.Update(sceneView.camera);
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
            if (gridSearchWindow != null)
            {
                gridSearchWindow.Close();
            }
#endif
            this.mapLoader?.Dispose();
        }

        private static Extent CalcExtentCoversAllGridCodes(NativeVectorGridCode gridCodes)
        {
            var entireMin = new GeoCoordinate(90, 180, 9999);
            var entireMax = new GeoCoordinate(-90, -180, -9999);
            var japanExtent = new Extent(new GeoCoordinate(20, 122, -9999), new GeoCoordinate(46, 154, 9999));
            for(int i=0; i<gridCodes.Length; i++)
            {
                using var gridCode = gridCodes.At(i); // 廃棄を明示
                if (!gridCode.IsValid) continue;
                
                // 日本の範囲外のデータは異常値として無視する
                var gridExtent = gridCode.Extent;
                var intersection = Extent.Intersection(gridExtent, japanExtent, true);
                if (intersection.Min.Latitude < -90 && intersection.Max.Latitude < -90) // intersectionが失敗値を返したとき
                {
                    continue; // 日本の範囲外のデータは無視する
                }
                
                var areaMin = gridCode.Extent.Min;
                var areaMax = gridCode.Extent.Max;
                entireMin = GeoCoordinate.Min(entireMin, areaMin);
                entireMax = GeoCoordinate.Max(entireMax, areaMax);
            }

            var entireExtent = new Extent(entireMin, entireMax);
            return entireExtent;
        }

        private static NativeVectorGridCode GatherGridCodes(IDatasetSourceConfig datasetSourceConfig)
        {
            using var datasetSource = DatasetSource.Create(datasetSourceConfig);
            using var accessor = datasetSource.Accessor;
            
            // グリッドコードをコピーして返します
            var dstGridCodes = NativeVectorGridCode.Create();
            var srcGridCodes = accessor.GridCodes;
            for (int i = 0; i < srcGridCodes.Length; i++)
            {
                using var gridCode = srcGridCodes.At(i);
                if (!gridCode.IsValid) continue;
                dstGridCodes.AddCopyOf(gridCode);
            }
            
            return dstGridCodes;
        }

        internal void ResetSelectedArea()
        {
            this.gizmosDrawer.ResetSelectedArea();
        }

        internal bool IsSelectedArea()
        {
            return this.gizmosDrawer.IsSelectedArea();
        }

        internal void EndAreaSelection()
        {
            IsAreaSelectEnabled = false;
            AreaSelectorMenuWindow.Disable();
            AreaSelectorGuideWindow.Disable();
            LodLegendGUI.Disable();
            var selectedGridCodes = this.gizmosDrawer.SelectedGridCodes;
            var areaSelectResult = new AreaSelectResult(confBeforeAreaSelect, selectedGridCodes, AreaSelectResult.ResultReason.Confirm);

            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            #if UNITY_EDITOR
            AreaSelectorDataPass.Exec(this.prevScenePath, areaSelectResult, this.areaSelectResultReceiver, this.prevEditorWindow);
            #endif
        }

        internal void CancelAreaSelection()
        {
            AreaSelectorMenuWindow.Disable();
            AreaSelectorGuideWindow.Disable();
            LodLegendGUI.Disable();
            IsAreaSelectEnabled = false;
            var emptyAreaSelectResult = new AreaSelectResult(confBeforeAreaSelect, GridCodeList.Empty, AreaSelectResult.ResultReason.Cancel);
            #if UNITY_EDITOR
            AreaSelectorDataPass.Exec(this.prevScenePath, emptyAreaSelectResult, this.areaSelectResultReceiver, this.prevEditorWindow);
            #endif
        }

        internal void SwitchLodIcon(int lod, bool isCheck)
        {
            gizmosDrawer.SwitchLodIcon(lod, isCheck);
        }

        private static void RotateSceneViewCameraDown()
        {
            #if UNITY_EDITOR
            SceneView.lastActiveSceneView.in2DMode = false; // 2Dモードだと上から見下ろすカメラにできないため
            var scene = SceneView.lastActiveSceneView;
            scene.isRotationLocked = true;
            scene.rotation = Quaternion.Euler(90, 0, 0);
            #endif
        }

        internal void ShowGridCodeSearchWindow()
        {
            #if UNITY_EDITOR
            gridSearchWindow = GridCodeSearchWindow.ShowWindow();
            gridSearchWindow.Init(this);
            #endif
        }

        internal bool SearchByGridCode(string code)
        {
            #if UNITY_EDITOR
            try
            {
                var gridCode = GridCode.Create(code);
                var extent = gridCode.Extent;
                var min = geoReference.Project(extent.Min);
                var max = geoReference.Project(extent.Max);
                var center = geoReference.Project(extent.Center);
                var centerPos = center.ToUnityVector();
                var initialCameraPos = new Vector3(centerPos.x, 0, centerPos.z);

                //範囲チェック
                var intersection = Extent.Intersection(extent, entireExtent, true);
                var failedCoord = new GeoCoordinate(-99, -99, -99);
                if (intersection.Min.Equals(failedCoord) && intersection.Max.Equals(failedCoord))
                {
                    return false;
                }

                SceneView.lastActiveSceneView.pivot = initialCameraPos;
                // シーンビューのカメラが全体を映すようにします。
                SceneView.lastActiveSceneView.size = Mathf.Abs((float)(max.Z - min.Z) / 2f);
            }
            catch(Exception e)
            {
                Debug.LogError($"code:{code} Error:{e.Message}");
                return false;
            }

            if (gridSearchWindow != null)
            {
                gridSearchWindow.Close();
            }
            gridSearchWindow = null;
            #endif
            return true;
        }

    }
}
