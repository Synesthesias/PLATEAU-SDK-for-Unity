using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ExportGUIParts;
using PLATEAU.Geometries;
using UnityEditor;
using UnityEngine;
using Directory = System.IO.Directory;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
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
    internal class CityExportGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel exportTarget;
        private MeshFileFormat meshFileFormat = MeshFileFormat.OBJ;

        private Dictionary<MeshFileFormat, IPlateauModelExporter> formatToExporter = new()
        {
            { MeshFileFormat.OBJ, new ObjModelExporter() },
            { MeshFileFormat.FBX, new FbxModelExporter() },
            { MeshFileFormat.GLTF, new GltfModelExporter() }
        };
        
        private bool exportTextures;
        private bool exportHiddenObject;
        private MeshExportOptions.MeshTransformType meshTransformType = MeshExportOptions.MeshTransformType.Local;
        private CoordinateSystem meshAxis = CoordinateSystem.ENU;
        private static readonly List<CoordinateSystem> meshAxisChoices = ((CoordinateSystem[])Enum.GetValues(typeof(CoordinateSystem))).ToList();
        private static readonly string[] meshAxisDisplay = meshAxisChoices.Select(axis => axis.ToNaturalLanguage()).ToArray();
        private string exportDirPath = "";
        private bool foldOutOption = true;
        private bool foldOutExportPath = true;
        private PathSelectorFolder exportDirSelector = new PathSelectorFolder();
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
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.meshFileFormat = (MeshFileFormat)EditorGUILayout.EnumPopup("出力形式", this.meshFileFormat);
            }
            
            this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    // 選択した出力設定に固有の設定
                    this.formatToExporter[this.meshFileFormat].DrawConfigGUI();

                    this.exportTextures = EditorGUILayout.Toggle("テクスチャ", this.exportTextures);
                    this.exportHiddenObject = EditorGUILayout.Toggle("非アクティブオブジェクトを含める", this.exportHiddenObject);
                    this.meshTransformType =
                        (MeshExportOptions.MeshTransformType)EditorGUILayout.EnumPopup("座標変換", this.meshTransformType);

                    
                    this.meshAxis = meshAxisChoices[EditorGUILayout.Popup("座標軸", meshAxisChoices.IndexOf(this.meshAxis), meshAxisDisplay)];
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

        // FIXME 出力したファイルパスのリストを返すようにできるか？
        private void Export(string destinationDir, PLATEAUInstancedCityModel target)
        {
            if (target == null)
            {
                Debug.LogError("エクスポート対象が指定されていません。");
                return;
            }

            if (string.IsNullOrEmpty(destinationDir))
            {
                Debug.LogError("エクスポート先が指定されていません。");
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Debug.LogError("エクスポート先フォルダが実在しません。");
                return;
            }
            var meshExportOptions = new MeshExportOptions(this.meshTransformType, this.exportTextures, this.exportHiddenObject,
                this.meshFileFormat, this.meshAxis, this.formatToExporter[this.meshFileFormat]);
            UnityModelExporter.Export(destinationDir, target,  meshExportOptions);
        }
    }
}
