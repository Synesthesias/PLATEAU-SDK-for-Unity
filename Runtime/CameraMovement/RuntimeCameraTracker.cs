using PLATEAU.DynamicTile;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.VirtualTexturing;

namespace PLATEAU.CameraMovement
{

    public class RuntimeCameraTracker : MonoBehaviour
    {
        private static PLATEAUTileManager tileManageer;

        private static Vector3 lastPosition;

        public struct MyUpdate { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeInitialize()
        {
            //Disabled
            //return;

            tileManageer = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (tileManageer == null)
                return;

            Debug.Log("ゲーム開始時に実行されました！");

            lastPosition = Camera.main.transform.position;

            tileManageer.ClearAll();
            tileManageer.UpdateAssetByCameraPosition(lastPosition);

            var mySystem = new PlayerLoopSystem
            {
                type = typeof(MyUpdate),
                updateDelegate = CustomUpdate,
            };

            var playerloop = PlayerLoop.GetDefaultPlayerLoop();

            for (var i = 0; i < playerloop.subSystemList.Length; i++)
            {
                if (playerloop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.PostLateUpdate))
                {
                    playerloop.subSystemList[i] = new PlayerLoopSystem
                    {
                        type = playerloop.subSystemList[i].type,
                        updateDelegate = playerloop.subSystemList[i].updateDelegate,
                        //subSystemList = playerloop.subSystemList[i].subSystemList.Prepend(mySystem).ToArray(),      // subSystemListの先頭にmySystem追加
                        subSystemList = playerloop.subSystemList[i].subSystemList.Append(mySystem).ToArray(), // subSystemListの末尾にmySystem追加
                        updateFunction = playerloop.subSystemList[i].updateFunction,
                        loopConditionFunction = playerloop.subSystemList[i].loopConditionFunction,
                    };
                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(playerloop);
        }

        private static void CustomUpdate()
        {
            //Debug.Log("my update.");

            if (Camera.main != null && tileManageer != null)
            {
                Vector3 currentPosition = Camera.main.transform.position;
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