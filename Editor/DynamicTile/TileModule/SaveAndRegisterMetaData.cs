using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// メタデータを保存し、Addressableとして登録します。
    /// </summary>
    public class SaveAndRegisterMetaData : IBeforeTileAssetBuild
    {
        private DynamicTileProcessingContext context;

        public SaveAndRegisterMetaData(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        public bool BeforeTileAssetBuild()
        {
            var metaStore = context.MetaStore;
            var groupName = context.AddressableGroupName;
            
            if (metaStore == null)
            {
                Debug.LogWarning("メタデータがnullです。");
                return false;
            }
            
            string dataPath = context.DataPath;
            // Path.Combineは環境によってバックスラッシュを使うため、フォワードスラッシュに統一
            dataPath = dataPath.Replace('\\', '/');

            // 既存アセットとの衝突を回避
            // dataPath = AssetDatabase.GenerateUniqueAssetPath(dataPath);

            // 既に存在する場合は新規作成を行わない（前と同じフォルダに追加で生成するケースが該当）
            var existing = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(metaStore, dataPath);
                AssetDatabase.SaveAssets();
            }

            // メタデータをAddressableに登録
            AddressablesUtility.RegisterAssetAsAddressable(
                dataPath,
                context.AddressName,
                groupName,
                new List<string> { DynamicTileExporter.AddressableLabel });
            return true;
        }
    }
}