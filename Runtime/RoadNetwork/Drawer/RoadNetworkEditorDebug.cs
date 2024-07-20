using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
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

            [Serializable]
            public class LaneWidthEdit : WithButton
            {
                public bool moveLeft = true;
                public float width = 0f;
            }
            //public LaneWidthEdit widthEdit = new LaneWidthEdit();

            ulong laneNormalId = ulong.MaxValue;
            private Dictionary<RnLaneWayDir, List<Vector2>> vertexNormals = new Dictionary<RnLaneWayDir, List<Vector2>>();
            public float rightWayPos = 0f;
            public float leftWayPos = 0f;


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

                if (laneNormalId != targetLaneId)
                {
                    laneNormalId = targetLaneId;
                    vertexNormals[RnLaneWayDir.Right] = lane.RightWay?.GetVertexNormals().Select(v => v.Xz()).ToList() ?? new List<Vector2>();
                    vertexNormals[RnLaneWayDir.Left] = lane.LeftWay?.GetVertexNormals().Select(v => v.Xz()).ToList() ?? new List<Vector2>();
                }
                if (rightWayPos != 0f && lane.RightWay != null)
                {
                    for (var i = 0; i < lane.RightWay.Count; i++)
                    {
                        lane.RightWay.GetPoint(i).Vertex += (vertexNormals[RnLaneWayDir.Right][i]).Xay() * rightWayPos;
                    }

                    rightWayPos = 0f;
                }

                if (leftWayPos != 0f && lane.LeftWay != null)
                {
                    for (var i = 0; i < lane.LeftWay.Count; i++)
                    {
                        lane.LeftWay.GetPoint(i).Vertex += (vertexNormals[RnLaneWayDir.Left][i]).Xay() * leftWayPos;
                    }

                    leftWayPos = 0f;
                }

                if (splitEdit.Button())
                {
                    if (lane.Parent is RnLink link)
                    {
                        var lanes = lane.SplitLane(splitEdit.splitNum, true);
                        foreach (var item in lanes)
                        {
                            var l = item.Key;
                            var parent = l.Parent as RnLink;
                            parent?.ReplaceLane(l, item.Value);
                        }
                    }
                }

                //if (widthEdit.Button())
                //{
                //    lane.SetLaneWidth(widthEdit.width, widthEdit.moveLeft);
                //}
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