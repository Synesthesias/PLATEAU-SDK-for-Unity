using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.ImportGuiParts
{
    /// <summary>
    /// 「モデルをインポート」ボタンの描画と実行を行います。
    /// </summary>
    internal static class ImportButton
    {
        /// <summary>
        /// 「モデルをインポート」のキャンセル用Tokenソース
        /// インポートタスクは1本なので発行するトークンも１つ
        /// </summary>
        private static CancellationTokenSource cancellationTokenSrc;

        private static int numCurrentRunningTasks;

        /// <summary>
        /// 「モデルをインポート」ボタンの描画と実行を行います。
        /// </summary>
        public static void Draw(CityImportConfig config, IProgressDisplay progressDisplay)
        {
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (numCurrentRunningTasks <= 0)
                {
                    // ボタンを描画します。
                    if (PlateauEditorStyle.MainButton("モデルをインポート"))
                    {
                        // 動的タイルの場合のバリデーション
                        if (config.DynamicTileImportConfig.ImportType == ImportType.DynamicTile &&
                            string.IsNullOrEmpty(config.DynamicTileImportConfig.OutputPath))
                        {
                            Dialogue.Display("動的タイル（Addressable出力）を選択する場合は、出力先を指定してください", "OK");
                            return;
                        }

                        // ボタンを実行します。
                        Interlocked.Increment(ref numCurrentRunningTasks);

                        cancellationTokenSrc = new CancellationTokenSource();

                        // Addressableインポートの場合は、事前処理を実行。
                        var addressableContext = default(DynamicTileProcessingContext);
                        
                        // ロードされたGMLファイルの数をカウントする変数
                        var loadedGmlCount = 0;
                        if (config.DynamicTileImportConfig.ImportType == ImportType.DynamicTile)
                        {
                            addressableContext = DynamicTileImportProcessor.SetupPreProcessing(progressDisplay, config);
                        }
    
                        // ここでインポートします。
                        var task = CityImporter.ImportAsync(config, progressDisplay, cancellationTokenSrc.Token, 
                            (placedObjects, totalGmls, meshCode) =>
                            {
                                if (config.DynamicTileImportConfig.ImportType == ImportType.DynamicTile)
                                {
                                    if (addressableContext == null)
                                    {
                                        return;
                                    }
                                    addressableContext.GmlCount = totalGmls;
                                    System.Threading.Interlocked.Increment(ref loadedGmlCount);

                                    // 各GMLファイルのインポート完了時に都市オブジェクトを処理
                                    DynamicTileImportProcessor.ProcessCityObjects(
                                        placedObjects,
                                        addressableContext,
                                        progressDisplay,
                                        meshCode,
                                        loadedGmlCount);
                                }
                            });

                        // インポート完了後の事後処理
                        DynamicTileImportProcessor.HandleCompletionAsync(
                            task, 
                            addressableContext, 
                            progressDisplay,
                            () => Interlocked.Decrement(ref numCurrentRunningTasks));
                        
                        task.ContinueWithErrorCatch();
                    }
                }
                else if (cancellationTokenSrc?.Token != null && cancellationTokenSrc.Token.IsCancellationRequested)
                {
                    if (PlateauEditorStyle.CancelButton("キャンセル中…")){}
                }
                else
                {
                    //Cancel ボタンを描画します。
                    if (PlateauEditorStyle.CancelButton("インポートをキャンセルする"))
                    {
                        bool dialogueResult = Dialogue.Display($"インポートをキャンセルしますか？", "はい", "いいえ");
                        if (dialogueResult)
                        {
                            cancellationTokenSrc?.Cancel();
                        }
                    }
                }
            }
        }
    }
}
