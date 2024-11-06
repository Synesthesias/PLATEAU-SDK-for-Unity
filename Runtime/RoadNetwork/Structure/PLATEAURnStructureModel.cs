using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PLATEAU.RoadNetwork.Structure
{
    public class PLATEAURnStructureModel
        : MonoBehaviour
            , IRoadNetworkObject
            , ISerializationCallbackReceiver
    {
        // 勝手にシリアライズ/デシリアライズされると開発中だと困ることもあるのでフラグに出しておく
        [SerializeField]
        private bool autoSaving = true;

        // シリアライズ用フィールド
        [SerializeField]
        private RoadNetworkStorage storage;

        /// <summary>
        /// 道路構造
        /// </summary>
        public RnModel RoadNetwork { get; set; }

        /// <summary>
        /// RnModelをRoadNetworkStorageへのシリアライズ
        /// </summary>
        public void Serialize()
        {
            // 一応ランタイムでも呼べるようにUNITY_EDITORで切っていない
            if (RoadNetwork == null)
                return;
            var sw = new Stopwatch();
            sw.Start();
            storage = RoadNetwork.Serialize();
            sw.Stop();
            DebugEx.Log($"S{GetType().Name} Serialize({sw.ElapsedMilliseconds}[ms])");
        }

        /// <summary>
        /// RoadNetworkStorageからRnModelへのデシリアライズ
        /// </summary>
        public void Deserialize()
        {
            // 一応ランタイムでも呼べるようにUNITY_EDITORで切っていない
            var sw = new Stopwatch();
            sw.Start();
            RoadNetwork ??= new RnModel();
            RoadNetwork.Deserialize(storage);
            sw.Stop();
            DebugEx.Log($"S{GetType().Name} Deserialize({sw.ElapsedMilliseconds}[ms])");
        }

        /// <summary>
        /// 道路ネットワークのデータを設定するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataSetter GetRoadNetworkDataSetter()
        {
            return new RoadNetworkDataSetter(storage);
        }

        /// <summary>
        /// 道路ネットワークのデータを取得するためのクラスを取得する
        /// </summary>
        /// <returns></returns>
        public RoadNetworkDataGetter GetRoadNetworkDataGetter()
        {
            return new RoadNetworkDataGetter(storage);
        }

        public void OnBeforeSerialize()
        {
            // シリアライズ処理は異常な回数呼ばれるのでここで呼ぶのはまずいので何もしない
            // シーン保存時に行うようにしている.
            // https://discussions.unity.com/t/onbeforeserialize-constantly-called-in-editor-mode/115994
        }

        public void OnAfterDeserialize()
        {
            // Deserialize()は頻繁に呼ばれるわけではない & コンパイル後にでシリアライズされるように
            // OnAfterDeserializeでは呼ぶようにする
            if (!autoSaving)
                return;
            Deserialize();
        }

        [Conditional("UNITY_EDITOR")]
        public static void OnSceneSaving(Scene scene, string path)
        {
            // Scene内のPLATEAURnStructureModelをシリアライズチェックを行う
            foreach (var obj in scene.GetRootGameObjects())
            {
                foreach (var model in obj.GetComponentsInChildren<PLATEAURnStructureModel>())
                {
                    if (!model.autoSaving)
                        continue;
                    model.Serialize();
                }
            }
        }
    }
}