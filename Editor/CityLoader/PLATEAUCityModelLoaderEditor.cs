using System.IO;
using PLATEAU.CityLoader;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.Editor.PlateauWindow.Import.AreaSelect;
using PLATEAU.IO;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityLoader
{
    [CustomEditor(typeof(PLATEAUCityModelLoader))]
    internal class PLATEAUCityModelLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityModelLoader)target;
            if (PlateauEditorStyle.MainButton("範囲選択"))
            {
                AreaSelectorStarter.Start(loader.SourcePathBeforeImport, loader);
            }

            if (PlateauEditorStyle.MainButton("インポート"))
            {
                string path = Path.Combine(loader.SourcePathBeforeImport, "udx/bldg/53392642_bldg_6697_op2.gml").Replace('\\', '/');
                Debug.Log($"loading {path}");
                var task = PLATEAU.CityLoader.Load.CityLoader.Load(
                    // TODO これは仮。ここに設定を正しく渡せるようにする
                    "TestDataSimple/udx/bldg/53392642_bldg_6697_op2.gml",
                    MeshGranularity.PerCityModelArea,
                    2, 2, true, 5,
                    -90, -180, 90, 180
                );
                task.ContinueWithErrorCatch();
            }
            EditorGUILayout.LabelField("インポート前パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
            EditorGUILayout.LabelField("インポート後パス:");
            PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
            EditorGUILayout.LabelField("地域メッシュコード");
            PlateauEditorStyle.MultiLineLabelWithBox(DebugUtil.EnumerableToString(loader.AreaMeshCodes));
        }   
    }
}
