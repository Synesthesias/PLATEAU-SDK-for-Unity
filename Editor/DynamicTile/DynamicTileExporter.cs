using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.Editor.DynamicTile.TileModule;
using PLATEAU.Editor.TileAddressables;
using PLATEAU.Util;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    /// <summary>
    /// 都市モデルをDynamicTile用にプレハブ化し、一括でアセットバンドルとして出力する。
    /// </summary>
    internal class DynamicTileExporter : IPostTileImportProcessor
    {

        public const string AddressableLabel = "DynamicTile";
        public const string AddressableAddressBase = "PLATEAUTileMeta";
        
        
        /// <summary> タイルエクスポートの事前処理を行うクラス </summary>
        private readonly IOnTileGenerateStart[] onTileGenerateStarts;

        /// <summary> タイルアセットのビルドの直前に行う処理 </summary>
        private readonly IBeforeTileAssetBuild[] beforeTileAssetBuilds;
        
        /// <summary> タイルエクスポートの事後処理（ビルド後）</summary>
        private readonly IAfterTileAssetBuild[] afterTileAssetBuilds;

        /// <summary> タイルを1つインポートするごとの処理 </summary>
        private readonly IOnOneTileImported[] onOneTileImported;
        
        /// <summary> タイル生成がキャンセルされた時の処理 </summary>
        private readonly IOnTileGenerationCancelled[] onTileGenerationCancelled;

        /// <summary> タイルビルド前後で失敗した場合の処理 </summary>
        private readonly IOnTileBuildFailed[] onTileBuildFailed;

        private readonly DynamicTileProcessingContext context;
        
        public DynamicTileExporter(DynamicTileProcessingContext context, CityImportConfig cityConf, IProgressDisplay progressDisplay)
        {
            this.context = context;
            
            // 各処理を生成します。
            var tileManagerGenerator = new TileManagerGenerator(context);
            var tileAddressableConfigMaker = new TileAddressableConfigMaker(context);
            var initializeTileManagerAndFocus = new InitializeTileManagerAndFocus();
            var tileEditorProcedure = new TileGenEditorProcedure();
            var setupBuildPath = new SetUpTileBuildPath(context);
            var addTilesToOldMetaIfExist = new AddTilesToOldMetaIfExist(context);
            var setReferencePointToSameIfExist = new SetReferencePointSameIfExist(cityConf, context);
            var generateOneTile = new GenerateOneTile(context, progressDisplay);
            var saveAndRegisterMetaData = new SaveAndRegisterMetaData(context);
            var cleanUpTempFolder = new TileCleanupTempFolder(context);
            var exportUnityPackage = new ExportUnityPackageOfPrefabs(context);
            
            // フェイズ1: 事前処理
            // 動的タイル出力の事前処理を列挙します。
            onTileGenerateStarts = new IOnTileGenerateStart[]
            {
                tileEditorProcedure, // エディタ上での準備
                setupBuildPath, // Addressableビルドパスを設定
                tileManagerGenerator, // 古いTileManagerを消します。
                tileAddressableConfigMaker, // Addressableの設定を行います。
                setReferencePointToSameIfExist // 既存のタイルがあればそれと同じ基準点を使うようにします。
            };
            
            // フェイズ2: タイル生成
            // 1タイル生成ごとの処理を列挙します。
            onOneTileImported = new IOnOneTileImported[]
            {
                generateOneTile // 1タイル生成ごとに呼ばれます。
            };
            
            // フェイズ3: ビルド直前
            // タイルをビルドする直前の処理を列挙します。
            beforeTileAssetBuilds = new IBeforeTileAssetBuild[]
            {
                addTilesToOldMetaIfExist, // 前と同じフォルダに出力するなら追加します。前のフォルダにあるunity packageのインポートも行います。
                saveAndRegisterMetaData, // メタデータを保存・登録
                tileEditorProcedure // エディタ上での準備。処理順の都合上、配列の最後にしてください。
            };
            
            // フェイズ4: ビルド直後
            // タイルをビルドしたあとの処理を列挙します。
            afterTileAssetBuilds = new IAfterTileAssetBuild[]
            {
                tileManagerGenerator, // TileManagerを生成します。
                initializeTileManagerAndFocus, // タイル初期化とシーンビューのフォーカス
                tileAddressableConfigMaker, // Addressableの設定を消去し元に戻します。
                exportUnityPackage, // 生成したプレハブ群をUnityPackageとしてエクスポートします。cleanupTempFolderより前に行ってください。
                cleanUpTempFolder, // 不要なフォルダを消します。
                tileEditorProcedure // エディタ上での後始末。処理の都合上、配列の最後にしてください。
            };
            
            // タイル生成のキャンセル時の処理を列挙します。
            onTileGenerationCancelled = new IOnTileGenerationCancelled[]
            {
                tileEditorProcedure
            };

            onTileBuildFailed = new IOnTileBuildFailed[] { tileEditorProcedure };
        }

        /// <summary>
        /// 動的タイルの事前処理を行います。
        /// 成否を返します。
        /// </summary>
        public bool SetupPreProcessing()
        {
            try
            {
                
                // 与えられた事前処理を実行します。
                foreach (var before in onTileGenerateStarts)
                {
                    var result = before.OnTileGenerateStart();
                    if (!result)
                    {
                        throw new Exception("動的タイルの事前処理に失敗しました。");
                    }
                }

                return true; 
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                // 与えられた事前処理の例外系を実行します。
                foreach (var before in onTileGenerateStarts)
                {
                    before.OnTileGenerateStartFailed();
                }
                return false;
            }
        }
        

        /// <summary>
        /// DynamicTileの完了処理を行います（メタストア保存、Addressable処理、マネージャー設定）
        /// 成否を返します。
        /// </summary>
        public async Task<bool> CompleteProcessingAsync(CancellationToken ct)
        {
            try
            {
                // 与えられたビルド直前の処理を実行
                foreach (var before in beforeTileAssetBuilds)
                {
                    ct.ThrowIfCancellationRequested();
                    var ok = await before.BeforeTileAssetBuildAsync(ct);
                    if (!ok)
                    {
                        Debug.LogError("failed on beforeTileAssetsBuild.");
                        foreach(var f in onTileBuildFailed) f.OnTileBuildFailed();
                        return false;
                    }
                }
                //uv4に入ってる属性情報が消えてしまうのでStripさせないようにしたうえでビルドする
                //MEMO: AssetBundleが大幅に肥大化してしまうようであればuv4を明示的に参照するシェーダーを用意するなどしてstrip対象にさせないようなアプローチも検討
                var currentStripUnusedMeshComponents = PlayerSettings.stripUnusedMeshComponents;
                try
                {
                    PlayerSettings.stripUnusedMeshComponents = false;
                    // Addressablesのビルドを実行
                    AddressablesUtility.BuildAddressables(context.BuildMode);
                }
                finally
                {
                    PlayerSettings.stripUnusedMeshComponents = currentStripUnusedMeshComponents;
                }

                // ビルド後の処理で与えられたものを実行します
                foreach (var after in afterTileAssetBuilds)
                {
                    ct.ThrowIfCancellationRequested();
                    bool result = after.AfterTileAssetBuild();
                    if (!result) return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"動的タイルのエクスポート中にエラーが発生しました: {ex}");
                // 与えられた例外系を実行します。
                foreach(var f in onTileBuildFailed)
                {
                    f.OnTileBuildFailed();
                }
                return false;
            }
        }

        /// <summary>
        /// 進行中のAddressable化をキャンセルします。
        /// </summary>
        public void Cancel()
        {
            
            // 与えられたキャンセル処理を実行
            foreach (var c in onTileGenerationCancelled)
            {
                c.OnTileGenerationCancelled();
            }
        }
        
        /// <summary>
        /// インポートしつつ動的タイルにするモードにおいて、GML1つがインポートされた後に呼ばれます。
        /// 動的タイルにします。
        /// <see cref="IPostTileImportProcessor"/> のコールバック実装です。
        /// </summary>
        public void OnTileImported(TileImportResult importResult)
        {
            foreach (var o in onOneTileImported)
            {
                o.OnOneTileImported(importResult);
            }
            
        }
    }
}