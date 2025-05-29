using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// Runtime中のカメラ位置を監視し、位置が変わったらPLATEAUTileManagerに通知するクラス。
    /// </summary>
    public class RuntimeCameraTracker
    {
        private static PLATEAUTileManager tileManageer;

        public struct CustomUpdateDelegete { }

        /// <summary>
        /// RuntimeCameraTrackerを初期化し、PLATEAUTileManagerを取得してカメラ位置の監視を開始する。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeInitialize()
        {
            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            Debug.Log("RuntimeCameraTracker Initialized (Runtime Camera Tracking Start))");

            tileManageer.ClearAll();
            tileManageer.UpdateAssetByCameraPosition(tileManageer.LastCameraPosition);

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
        /// Runtime中のカメラ位置を監視し、位置が変わったらPLATEAUTileManagerに通知するメソッド。
        /// </summary>
        private static void CustomUpdate()
        {
            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            var targetCamera = Camera.main;
            if (targetCamera != null && tileManageer != null)
            {
                Vector3 currentPosition = targetCamera.transform.position;
                if (currentPosition != tileManageer.LastCameraPosition)
                {
                    tileManageer.UpdateAssetByCameraPosition(currentPosition);
                }
            }
        }
    }
}