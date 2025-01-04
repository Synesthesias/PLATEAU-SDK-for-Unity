using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 交差点編集のシーンビュー上のGUIを表示します。
    /// </summary>
    public class IntersectionEditSceneViewGui
    {
        private RnIntersection targetIntersection;
        private readonly IntersectionTrackEditor trackEditor = new();
        private readonly RoadNetworkEditTarget editTarget;
        
        public IntersectionEditSceneViewGui(RoadNetworkEditTarget target)
        {
            editTarget = target;
        }
        
        public void Update()
        {
            if (targetIntersection == null) return;
            trackEditor.Draw(editTarget, targetIntersection);
        }

        public void Terminate()
        {
            targetIntersection = null;
        }
        

        public void SetupIntersection(RnIntersection data)
        {
            targetIntersection = data;
        }
    }
}