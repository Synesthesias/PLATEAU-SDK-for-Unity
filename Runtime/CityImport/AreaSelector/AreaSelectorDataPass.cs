﻿using System.Collections.Generic;
using System.Linq;
using PLATEAU.Dataset;
using PLATEAU.Native;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PLATEAU.CityImport.AreaSelector
{
    /// <summary>
    /// 範囲選択画面が終わったとき、元のシーンの <see cref="PLATEAUCityModelLoader"/> コンポーネントに
    /// 選択結果を渡します。
    /// </summary>
    internal static class AreaSelectorDataPass
    {
        // シーンをまたいで渡したいデータ
        private static string prevScenePath;
        private static IEnumerable<MeshCode> selectedMeshCodes;
        private static IAreaSelectResultReceiver areaSelectResultReceiver;
        private static PredefinedCityModelPackage availablePackageFlags;
        private static Extent extent;

#if UNITY_EDITOR
        public static void Exec(
            string prevScenePathArg, IEnumerable<MeshCode> selectedMeshCodesArg,
            IAreaSelectResultReceiver areaSelectResultReceiverArg, PredefinedCityModelPackage availablePackageFlagsArg,
            Extent selectedExtent, EditorWindow prevEditorWindow)
        {


            prevScenePath = prevScenePathArg;
            selectedMeshCodes = selectedMeshCodesArg;
            areaSelectResultReceiver = areaSelectResultReceiverArg;
            availablePackageFlags = availablePackageFlagsArg;
            extent = selectedExtent;
            
            EditorSceneManager.sceneOpened += OnBackToPrevScene;
            EditorSceneManager.OpenScene(prevScenePath);
            // MacOSだと、範囲選択のときに PlateauWindow が背面に回ってしまい見失いがちなので再表示します。
            prevEditorWindow.Focus();
        }
#endif

#if UNITY_EDITOR
        private static void OnBackToPrevScene(Scene scene, OpenSceneMode __)
        {
            EditorSceneManager.sceneOpened -= OnBackToPrevScene;
            SceneManager.SetActiveScene(scene);
            PassAreaSelectDataToBehaviour();
            EditorSceneManager.MarkSceneDirty(scene);
        }
#endif

        /// <summary>
        /// 戻った先のシーンで、範囲選択の結果を渡します。
        /// </summary>
        private static void PassAreaSelectDataToBehaviour()
        {
            var areaMeshCodes = selectedMeshCodes
                .Select(meshCode => meshCode.ToString())
                .ToArray();
            areaSelectResultReceiver.ReceiveResult(areaMeshCodes, extent, availablePackageFlags);
        }
    }
}
