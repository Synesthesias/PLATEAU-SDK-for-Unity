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

        /// <summary> タイルビルド後のグループ設定を整理します。 </summary>
        private void Cleanup()
        {
            var groupName = context.AddressableGroupName;
            if (string.IsNullOrEmpty(groupName)) return;

            if (context.IsExcludeAssetFolder)
            {
                // Assets外: ユーザーの設定を汚さないようグループを削除します。
                // ロード時にはカタログパスから設定を読み直すので動く仕組みです。
                AddressablesUtility.RemoveGroup(groupName);
            }
            else
            {
                // Assets内: グループは残すが IncludeInBuild を false にします。
                // ビルド対象から外さないとAddressablesメニューからの再ビルド時にタイルバンドルが
                // 再ビルドされ、カタログはSDK利用者の指定した別の場所(ServerData等)に出力されるため、
                // PLATEAUBundles内の既存カタログとCRCが不整合になります。
                // タイルバンドルとカタログは既にPLATEAUBundlesに存在しており、
                // DynamicTileが明示的にカタログをロードするため問題ありません。
                // 次回のタイルビルド時はOnTileGenerateStartでグループが再作成されます。
                AddressablesUtility.SetGroupIncludeInBuild(groupName, false);
            }
        }
        
        public void OnTileGenerateStartFailed()
        {
            AddressablesUtility.BackToDefaultProfile();
            RestoreMonoScriptBundleNaming();
            Cleanup();
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