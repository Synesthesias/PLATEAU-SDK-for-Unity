using PLATEAU.CityImport.Config;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.DynamicTile.TileModule
{
    /// <summary>
    /// 保存先にすでにメタデータがあるのなら、ReferencePointを既存のメタデータに合わせます。
    /// なければ範囲選択後のデフォルト値が使われます。
    /// これにより、同じフォルダに複数回タイルを生成した場合でも位置が合うようになります。
    /// </summary>
    public class SetReferencePointSameIfExist : IOnTileGenerateStart
    {
        private readonly  CityImportConfig cityConfig;
        private readonly DynamicTileProcessingContext context;

        public SetReferencePointSameIfExist(CityImportConfig cityConfig, DynamicTileProcessingContext context)
        {
            this.cityConfig = cityConfig;
            this.context = context;
        }
        
        public bool OnTileGenerateStart()
        {
            Vector3 rp = cityConfig.ReferencePoint.ToUnityVector(); // デフォルト値
            try
            {
                // 1) Assets 内にメタがある場合
                string dataPath = context.DataPath;
                var existingMeta = AssetDatabase.LoadAssetAtPath<PLATEAUDynamicTileMetaStore>(dataPath);
                if (existingMeta != null)
                {
                    rp = existingMeta.ReferencePoint;
                }
                else
                {
                    // 2) 外部出力先のカタログからAddressables経由で読み込み
                    if (!string.IsNullOrEmpty(context.BuildFolderPath) && Directory.Exists(context.BuildFolderPath))
                    {
                        var catalogFiles = TileCatalogSearcher.FindCatalogFiles(context.BuildFolderPath, true);
                        if (catalogFiles.Length > 0)
                        {
                            var latestCatalog = catalogFiles[0];
                            var loader = new AddressableLoader();
                            var oldMeta = loader.InitializeAsync(latestCatalog).GetAwaiter().GetResult();
                            if (oldMeta != null)
                            {
                                rp = oldMeta.ReferencePoint;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"既存メタからReferencePointの取得に失敗しました: {ex.Message}");
            }
            finally
            {
                context.MetaStore.ReferencePoint = rp;
                cityConfig.ReferencePoint = rp.ToPlateauVector();
            }

            return true;
        }

        public void OnTileGenerateStartFailed()
        {
            // noop
        }
    }
}