using PLATEAU.CityAdjust.ChangeActive;
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
        /// </summary>
        public async Task ExecAsync(CityImportConfig config, CancellationToken cancelToken)
        {
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
                    return;
                }
            }
            
            // 動的タイルのバリデーション
            if (!config.ValidateForTile())
            {
                Debug.LogError($"validation failed.");
                return;
            }

            // 事前処理を実行
            progressDisplay?.SetProgress(TileProgressTitle, 0f, "動的タイル生成を開始中...");
            dynamicTileExporter = new DynamicTileExporter(progressDisplay);
            bool preProcessSucceed = dynamicTileExporter.SetupPreProcessing(config.DynamicTileImportConfig);
            if (!preProcessSucceed)
            {
                Debug.LogError("動的タイルの事前処理に失敗しました。");
                return;
            }
            progressDisplay?.SetProgress(TileProgressTitle, 10f, "動的タイル生成を開始中...");
            
            // GMLを1つインポート完了したときの処理を登録します。
            var postGmlImport = new List<IPostGmlImportProcessor>
            {
                new CityDuplicateProcessor(), // 重複した低LODを非表示にします
                dynamicTileExporter // 動的タイル化します
            };
            
            // インポートを実行
            await CityImporter.ImportAsync(config, progressDisplay, cancelToken, postGmlImport);
            
            // 事後処理
            try
            {
                progressDisplay?.SetProgress(TileProgressTitle, 90f, "最終処理を実行中...");
                // 実際の完了処理をDynamicTileExporterに委譲
                dynamicTileExporter.CompleteProcessing();
            }catch (System.OperationCanceledException)
            {
                Debug.Log("動的タイルインポート処理がキャンセルされました。");
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"動的タイルインポート処理中にエラーが発生しました: {ex.Message}");
                return;
            }
            
            progressDisplay?.SetProgress(TileProgressTitle, 100f, "動的タイル生成完了");
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