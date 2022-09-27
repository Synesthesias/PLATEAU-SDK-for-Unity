using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityLoader;
using PLATEAU.CityLoader.Load;
using PLATEAU.CityLoader.Setting;
using PLATEAU.Editor.CityLoader.AreaSelector;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.ProgressDisplay;
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
    [CustomEditor(typeof(PLATEAUCityLoaderBehaviour))]
    internal class PLATEAUCityLoaderBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityLoaderBehaviour)target;
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
                        CityImporter.Import(loader, ProgressDisplayWindow.Open());
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
    }
}
