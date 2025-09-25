using PLATEAU.DynamicTile;
using PLATEAU.Util;
using UnityEngine;
using UnityEditor;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// <see cref="PLATEAUEditingTile"/>に対して行われた変更をプレハブに適用します。
    /// </summary>
    public class ApplyEditingTilesToPrefabs : IOnTileGenerateStart
    {
        private DynamicTileProcessingContext context;
        
        public ApplyEditingTilesToPrefabs(DynamicTileProcessingContext context)
        {
            this.context = context;
        }


        public bool OnTileGenerateStart()
        {
			var tiles = Object.FindObjectsByType<PLATEAUEditingTile>(FindObjectsSortMode.None);
			if (tiles == null || tiles.Length == 0)
			{
				return true; // 適用対象が無ければ成功扱い
			}

			try
			{
				foreach (var editingTile in tiles)
				{
					if (editingTile == null) continue;
					var go = editingTile.gameObject;

					// 近傍のPrefabインスタンスルートを取得
					var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(go);
					if (instanceRoot == null) instanceRoot = go;

					// プレハブインスタンスでなければスキップ
					if (!PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
						continue;

					// 変更をプレハブに適用
					PrefabUtility.ApplyPrefabInstance(instanceRoot, InteractionMode.AutomatedAction);

					// 念のためプレハブアセットをDirtyにして保存
					var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
					if (!string.IsNullOrEmpty(prefabPath))
					{
						var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
						if (prefabAsset != null)
						{
							EditorUtility.SetDirty(prefabAsset);
						}
					}
                }

				AssetDatabase.SaveAssets();
				return true;
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to apply editing tiles to prefabs: {e.Message}\n{e.StackTrace}");
				return false;
			}
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }
    }
}