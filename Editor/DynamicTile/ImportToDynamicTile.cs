using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.Editor.DynamicTile
{
    /// <summary>
    /// 都市モデルをインポートしながら動的タイルを生成します。
    /// </summary>
    public class ImportToDynamicTile
    {
        private DynamicTileExporter dynamicTileExporter;
        private IProgressDisplay progressDisplay;
        public const string TileProgressTitle = "動的タイル生成処理";

        public ImportToDynamicTile(IProgressDisplay progressDisplay)
        {
            this.progressDisplay = progressDisplay;
        }
        
        /// <summary>
        /// インポートしながら動的タイルを生成します。
        /// 成否を返します。
        /// </summary>
        public async Task<bool> ExecAsync(CityImportConfig config, CancellationToken cancelToken)
        {
            
            // 動的タイルのバリデーション
            if (!config.ValidateForTile())
            {
                Debug.LogError($"validation failed.");
                return false;
            }
            
            // アセットバンドルのビルド時はシーンの保存が求められますが、
            // 処理の途中で「保存しますか」と聞くよりも最初に聞いた方が良いので聞きます。
            if (SceneManager.GetActiveScene().isDirty)
            {
                bool saveScene = Dialogue.Display("シーンの保存が必要です。保存して続行しますか？", "続行", "キャンセル");
                
                if (saveScene)
                {
                    EditorSceneManager.SaveOpenScenes();
                }
                else
                {
                    Debug.Log("シーンの保存が拒否されたため処理を中止します。");
                    return false;
                }
            }

            // 事前処理を実行
            progressDisplay?.SetProgress(TileProgressTitle, 0f, "動的タイル生成を開始中...");
            
            var context = new DynamicTileProcessingContext(config.DynamicTileImportConfig);
            dynamicTileExporter = new DynamicTileExporter(context, config, progressDisplay);
            
            bool preProcessSucceed = dynamicTileExporter.SetupPreProcessing();
            if (!preProcessSucceed)
            {
                Debug.LogError("動的タイルの事前処理に失敗しました。");
                return false;
            }
            progressDisplay?.SetProgress(TileProgressTitle, 10f, "動的タイル生成を開始中...");

            // GMLを1つインポート完了したときの処理を登録します。
            var postGmlImport = new List<IPostTileImportProcessor>
            {
                dynamicTileExporter // 動的タイル化します
            };

            // インポートを実行
            var task = TileImporter.ImportAsync(config, progressDisplay, cancelToken, postGmlImport);
            await task;
            
            // 事後処理
            bool succeed = false;
            try
            {
                progressDisplay?.SetProgress(TileProgressTitle, 90f, "最終処理を実行中...");
                // 実際の完了処理をDynamicTileExporterに委譲
                succeed = await dynamicTileExporter.CompleteProcessingAsync(cancelToken);
            }catch (System.OperationCanceledException)
            {
                Debug.Log("動的タイルインポート処理がキャンセルされました。");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"動的タイルインポート処理中にエラーが発生しました: {ex}");
                return false;
            }
            
            progressDisplay?.SetProgress(TileProgressTitle, 100f, "動的タイル生成完了");
            return succeed;
        }
        
        /// <summary>
        /// インポートのキャンセルボタンが押された時
        /// </summary>
        public void CancelImport()
        {
            progressDisplay?.SetProgress(TileProgressTitle, 0f, "キャンセルされました。");
            dynamicTileExporter?.Cancel();
        }
    }
    
    
}