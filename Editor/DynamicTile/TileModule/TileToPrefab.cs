using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルをプレハブにします。
    /// タイルを変更して再出力する過程で使います。
    /// </summary>
    public class TileToPrefab : IOnTileGenerateStart
    {
        private PLATEAUTileManager manager;
        private string prefabDirPath;

        public TileToPrefab(PLATEAUTileManager manager, string prefabDirPath)
        {
            this.manager = manager;
            this.prefabDirPath = prefabDirPath;
        }
        
        public bool OnTileGenerateStart()
        {
            var model = manager.GetComponentInChildren<PLATEAUInstancedCityModel>();
            if (model == null)
            {
                Debug.LogError("タイルの親に相当するゲームオブジェクトが見つかりません。");
                return false;
            }
            var tileParent = model.transform;
            AssetPathUtil.CreateDirectoryIfNotExist(prefabDirPath);

            // タイルごとにプレハブ化します。
            foreach (Transform tile in tileParent)
            {
                var tileObj = tile.gameObject;
                var prevFlag = tileObj.hideFlags;
                tileObj.hideFlags = HideFlags.None; // 保存のために一時的にHideFlagsをオフに
                try
                {
                    var prefabPath = Path.Combine(prefabDirPath, tileObj.name).Replace('\\', '/') + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(tileObj, prefabPath);
                }
                finally
                {
                    tileObj.hideFlags = prevFlag;
                }
            }

            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }
    }
}