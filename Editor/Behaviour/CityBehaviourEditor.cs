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
        private CityMeshPlacerPresenter cityMeshPlacerPresenter = new CityMeshPlacerPresenter();
        
        public override void OnInspectorGUI()
        {
            CityBehaviour cityBehaviour = (CityBehaviour)target;
            HeaderDrawer.Reset();
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(15);
            
            HeaderDrawer.Draw("3Dモデルをシーンに再配置");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                var metadata = cityBehaviour.CityMetadata;
                if (metadata == null)
                {
                    EditorGUILayout.HelpBox("メタデータがアタッチされていません。", MessageType.Error);
                    return;
                }
                this.foldOutObjPlaceGUI = EditorGUILayout.Foldout(this.foldOutObjPlaceGUI, "再配置画面");
                if (this.foldOutObjPlaceGUI)
                {
                    this.cityMeshPlacerPresenter.Draw(metadata);
                }
            }

        }
    }
}