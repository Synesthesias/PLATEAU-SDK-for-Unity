using PLATEAU.DynamicTile;
using PLATEAU.Editor.Addressables;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルインポートについて、Addressableの設定を行います。
    /// </summary>
    internal class TileAddressableConfigMaker : IOnTileGenerateStart, IAfterTileAssetBuild
    {
        public readonly DynamicTileProcessingContext context;

        public TileAddressableConfigMaker(DynamicTileProcessingContext context)
        {
            this.context = context;
        }
        
        /// <summary>
        /// エクスポートの事前処理で、タイルのビルドに必要なAddressable設定を行います。
        /// </summary>
        public bool OnTileGenerateStart()
        {
            // プロファイルを作成
            var profileID = AddressablesUtility.SetOrCreateProfile();
            if (string.IsNullOrEmpty(profileID))
            {
                Debug.LogError("プロファイルの作成に失敗しました。");
                return false;
            }

            // ビルド設定を行います。
            AddressablesUtility.SetRemoteProfileSettings(context.BuildFolderPath, context.AddressableGroupName);
            AddressablesUtility.SetGroupLoadAndBuildPath(context.AddressableGroupName);
            
            return true;
        }

        /// <summary>
        /// タイルビルド後、Addressableの設定を削除し元に戻します。
        /// </summary>
        public bool AfterTileAssetBuild()
        {
            AddressablesUtility.BackToDefaultProfile();
            
            // ビルドが終わったらAddressableGroup設定はもう不要です
            AddressablesUtility.RemoveGroup(context.AddressableGroupName);
            return true;
        }
    }
}