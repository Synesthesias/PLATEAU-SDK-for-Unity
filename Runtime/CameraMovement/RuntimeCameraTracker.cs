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

            Debug.Log("�Q�[���J�n���Ɏ��s����܂����I");

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
                        //subSystemList = playerloop.subSystemList[i].subSystemList.Prepend(mySystem).ToArray(),      // subSystemList�̐擪��mySystem�ǉ�
                        subSystemList = playerloop.subSystemList[i].subSystemList.Append(mySystem).ToArray(), // subSystemList�̖�����mySystem�ǉ�
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
                    //Debug.Log($"MainCamera���ړ����܂����I �V�����ʒu: {currentPosition}");
                    lastPosition = currentPosition;

                    tileManageer.UpdateAssetByCameraPosition(currentPosition);
                }
            }
        }
    }
}