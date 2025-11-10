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
            // すでに同名のグループが存在する場合、一度削除します。
            // こうしないと、Assets内のタイルを更新したケースで更新が反映されなくなります。
            AddressablesUtility.RemoveGroup(context.AddressableGroupName);
            
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

        /// <summary> 古いグループを削除します。（Assets外に限り） </summary>
        private void Cleanup()
        {
            // ユーザーのAddressablesの設定をなるべく汚したくないので、
            // タイルの出力先がAssets外のときはGroup設定を削除します。
            // ロード時にはカタログパスから設定を読み直すので動く仕組みです。
            // ただし、タイルの出力先がAssets内のときは、タイルはビルドに含める必要があるので設定を残します。
            if (!context.IsExcludeAssetFolder) return;
            
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