using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Mesh;
using System.Collections.Generic;
using UnityEditor;
using static PLATEAU.RoadNetwork.Factory.RsFactoryMidStageData;

namespace PLATEAU.Editor.RoadNetwork.CityObject
{
    public class SubDividedCityObjectDebugEditorWindow : EditorWindow
    {
        public interface IInstanceHelper
        {
            // グラフ取得
            SubDividedCityObjectWrap GetCityObjects();

            bool IsTarget(SubDividedCityObject cityObject);

            HashSet<SubDividedCityObject> TargetCityObjects { get; }

            // モデル作成する
            void CreateRnModel();

            RGraphDrawerDebug GetDrawer();

            void CreateTranMesh();
        }

        private const string WindowName = "SubDividedCityObject Editor";

        public IInstanceHelper InstanceHelper { get; set; }

        public void EditSubDividedCityObject(SubDividedCityObject e)
        {
            if (e == null)
                return;
            RnEditorUtil.TargetToggle($"{e.Name}", InstanceHelper.TargetCityObjects, e);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.EnumFlagsField("SelfRoadType", e.SelfRoadType);
                EditorGUILayout.EnumFlagsField("ParentRoadType", e.ParentRoadType);
                foreach (var root in e.CityObjects.rootCityObjects)
                {
                    RnEditorUtil.Separator();
                    EditorGUILayout.LabelField(root.GmlID);
                    EditorGUILayout.EnumFlagsField("Type", root.GetRoadType());
                }
            }

            RnEditorUtil.Separator();
            EditorGUILayout.LabelField("Children");
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var child in e.Children)
                {
                    RnEditorUtil.Separator();
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditSubDividedCityObject(child);
                    }
                }
            }

        }

        /// <Summary>
        /// ウィンドウのパーツを表示します。
        /// </Summary>
        private void OnGUI()
        {
            if (InstanceHelper == null)
                return;

            var cityObjects = InstanceHelper.GetCityObjects();
            if (cityObjects == null || cityObjects.cityObjects == null)
            {
                return;
            }


            RnEditorUtil.Separator();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("CityObjects", cityObjects.cityObjects.Count);
            }

            RnEditorUtil.Separator();
            foreach (var cog in cityObjects.cityObjects)
            {
                if (InstanceHelper.IsTarget(cog) || InstanceHelper.TargetCityObjects.Contains(cog))
                {
                    RnEditorUtil.Separator();
                    EditSubDividedCityObject(cog);
                }
            }
        }


        /// <summary>
        /// ウィンドウを取得する、存在しない場合に生成する
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static SubDividedCityObjectDebugEditorWindow OpenWindow(IInstanceHelper instance, bool focus)
        {
            var ret = GetWindow<SubDividedCityObjectDebugEditorWindow>(WindowName, focus);
            ret.InstanceHelper = instance;
            return ret;
        }

    }
}