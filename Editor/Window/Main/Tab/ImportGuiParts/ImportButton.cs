using PLATEAU.CityAdjust.ChangeActive;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.ProgressDisplay;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
{
    /// <summary>
    /// 「モデルをインポート」ボタンの描画と実行を行います。
    /// </summary>
    internal class ImportButton
    {
        /// <summary>
        /// 「モデルをインポート」のキャンセル用Tokenソース
        /// インポートタスクは1本なので発行するトークンも１つ
        /// </summary>
        private CancellationTokenSource cancellationTokenSrc;

        private int numCurrentRunningTasks;
        private ImportToDynamicTile importToDynamicTile;
        private volatile bool cancelRequested;

        /// <summary>
        /// 「モデルをインポート」ボタンの描画と実行を行います。
        /// </summary>
        public void Draw(CityImportConfig config, IProgressDisplay progressDisplay)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (Volatile.Read(ref numCurrentRunningTasks) <= 0)
                {
                    // 動的タイルインポート時のバリデーション
                    bool isDynamicTile = config.DynamicTileImportConfig.ImportType == ImportType.DynamicTile;
                    bool isValidPath = true;
                    if (isDynamicTile && !string.IsNullOrEmpty(config.DynamicTileImportConfig.OutputPath))
                    {
                        isValidPath = config.DynamicTileImportConfig.IsValidOutputPath;
                    }

                    EditorGUI.BeginDisabledGroup(!isValidPath);
                    // ボタンを描画します。
                    if (PlateauEditorStyle.MainButton("モデルをインポート"))
                    {
                        (progressDisplay as ProgressDisplayGUI)?.Clear();
                        // タスク数をインクリメントし、キャンセルトークンを初期化
                        cancelRequested = false; 
                        Interlocked.Increment(ref numCurrentRunningTasks);
                        cancellationTokenSrc = new CancellationTokenSource();

                        switch (config.DynamicTileImportConfig.ImportType)
                        {
                            case ImportType.Scene:
                                // シーン上へのインポートを実行します。
                                ExecuteNormalImport(config, progressDisplay);
                                break;
                            case ImportType.DynamicTile:
                                // 動的タイル形式でのインポートを実行します。
                                importToDynamicTile = new ImportToDynamicTile(progressDisplay);
                                Task<bool> task;
                                try
                                {
                                    task = importToDynamicTile.ExecAsync(config, cancellationTokenSrc.Token);
                                }catch (Exception ex)
                                {
                                    CleanupDynamicTileImport();
                                    Dialogue.Display("動的タイルのインポートでエラーが発生しました。処理を中断しました。", "OK");
                                    UnityEngine.Debug.LogException(ex);
                                    break;
                                }
                                task.ContinueWith(_ =>
                                {
                                    CleanupDynamicTileImport();
                                });
                                task.ContinueWith(t =>
                                {
                                    if (t.Result)
                                    {
                                        EditorApplication.delayCall += () => Dialogue.Display("動的タイルの保存が完了しました！", "OK");
                                    }
                                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                                task.ContinueWithErrorCatch();
                                break;
                            default:
                                throw new Exception("invalid import type.");
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else if (cancellationTokenSrc?.IsCancellationRequested == true)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    PlateauEditorStyle.CancelButton("キャンセル中…");
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    //Cancel ボタンを描画します。
                    if (PlateauEditorStyle.CancelButton("インポートをキャンセルする"))
                    {
                        bool dialogueResult = Dialogue.Display("インポートをキャンセルしますか？", "はい", "いいえ");
                        if (dialogueResult)
                        {
                            cancelRequested = true;
                            cancellationTokenSrc?.Cancel();

                            if (config.DynamicTileImportConfig.ImportType != ImportType.DynamicTile)
                            {
                                return;
                            }
                            // 動的タイルインポートのクリーンアップ
                            importToDynamicTile?.CancelImport();
                            importToDynamicTile = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通常のインポート処理を実行します
        /// </summary>
        private void ExecuteNormalImport(CityImportConfig config, IProgressDisplay progressDisplay)
        {

            var postGmlProcessors = new List<IPostGmlImportProcessor>
            {
                new CityDuplicateProcessor() // 重複した低LODを非表示にします。
            };
            var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, postGmlProcessors);
 
            // 完了後にUIスレッドでダイアログを出す
            task.ContinueWith(t =>
            {
                try
                {
                    Interlocked.Decrement(ref numCurrentRunningTasks);
                    if (cancelRequested)
                    {
                        EditorApplication.delayCall += () =>
                            Dialogue.Display("インポートをキャンセルしました", "OK");
                    }
                    else
                    {
                        EditorApplication.delayCall += () =>
                            Dialogue.Display("インポートが完了しました！", "OK");
                    }
                    
                    EditorApplication.delayCall += SceneView.RepaintAll;
                    
                }
                finally
                {
                    cancellationTokenSrc?.Dispose();
                    cancellationTokenSrc = null;
                }
            });
            task.ContinueWithErrorCatch();
        }

        private void CleanupDynamicTileImport()
        {
            Interlocked.Decrement(ref numCurrentRunningTasks);
            cancellationTokenSrc?.Dispose();
            cancellationTokenSrc = null;
            importToDynamicTile = null;
        }
        
    }
}
