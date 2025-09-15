using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
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
            AddressablesUtility.SaveAddressableSettings();
            
            return true;
        }

        /// <summary>
        /// タイルビルド後、Addressableの設定を元に戻します。
        /// </summary>
        public bool AfterTileAssetBuild()
        {
            AddressablesUtility.BackToDefaultProfile();
            Cleanup();
            return true;
        }

        /// <summary> 古いグループを削除します。 </summary>
        private void Cleanup()
        {
            var groupName = context.AddressableGroupName;
            if (!string.IsNullOrEmpty(groupName))
            {
                AddressablesUtility.RemoveGroup(groupName);
            }
        }
        
        public void OnTileGenerateStartFailed()
        {
            AddressablesUtility.BackToDefaultProfile();
        }
    }
}