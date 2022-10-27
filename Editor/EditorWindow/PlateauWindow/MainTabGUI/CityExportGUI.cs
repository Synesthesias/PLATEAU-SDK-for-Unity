using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.Common.PathSelector;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;
using UnityEditor;
using UnityEngine;
using Directory = System.IO.Directory;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityExportGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel exportTarget;
        private MeshFileFormat meshFileFormat = MeshFileFormat.OBJ;
        private GltfFileFormat gltfFileFormat = GltfFileFormat.GLB;
        private bool exportTextures;
        private bool exportHiddenObject;
        private MeshExportOptions.MeshTransformType meshTransformType = MeshExportOptions.MeshTransformType.Local;
        private CoordinateSystem meshAxis = CoordinateSystem.ENU;
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
                    if (this.meshFileFormat == MeshFileFormat.GLTF)
                    {
                        this.gltfFileFormat = (GltfFileFormat)EditorGUILayout.EnumPopup("GLTFフォーマット", this.gltfFileFormat);
                    }

                    this.exportTextures = EditorGUILayout.Toggle("テクスチャ", this.exportTextures);
                    this.exportHiddenObject = EditorGUILayout.Toggle("非アクティブオブジェクトを含める", this.exportHiddenObject);
                    this.meshTransformType =
                        (MeshExportOptions.MeshTransformType)EditorGUILayout.EnumPopup("座標変換", this.meshTransformType);
                    this.meshAxis = (CoordinateSystem)EditorGUILayout.EnumPopup("座標軸", this.meshAxis);
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

        // TODO 出力したファイルパスのリストを返すようにする
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
                this.meshFileFormat, this.meshAxis, new GltfWriteOptions(this.gltfFileFormat, destinationDir));
            MeshExporter.Export(destinationDir, target,  meshExportOptions);
        }
    }
}
