using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityLoader;
using PLATEAU.CityLoader.Load;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Editor.CityLoader.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.IO;
using PLATEAU.Udx;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using UnityEditor;

namespace PLATEAU.Editor.CityLoader
{
    /// <summary>
    /// 都市モデルの読み込み設定をするGUIをインスペクタに表示し、
    /// ボタン操作に応じて選択画面の表示や読み込みを行います。
    /// </summary>
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
                        string destPath = CityFilesCopy.ToStreamingAssets(loader.SourcePathBeforeImport, loader.CityLoadConfig);
                        loader.SourcePathAfterImport = destPath;
                        var gmlPathsDict = loader.CityLoadConfig.SearchMatchingGMLList(destPath, out _);
                        var task = LoadGmlsAsync(gmlPathsDict, loader.CityLoadConfig);
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
        
        private static async Task LoadGmlsAsync(Dictionary<PredefinedCityModelPackage, List<string>> gmlPathsDict, CityLoadConfig config)
        {
            foreach (var package in gmlPathsDict.Keys)
            {
                var packageConf = config.GetConfigForPackage(package);
                foreach (string gmlPath in gmlPathsDict[package])
                {
                    await PLATEAU.CityLoader.Load.CityLoader.Load(
                        // TODO これは仮。ここに後半の設定を正しく渡せるようにする。
                        gmlPath,
                        packageConf.meshGranularity,
                        packageConf.minLOD, packageConf.maxLOD, packageConf.includeTexture, 5,
                        -90, -180, 90, 180
                    );
                }
            }
        }
    }
}
