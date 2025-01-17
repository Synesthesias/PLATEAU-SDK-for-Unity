using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.CityExport;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Common.PathSelector;
using PLATEAU.Editor.Window.Main.Tab.ExportGuiParts;
using PLATEAU.Geometries;
using PLATEAU.Util;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Directory = System.IO.Directory;
using ProgressBar = PLATEAU.Util.ProgressBar;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// 都市のモデルデータのエクスポートのGUIです。
    /// </summary>
    /// エクスポートの座標軸設定について留意点 :
    /// Unity の座標系は EUN なので、objファイルを EUN でエクスポートしてそれを Unity にインポートすれば
    /// 元のモデルと同じものが現れるだろうと考えるのが自然です。
    /// ところが、実際には X軸方向に反転したモデルが表示されます。
    /// これは Unityの仕様によります。
    /// Unityは、objファイルは右手座標系であると考えます。実際には左手座標系である EUN で座標を記述したとしてもです。
    /// 右手座標系を左手座標系に補正するため、Unityは自動で objファイルに記載された X座標の正負を反転させます。
    /// そのため左右が反転します。
    /// 対して、Blender は objファイルのインポート時に座標系を設定できるので、
    /// Blenderの画面で正しく設定すればモデルが反転することなく objファイルをインポートできます。
    /// なお、Unity で正しい形状になるようにエクスポートしたければ、 EUN を左右反転させた座標系である WUN を利用してください。
    /// 参考 : https://gamedev.stackexchange.com/questions/39906/why-does-unity-obj-import-flip-my-x-coordinate
    internal class CityExportGui : ITabContent
    {
        private PLATEAUInstancedCityModel exportTarget;
        private CoordinateSystemGui coordinateSystemGui;
        private readonly MeshFileFormatGui meshFileFormatGui;

        private readonly Dictionary<MeshFileFormat, IExportConfigGUI> formatToExporterGUI = new()
        {
            { MeshFileFormat.OBJ, new ExportConfigGuiObj() },
            { MeshFileFormat.FBX, new ExportConfigGuiFbx() },
            { MeshFileFormat.GLTF, new ExportConfigGuiGltf() }
        };
        
        private bool exportTextures = true;
        private bool exportDefaultTextures;
        private bool exportHiddenObject;
        private MeshExportOptions.MeshTransformType meshTransformType = MeshExportOptions.MeshTransformType.Local;
        private string exportDirPath = "";
        private bool foldOutOption = true;
        private bool foldOutExportPath = true;
        private readonly PathSelectorFolder exportDirSelector = new PathSelectorFolder();

        public CityExportGui()
        {
            // フォーマット選択に応じて、座標軸の選択肢が変わるようにイベントを設定します。
            meshFileFormatGui = new MeshFileFormatGui();
            meshFileFormatGui.OnFormatChanged += (format) =>
            {
                var selectedSystem = coordinateSystemGui?.SelectedCoordinateSystem ?? CoordinateSystem.WUN;
                coordinateSystemGui = new CoordinateSystemGui(format, selectedSystem);
            };
            meshFileFormatGui.InvokeOnFormatChanged();
        }
        
        public VisualElement CreateGui()
        {
            return new IMGUIContainer(Draw);
        }
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのエクスポートを行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", "num1.png");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.exportTarget =
                    (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                        "エクスポート対象", this.exportTarget,
                        typeof(PLATEAUInstancedCityModel), true);
            }
            PlateauEditorStyle.Heading("出力形式", "num2.png");
            meshFileFormatGui.Draw();
            
            this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    // 選択した出力設定に固有の設定
                    this.formatToExporterGUI[meshFileFormatGui.SelectedFormat].Draw();

                    this.exportTextures = PlateauEditorStyle.Toggle("テクスチャを含める", this.exportTextures);
                    if (exportTextures)
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel2())
                        {
                            float prevLabelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 200;
                            exportDefaultTextures =
                                PlateauEditorStyle.Toggle("SDK付属のデフォルトテクスチャを含める", exportDefaultTextures);
                            EditorGUIUtility.labelWidth = prevLabelWidth;
                        }
                    }
                    this.exportHiddenObject = PlateauEditorStyle.Toggle("非アクティブオブジェクトを含める", this.exportHiddenObject);
                    this.meshTransformType =
                        (MeshExportOptions.MeshTransformType)EditorGUILayout.EnumPopup("座標変換", this.meshTransformType);

                    coordinateSystemGui.Draw();
                    
                }
            });

            this.foldOutExportPath = PlateauEditorStyle.FoldOut(this.foldOutExportPath, "出力フォルダ", () =>
            {
                this.exportDirPath = this.exportDirSelector.Draw("フォルダパス");
            });
            
            PlateauEditorStyle.Separator(0);
            if (PlateauEditorStyle.MainButton("エクスポート"))
            {
                Export(this.exportDirPath, this.exportTarget);
            }
        }

        private void Export(string destinationDir, PLATEAUInstancedCityModel target)
        {
            if (target == null)
            {
                Dialogue.Display("エクスポート失敗：\nエクスポート対象を指定してください。", "OK");
                return;
            }

            if (string.IsNullOrEmpty(destinationDir))
            {
                Dialogue.Display("エクスポート失敗：\nエクスポート先を指定してください。", "OK");
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Dialogue.Display("エクスポート失敗：\nエクスポート先フォルダが実在しません。\n再度エクスポート先を指定してください。", "OK");
                return;
            }

            if (!WarnIfFileExist(destinationDir))
            {
                return;
            }

            var format = meshFileFormatGui.SelectedFormat;
            var meshExportOptions = new MeshExportOptions(this.meshTransformType, this.exportTextures, exportDefaultTextures, this.exportHiddenObject,
                format, coordinateSystemGui.SelectedCoordinateSystem, this.formatToExporterGUI[format].GetExporter());
            using (var progress = new ProgressBar("エクスポート中..."))
            {
                progress.Display(0.5f);
                try
                {
                    UnityModelExporter.Export(destinationDir, target, meshExportOptions);
                }
                catch (Exception e)
                {
                    Dialogue.Display("エラーによりエクスポートを中止しました。", "OK");
                    Debug.LogError(e);
                    return;
                }

                EditorUtility.RevealInFinder(destinationDir +"/");
            }

            Dialogue.Display("エクスポートが完了しました。", "OK");
        }

        /// <summary>
        /// フォルダ内に、出力しようとしているファイルと同じ拡張子のものがあれば上書きの警告を出します。
        /// ユーザーが警告に了承すればtrueを、拒否すればfalseを返します。
        /// </summary>
        private bool WarnIfFileExist(string destinationDir)
        {
            // 厳密には、拡張子チェックだけするよりも、出力しようとしているファイル名をすべて調べるのがベストです。
            // しかし、エクスポートを共通ライブラリに任せている都合上それは難しいので、拡張子のみのチェックとします。
            bool found = meshFileFormatGui.SelectedFormat.ToExtensions().Any(extension => FileExistsWithinDepth1(destinationDir, extension, 0));

            if (found)
            {
                return Dialogue.Display("同名のファイルは上書きされます。よろしいですか？", "OK", "キャンセル");
            }

            return true;
        }

        /// <summary>
        /// ディレクトリを再帰探索したとき、再帰の深さが0をトップとして1以内に指定拡張子のファイルがあるかを調べます。
        /// エクスポートの上書きチェックの用途なら、深さは1以内で探せば十分です。
        /// </summary>
        private bool FileExistsWithinDepth1(string destinationDir, string extension, int depth)
        {
            if (depth > 1) return false;
            var files =
                Directory.EnumerateFiles(destinationDir, "*" + extension, SearchOption.TopDirectoryOnly);
            if (files.Any())
            {
                return true;
            }

            foreach (var dir in Directory.EnumerateDirectories(destinationDir))
            {
                bool result = FileExistsWithinDepth1(dir, extension, depth + 1);
                if (result) return true;
            }

            return false;
        }

        public void Dispose() { }
        public void OnTabUnselect(){}
    }
}
