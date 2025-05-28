using PLATEAU.DynamicTile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Rendering.VirtualTexturing;

namespace PLATEAU.CameraMovement
{
    /// <summary>
    /// Runtime中のカメラ位置を監視し、位置が変わったらPLATEAUTileManagerに通知するクラス。
    /// </summary>
    public class RuntimeCameraTracker
    {
        private static PLATEAUTileManager tileManageer;

        private static Vector3 lastPosition;

        public struct PostLateUpdateDelegete { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeInitialize()
        {
            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            Debug.Log("RuntimeCameraTracker Initialized (Runtime Camera Tracking Start))");

            lastPosition = Camera.main.transform.position;

            tileManageer.ClearAll();
            tileManageer.UpdateAssetByCameraPosition(lastPosition);

            var loopSystem = new PlayerLoopSystem
            {
                updateDelegate = CustomPostLateUpdate,
                type = typeof(PostLateUpdateDelegete),
            };

            var playerloop = PlayerLoop.GetDefaultPlayerLoop();

            for (var i = 0; i < playerloop.subSystemList.Length; i++)
            {
                if (playerloop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.PostLateUpdate))
                {
                    //playerloop.subSystemList[i] = new PlayerLoopSystem
                    //{
                    //    type = playerloop.subSystemList[i].type,
                    //    updateDelegate = playerloop.subSystemList[i].updateDelegate,
                    //    subSystemList = playerloop.subSystemList[i].subSystemList.Append(loopSystem).ToArray(), // subSystemListの末尾にmySystem追加
                    //    updateFunction = playerloop.subSystemList[i].updateFunction,
                    //    loopConditionFunction = playerloop.subSystemList[i].loopConditionFunction,
                    //};

                    var postLateUpdateSystem = playerloop.subSystemList[i];
                    var subSystem = new List<PlayerLoopSystem>(postLateUpdateSystem.subSystemList);
                    subSystem.Add(loopSystem);
                    postLateUpdateSystem.subSystemList = subSystem.ToArray();
                    playerloop.subSystemList[i] = postLateUpdateSystem;

                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(playerloop);
        }

        private static void CustomPostLateUpdate()
        {
            //Debug.Log("my update.");

            var targetCamera = Camera.main;

            if (targetCamera != null && tileManageer != null)
            {
                Vector3 currentPosition = targetCamera.transform.position;
                if (currentPosition != lastPosition)
                {
                    //Debug.Log($"MainCameraが移動しました！ 新しい位置: {currentPosition}");
                    lastPosition = currentPosition;
                    tileManageer.UpdateAssetByCameraPosition(currentPosition);
                }
            }
        }
    }
}