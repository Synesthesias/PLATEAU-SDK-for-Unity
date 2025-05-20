using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// DynamicTileオブジェクトに付与し、ダウンサンプルレベルごとのAddress情報を保持するコンポーネント
    /// </summary>
    public class PLATEAUDynamicTile : MonoBehaviour
    {
        [SerializeField]
        private string originalAddress;
        public string OriginalAddress
        {
            get => originalAddress;
            set => originalAddress = value;
        }

        /// <summary>
        /// 指定したダウンサンプルレベルのAddressを取得
        /// </summary>
        public string GetAddress(int downSampleLevel)
        {
            return originalAddress + "_down_" + downSampleLevel;
        }

        /// <summary>
        /// オリジナル（downSampleLevel=0）のアドレスを返す。
        /// </summary>
        public string GetOriginalAddress()
        {
            return GetAddress(0);
        }

       [ContextMenu("DynamicTile/Load Tile")]
        public void LoadTile()
        {
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager != null)
            {
                manager.Load(this, 0); // ここで tile を渡す
                Debug.Log($"{this.name} のタイルをロードしました");
            }
            else
            {
                Debug.LogWarning("PLATEAUTileManagerがシーンに見つかりません");
            }
        }

        [ContextMenu("DynamicTile/Unload Tile")]
        public void UnloadTile()
        {
            var manager = GameObject.FindObjectOfType<PLATEAUTileManager>();
            if (manager != null)
            {
                manager.Unload(this, 0); // ここで tile を渡す
                Debug.Log($"{this.name} のタイルをアンロードしました");
            }
            else
            {
                Debug.LogWarning("PLATEAUTileManagerがシーンに見つかりません");
            }
        }
    }
} 