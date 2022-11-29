using System.IO;
using System.Threading;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Load;
using PLATEAU.CityImport.Setting;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.CityImport.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Dataset;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityImportLocalGUI : IEditorDrawable, IAreaSelectResultReceiver, IProgressDisplay
    {
        private readonly PathSelectorFolderPlateauInput folderSelector = new PathSelectorFolderPlateauInput();
        private readonly CityLoadConfig config = new CityLoadConfig();
        // インポートの処理状況はウィンドウを消しても残しておきたいので static にします。
        private static readonly ProgressDisplayGUI progressGUI = new ProgressDisplayGUI();
        private bool isAreaSelectComplete;
        private bool foldOutSourceFolderPath = true;
        private SynchronizationContext mainThreadContext;
        private UnityEditor.EditorWindow parentEditorWindow;

        private static int numCurrentRunningTasks;

        /// <summary>
        /// メインスレッドから呼ばれることを前提とします。
        /// </summary>
        public CityImportLocalGUI(UnityEditor.EditorWindow parentEditorWindow)
        {
            this.mainThreadContext = SynchronizationContext.Current;
            this.parentEditorWindow = parentEditorWindow;
        }

        public void Draw()
        {

            this.foldOutSourceFolderPath = PlateauEditorStyle.FoldOut(this.foldOutSourceFolderPath, "入力フォルダ", () =>
            {
                this.config.DatasetSourceConfig ??= new DatasetSourceConfig(false, "");
                this.config.DatasetSourceConfig.DatasetIdOrSourcePath = this.folderSelector.Draw("フォルダパス");
            });
            
            PlateauEditorStyle.Separator(0);
            PlateauEditorStyle.SubTitle("モデルデータの配置を行います。");
            PlateauEditorStyle.Heading("基準座標系の選択", "num1.png");

            // 基準座標系についてはこのWebサイトを参照してください。
            // https://www.gsi.go.jp/sokuchikijun/jpc.html
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.config.CoordinateZoneID = EditorGUILayout.Popup(
                    "基準座標系", this.config.CoordinateZoneID - 1, 
                    GeoReference.ZoneIdExplanation
                    ) + 1; // 番号は 1 スタート
            }
            

            PlateauEditorStyle.Heading("マップ範囲選択", "num2.png");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (PlateauEditorStyle.MainButton("範囲選択"))
                {
                    string sourcePath = this.config.DatasetSourceConfig.DatasetIdOrSourcePath;
                    if (!Directory.Exists(sourcePath))
                    {
                        EditorUtility.DisplayDialog("PLATEAU SDK", $"入力フォルダが存在しません。\nフォルダを指定してください。", "OK");
                        return;
                    }

                    var datasetSourceInitializer = new DatasetSourceConfig(false, sourcePath);
                    AreaSelectorStarter.Start(datasetSourceInitializer, this, this.config.CoordinateZoneID);
                    GUIUtility.ExitGUI();
                }
                this.isAreaSelectComplete = this.config.AreaMeshCodes != null && this.config.AreaMeshCodes.Length > 0;
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    string str = this.isAreaSelectComplete ? "範囲選択 : セット済" : "範囲選択 : 未";
                    PlateauEditorStyle.LabelSizeFit(new GUIContent(str), EditorStyles.label);
                });
            }
            
            
            if (this.isAreaSelectComplete)
            {
                PlateauEditorStyle.Heading("地物別設定", "num3.png");
                CityLoadConfigGUI.Draw(this.config);
                
                PlateauEditorStyle.Separator(0);
                
                PlateauEditorStyle.Separator(0);
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    if (PlateauEditorStyle.MainButton("モデルをインポート"))
                    {
                        if (numCurrentRunningTasks > 0)
                        {
                            bool dialogueResult = EditorUtility.DisplayDialog("PLATEAU SDK", $"すでに {numCurrentRunningTasks}つのインポート処理を実行中です。\n追加で処理に加えますか？", "はい", "いいえ");
                            if (!dialogueResult)
                            {
                                GUIUtility.ExitGUI();
                                return;
                            }
                        }
                        
                        Interlocked.Increment(ref numCurrentRunningTasks);
                        
                        // ここでインポートします。
                        var task = CityImporter.ImportAsync(this.config, this);
                        task.ContinueWith((t) => { Interlocked.Decrement(ref numCurrentRunningTasks); });
                        task.ContinueWithErrorCatch();
                    }
                }
            }
            
            PlateauEditorStyle.Separator(0);
            if (!progressGUI.IsEmpty)
            {
                PlateauEditorStyle.CenterAlignHorizontal(() =>
                {
                    PlateauEditorStyle.LabelSizeFit(new GUIContent("インポート処理"));
                });
                progressGUI.Draw();
            }
            
            
        }

        public void ReceiveResult(string[] areaMeshCodes, Extent extent, PredefinedCityModelPackage availablePackageFlags)
        {
            this.config.InitWithPackageFlags(availablePackageFlags);
            this.config.AreaMeshCodes = areaMeshCodes;
            this.config.Extent = extent;
        }

        public void SetProgress(string progressName, float percentage, string message)
        {
            progressGUI.SetProgress(progressName, percentage, message);
            this.mainThreadContext.Post(_ =>
            {
                this.parentEditorWindow.Repaint();
            }, null);
        }
    }
}
