using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PLATEAU.CityLoader;
using PLATEAU.CityLoader.Load;
using PLATEAU.Editor.CityLoader.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.IO;
using PLATEAU.Udx;
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
            bool isAreaSelectComplete;
            
            HeaderDrawer.Reset();
            HeaderDrawer.Draw("範囲選択");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (PlateauEditorStyle.MainButton("範囲選択"))
                {
                    AreaSelectorStarter.Start(loader.SourcePathBeforeImport, loader);
                }
                isAreaSelectComplete = loader.AreaMeshCodes != null && loader.AreaMeshCodes.Length > 0;
                EditorGUILayout.LabelField(isAreaSelectComplete ? "範囲選択 : 済" : "範囲選択 : 未");
            }
            

            

            if (isAreaSelectComplete)
            {
                CityLoadConfigGUI.Draw(loader.CityLoadConfig);
                
                HeaderDrawer.Draw("インポート");
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    if (PlateauEditorStyle.MainButton("インポート"))
                    {
                        // string path = Path.Combine(loader.SourcePathBeforeImport, "udx/bldg/53392642_bldg_6697_op2.gml").Replace('\\', '/');
                        // Debug.Log($"loading {path}");
                        string destPath = CityFilesCopy.ToStreamingAssets(loader.SourcePathBeforeImport, loader.CityLoadConfig);
                        loader.SourcePathAfterImport = destPath;
                        var gmlPaths = loader.CityLoadConfig.SearchMatchingGMLList(destPath, out _);
                        var task = LoadGmlsAsync(gmlPaths);
                        task.ContinueWithErrorCatch();
                    }
                }
            }
            
            HeaderDrawer.Draw("詳細データ");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUILayout.LabelField("インポート前パス:");
                PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
                EditorGUILayout.LabelField("インポート後パス:");
                PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
                EditorGUILayout.LabelField("地域メッシュコード");
                PlateauEditorStyle.MultiLineLabelWithBox(DebugUtil.EnumerableToString(loader.AreaMeshCodes));
            }
            
        }

        private async Task LoadGmlsAsync(ICollection<string> gmlPaths)
        {
            foreach (var gmlPath in gmlPaths)
            {
                await PLATEAU.CityLoader.Load.CityLoader.Load(
                    // TODO これは仮。ここに設定を正しく渡せるようにする
                    gmlPath,
                    MeshGranularity.PerCityModelArea,
                    2, 2, true, 5,
                    -90, -180, 90, 180
                );
            }
        }
    }
}
