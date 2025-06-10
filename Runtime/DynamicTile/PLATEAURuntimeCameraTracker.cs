using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Runtime中のカメラ位置を監視し、位置が変わったらPLATEAUTileManagerに通知するクラス。
    /// PlayerLoopでUpdateをオーバーライドして使用
    /// PLATEAUTileManager側のStart,Update処理で代用することも可能
    /// </summary>
    public class PLATEAURuntimeCameraTracker
    {
        private static PLATEAUTileManager cachedTileManager;

        public struct CustomUpdateDelegete { }

        /// <summary>
        /// RuntimeCameraTrackerを初期化し、PLATEAUTileManagerを取得してカメラ位置の監視を開始する。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static async void OnRuntimeInitialize()
        {
            cachedTileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (cachedTileManager == null)
                return;

            try
            {
                // PLATEAUTileManagerの初期化を行う (PLATEAUTileManager側でStartメソッドで行うことも可能)
                cachedTileManager.ClearTileAssets();
                await cachedTileManager.InitializeTiles();
                cachedTileManager.UpdateAssetsByCameraPosition(cachedTileManager.LastCameraPosition);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize PLATEAURuntimeCameraTracker: {e.Message}");
                return;
            }

            Debug.Log("RuntimeCameraTracker Initialized (Runtime Camera Tracking Start)");

            var loopSystem = new PlayerLoopSystem
            {
                updateDelegate = CustomUpdate,
                type = typeof(CustomUpdateDelegete),
            };

            var playerloop = PlayerLoop.GetDefaultPlayerLoop();

            for (var i = 0; i < playerloop.subSystemList.Length; i++)
            {           
                if (playerloop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.PreUpdate))
                {
                    // PreUpdateの中にCustomUpdateを追加する
                    var currentUpdateSystem = playerloop.subSystemList[i];
                    var subSystem = new List<PlayerLoopSystem>(currentUpdateSystem.subSystemList);
                    subSystem.Add(loopSystem);
                    currentUpdateSystem.subSystemList = subSystem.ToArray();
                    playerloop.subSystemList[i] = currentUpdateSystem;
                    break;
                }
            }
            PlayerLoop.SetPlayerLoop(playerloop);
        }

        /// <summary>
        /// DefaultのPlayerLoopに戻し、Runtime中のカメラ位置の監視を停止する。
        /// PLATEAUEditorEventListenerでEditorのEventを監視
        /// </summary>
        public static void StopCameraTracking()
        {
            var defaultLoop = PlayerLoop.GetDefaultPlayerLoop();
            PlayerLoop.SetPlayerLoop(defaultLoop);
        }

        /// <summary>
        /// Runtime中のカメラ位置を監視し、位置が変わったらPLATEAUTileManagerに通知するメソッド。
        ///  (PLATEAUTileManager側でUpdateメソッドで行うことも可能)
        /// </summary>
        private static void CustomUpdate()
        {
            if (cachedTileManager == null)
                cachedTileManager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (cachedTileManager == null)
                return;

            var targetCamera = Camera.main;
            if (targetCamera != null)
            {
                Vector3 currentPosition = targetCamera.transform.position;
                if (currentPosition != cachedTileManager.LastCameraPosition)
                {
                    cachedTileManager.UpdateAssetsByCameraPosition(currentPosition);
                }
            }
        }
    }
}
