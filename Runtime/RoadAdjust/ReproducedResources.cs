using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadAdjust
{
    /// <summary>
    /// 道路の再生成で作ったメッシュとマテリアルを、差し替え・削除のときに破棄するための記録です。
    /// Playモード中に作ったものだけを記録し、破棄するのも記録にあるものだけです。
    /// 読み込んだアセットや、編集時に作ってシーンに保存されたものは記録しないため、破棄しません。
    /// </summary>
    internal static class ReproducedResources
    {
        /// <summary> 破棄済みのものを取り除く目安の件数です。 </summary>
        private const int PruneThreshold = 4096;

        private static readonly HashSet<Object> owned = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            owned.Clear();
        }

        /// <summary> Playモード中なら、再生成で作ったものとして記録します。 </summary>
        public static T Track<T>(T obj) where T : Object
        {
            if (obj == null || !Application.isPlaying) return obj;
            if (owned.Count >= PruneThreshold)
            {
                // シーンの読み込み直しなどで破棄されたものが残り続けないようにします。
                owned.RemoveWhere(o => o == null);
            }
            owned.Add(obj);
            return obj;
        }

        /// <summary> 記録にあるものなら破棄します。 </summary>
        public static void Release(Object obj)
        {
            if (obj == null || !owned.Remove(obj)) return;
            Object.Destroy(obj);
        }

        /// <summary> ゲームオブジェクトが使っているメッシュとマテリアルのうち、記録にあるものを破棄します。ゲームオブジェクト自体は破棄しません。 </summary>
        public static void ReleaseRendererResources(GameObject obj)
        {
            if (obj == null) return;

            var filter = obj.GetComponent<MeshFilter>();
            if (filter != null) Release(filter.sharedMesh);

            var meshCollider = obj.GetComponent<MeshCollider>();
            if (meshCollider != null) Release(meshCollider.sharedMesh);

            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;
            foreach (var material in renderer.sharedMaterials)
            {
                Release(material);
            }
        }

        /// <summary> 結合前のメッシュなど、生成の途中で作って使い終わったものを破棄します。 </summary>
        public static void DestroyTemporary(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
