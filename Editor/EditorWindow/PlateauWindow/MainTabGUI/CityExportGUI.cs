using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;
using UnityEditor;
using UnityEngine;

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
        private bool foldOutOption = true;
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのエクスポートを行います。");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.exportTarget =
                    (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                        "エクスポート対象", this.exportTarget,
                        typeof(PLATEAUInstancedCityModel), true);
            }
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            // TODO
            PlateauEditorStyle.Heading("出力形式", null);
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
                }
            });
            PlateauEditorStyle.Separator(0);
            if (PlateauEditorStyle.MainButton("エクスポート"))
            {
                Export("D:\\Linoal\\Desktop\\tmpTestConv", this.exportTarget);
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
            var meshExportOptions = new MeshExportOptions(this.meshTransformType, this.exportTextures, this.exportHiddenObject,
                this.meshFileFormat, new GltfWriteOptions(this.gltfFileFormat, destinationDir));
            MeshExporter.Export(destinationDir, target,  meshExportOptions);
        }
    }
}
