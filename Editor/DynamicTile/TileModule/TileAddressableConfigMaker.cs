using PLATEAU.DynamicTile;
using PLATEAU.Editor.TileAddressables;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 動的タイルインポートについて、Addressableの設定を行います。
    /// </summary>
    internal class TileAddressableConfigMaker : IOnTileGenerateStart, IAfterTileAssetBuild
    {
        private readonly DynamicTileProcessingContext context;
        private MonoScriptBundleNaming? savedMonoScriptBundleNaming;
        private string savedMonoScriptBundleCustomNaming;

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
            if (context.IsExcludeAssetFolder)
            {
                AddressablesUtility.SetRemoteProfileSettings(context.BuildFolderPath, context.AddressableGroupName);
            }
            else
            {
                AddressablesUtility.SetProfileForAssetBundleInAssets(context.AddressableGroupName);
            }
            AddressablesUtility.SetGroupLoadAndBuildPath(context.AddressableGroupName);
            
            // MonoScript Bundle の生成を無効化します。
            // デフォルトではMonoScript BundleがLibraryフォルダに生成されますが、それを無効化することでLibraryフォルダへの依存がなくなり、他のPCで動作させることが簡単になります。
            // Addressables 2.x では Disabled が削除されたため、Custom に設定して名前を null にすることで生成を抑制します。
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                savedMonoScriptBundleNaming = settings.MonoScriptBundleNaming;
                savedMonoScriptBundleCustomNaming = settings.MonoScriptBundleCustomNaming;

#if UNITY_6000_0_OR_NEWER
                settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Custom;
                settings.MonoScriptBundleCustomNaming = null;
#else
                settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Disabled;
#endif
            }
            
            AddressablesUtility.SaveAddressableSettings();
            
            return true;
        }

        /// <summary>
        /// タイルビルド後、Addressableの設定を元に戻します。
        /// </summary>
        public bool AfterTileAssetBuild()
        {
            AddressablesUtility.BackToDefaultProfile();
            RestoreMonoScriptBundleNaming();
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
            RestoreMonoScriptBundleNaming();
        }

        private void RestoreMonoScriptBundleNaming()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null && savedMonoScriptBundleNaming.HasValue)
            {
                settings.MonoScriptBundleNaming = savedMonoScriptBundleNaming.Value;
                settings.MonoScriptBundleCustomNaming = savedMonoScriptBundleCustomNaming;
                AddressablesUtility.SaveAddressableSettings();
            }
            
            savedMonoScriptBundleNaming = null;
            savedMonoScriptBundleCustomNaming = null;
        }
    }
}