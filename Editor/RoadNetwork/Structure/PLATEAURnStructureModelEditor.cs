using PLATEAU.Editor.RoadNetwork.EditingSystem;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLATEAU.Editor.RoadNetwork.Structure
{
    [CustomEditor(typeof(PLATEAURnStructureModel))]
    public class PLATEAURnStructureModelEditor : UnityEditor.Editor
    {
        private class RnModelInstanceHelper : RnModelDebugEditorWindow.IInstanceHelper
        {
            private PLATEAURnStructureModel target;

            private PLATEAURnModelDrawerDebug Drawer
            {
                get
                {
                    if (!target)
                        return null;
                    return target.gameObject.GetOrAddComponent<PLATEAURnModelDrawerDebug>();
                }
            }

            public RnModelInstanceHelper(PLATEAURnStructureModel target)
            {
                this.target = target;
            }

            public RnModel GetModel()
            {
                return target.RoadNetwork;
            }

            public HashSet<object> InVisibleObjects
            {
                get
                {
                    if (!Drawer)
                        return new HashSet<object>();
                    return Drawer.InVisibleObjects;
                }
            }

            public HashSet<object> SelectedObjects
            {
                get
                {
                    if (!Drawer)
                        return new HashSet<object>();
                    return Drawer.SelectedObjects;
                }
            }

            public bool IsSceneSelected(RnRoadBase roadBase)
            {
                return roadBase.TargetTrans.Any(RnEx.IsEditorSceneSelected);
            }
        }


        public void OnSceneGUI()
        {
            // RoadNetworkを所持しているオブジェクトに表示するGUIシステムを更新する処理
            var editor = RoadNetworkEditingSystem.SingletonInstance;
            if (editor == null)
                return;

            editor.OnSceneGUI(target as PLATEAURnStructureModel);
            
        }

        public override void OnInspectorGUI()
        {
            var obj = target as PLATEAURnStructureModel;
            if (!obj)
                return;

            base.OnInspectorGUI();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Serialize"))
                    obj.Serialize();

                if (GUILayout.Button("Deserialize"))
                    obj.Deserialize();
            }

            if (GUILayout.Button("RnModel Debug Editor"))
                RnModelDebugEditorWindow.OpenWindow(new RnModelInstanceHelper(obj), true);
        }

        // https://docs.unity3d.com/ja/2020.3/ScriptReference/InitializeOnLoadMethodAttribute.html
        [InitializeOnLoadMethod]
        public static void RegisterEditorCallbacks()
        {
            // Scene保存時にPLATEAURnStructureModelのシリアライズを行う
            // https://docs.unity3d.com/ScriptReference/SceneManagement.EditorSceneManager-sceneSaving.html
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            // Scene保存時にPLATEAURnStructureModelのシリアライズを行う
            PLATEAURnStructureModel.OnSceneSaving(scene, path);
        }
    }
}