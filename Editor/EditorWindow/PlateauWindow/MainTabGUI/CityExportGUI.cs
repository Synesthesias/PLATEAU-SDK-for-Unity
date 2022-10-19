using PLATEAU.CityGML;
using PLATEAU.CityImport.Load.Convert;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    internal class CityExportGUI : IEditorDrawable
    {
        private MeshFileFormat meshFileFormat = MeshFileFormat.OBJ;
        private bool foldOutOption = true;
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("モデルデータのエクスポートを行います。");
            PlateauEditorStyle.Heading("選択オブジェクト", null);
            // TODO 仮
            this.meshFileFormat = (MeshFileFormat)EditorGUILayout.EnumPopup("出力形式", this.meshFileFormat);
            this.foldOutOption = PlateauEditorStyle.FoldOut(this.foldOutOption, "Option", () => { });
            PlateauEditorStyle.Separator(0);
            if (PlateauEditorStyle.MainButton("エクスポート"))
            {
                var exportedFileNames = Export("D:\\Linoal\\Desktop\\tmpTestConv", "C:\\Users\\Linoal\\Documents\\dev\\SynesthesiasProjects\\plateau\\PlateauUnitySDKDev\\Assets\\StreamingAssets\\.PLATEAU\\13100_tokyo23-ku_2020_citygml_3_2_op_heavy\\udx\\bldg\\53393544_bldg_6697_2_op.gml"
                    );
            }
        }

        private string[] Export(string destinationDir, string gmlPath)
        {
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
                return null;
        }
    }
}
