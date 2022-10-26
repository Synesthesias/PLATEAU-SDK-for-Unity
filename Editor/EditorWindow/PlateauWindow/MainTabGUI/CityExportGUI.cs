using System;
using PLATEAU.CityInfo;
using PLATEAU.Editor.CityExport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Interop;
using PLATEAU.MeshWriter;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityExportGUI : IEditorDrawable
    {
        private MeshFileFormat meshFileFormat = MeshFileFormat.OBJ;
        private GltfFileFormat gltfFileFormat = GltfFileFormat.GLB;
        private bool exportHiddenObject = false;
        private MeshExportOptions.MeshTransformType meshTransformType = MeshExportOptions.MeshTransformType.Local;
        private bool foldOutOption = true;
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのエクスポートを行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            // TODO 仮
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
                    this.exportHiddenObject = EditorGUILayout.Toggle("非アクティブオブジェクトを含める", this.exportHiddenObject);
                    this.meshTransformType =
                        (MeshExportOptions.MeshTransformType)EditorGUILayout.EnumPopup("座標変換", this.meshTransformType);
                }
            });
            PlateauEditorStyle.Separator(0);
            if (PlateauEditorStyle.MainButton("エクスポート"))
            {
                Export("D:\\Linoal\\Desktop\\tmpTestConv", Object.FindObjectOfType<PLATEAUInstancedCityModel>());
            }
        }

        // TODO 出力したファイルパスのリストを返すようにする
        private void Export(string destinationDir, PLATEAUInstancedCityModel instancedModel)
        {
            var meshExportOptions = new MeshExportOptions(this.meshTransformType, this.exportHiddenObject,
                this.meshFileFormat, new GltfWriteOptions(this.gltfFileFormat, destinationDir));
            MeshExporter.Export(destinationDir, instancedModel,  meshExportOptions);
            // TODO 仮
                // using var logger = DllUnityLogger.Create();
                // var parserParams = new CitygmlParserParams(true, true, false);
                // var cityModel = CityGml.Load(gmlPath, parserParams, DllLogCallback.UnityLogCallbacks, DllLogLevel.Warning);
                // // var converter = new MeshConverter();
                // var option = new MeshConvertOptionsData
                // {
                //     // TODO 選択できるようにする
                //     MeshAxes = CoordinateSystem.ENU,
                //     MeshFileFormat = this.meshFileFormat,
                //     ReferencePoint = cityModel.GetCenterPoint(9), // TODO coordinateZoneID を選択できるようにする
                //     MeshGranularity = MeshGranularity.PerPrimaryFeatureObject,
                //     MinLOD = 0,
                //     MaxLOD = 3,
                //     ExportLowerLOD = false,
                //     ExportAppearance = true,
                //     UnitScale = 1f,
                //     CoordinateZoneID = 9
                // };
                // converter.Options = option;
                // var exportedFileNames = converter.Convert(destinationDir, gmlPath, cityModel, logger);
                // return exportedFileNames;
        }
    }
}
