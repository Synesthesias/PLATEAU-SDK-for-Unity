using PLATEAU.Behaviour;
using PLATEAU.Editor.CityImport;
using PLATEAU.Editor.EditorWindowCommon;
using UnityEditor;

namespace PLATEAU.Editor.Behaviour
{
    [CustomEditor(typeof(CityBehaviour))]
    internal class CityBehaviourEditor : UnityEditor.Editor
    {
        private bool foldOutObjPlaceGUI;
        private ScenePlacementGUI scenePlacementGUI = new ScenePlacementGUI();
        
        public override void OnInspectorGUI()
        {
            CityBehaviour cityBehaviour = (CityBehaviour)target;
            HeaderDrawer.Reset();
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(15);
            
            HeaderDrawer.Draw("3Dモデルをシーンに再配置");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var metaData = cityBehaviour.CityMetaData;
                var importConfig = metaData.cityImportConfig;
                var placeConfig = importConfig.scenePlacementConfig;
                this.foldOutObjPlaceGUI = EditorGUILayout.Foldout(this.foldOutObjPlaceGUI, "再配置画面");
                if (this.foldOutObjPlaceGUI)
                {
                    this.scenePlacementGUI.Draw(placeConfig);
                    if (PlateauEditorStyle.MainButton("シーンにモデルを再配置"))
                    {
                        var availableObjs = importConfig.generatedObjFiles;
                        string rootDirName = importConfig.rootDirName;
                        CityMeshPlacerToScene.Place(placeConfig, availableObjs, rootDirName, metaData);
                    }
                }
            }

        }
    }
}