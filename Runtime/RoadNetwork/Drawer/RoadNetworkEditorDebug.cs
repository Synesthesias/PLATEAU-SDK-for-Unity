using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    /// <summary>
    /// Inspectorだけで実装するRoadNetworkの簡易編集クラス
    /// </summary>
    [Serializable]
    public class RoadNetworkEditorDebug
    {
        [Serializable]
        public class WithButton
        {
            [SerializeField]
            private bool execute = false;

            public bool Button()
            {
                var ret = execute;
                execute = false;
                return ret;
            }
        }
        [Serializable]
        private class LaneEdit
        {
            [SerializeField]
            private ulong targetLaneId = ulong.MaxValue;

            [Serializable]
            public class ShowInfo
            {
                public float width = -1f;
                public ulong prevBorderId = ulong.MaxValue;
                public ulong nextBorderId = ulong.MaxValue;
            }
            public ShowInfo showInfo = new ShowInfo();

            [Serializable]
            public class LaneSplitEdit : WithButton
            {
                public int splitNum = 2;
            }
            public LaneSplitEdit splitEdit = new LaneSplitEdit();

            public void Update(RnModel model)
            {
                var lane = model.CollectAllLanes().FirstOrDefault(l => l.DebugMyId == targetLaneId);
                if (lane == null)
                    return;
                foreach (var item in GeoGraphEx.GetEdges(lane.GetVertices().Select(p => p.Vertex), true))
                {
                    DebugEx.DrawArrow(item.Item1, item.Item2, bodyColor: Color.red);
                }
                showInfo.width = lane.CalcWidth();
                showInfo.prevBorderId = lane.PrevBorder?.LineString?.DebugMyId ?? ulong.MaxValue;
                showInfo.nextBorderId = lane.NextBorder?.LineString?.DebugMyId ?? ulong.MaxValue;

                if (splitEdit.Button())
                {
                    if (lane.Parent is RnLink link)
                    {
                        var lanes = lane.SplitLane(splitEdit.splitNum, true);
                        foreach (var item in lanes)
                        {
                            var l = item.Key;
                            var parent = l.Parent as RnLink;
                            parent.RemoveLane(l);
                            foreach (var newLane in item.Value)
                                parent.AddMainLane(newLane);
                        }
                    }
                }
            }
        }

        [SerializeField]
        private LaneEdit laneEdit = new LaneEdit();

        [SerializeField]
        public class WayEdit
        {

        }


        [SerializeField]
        private ulong targetWayId = ulong.MaxValue;

        [SerializeField]
        private ulong targetPointId = ulong.MaxValue;

        public void OnInspectorGUI(RnModel model)
        {
            laneEdit.Update(model);
        }
    }
}