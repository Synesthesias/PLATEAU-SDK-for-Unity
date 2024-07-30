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
            private Dictionary<RnDir, List<Vector2>> vertexNormals = new Dictionary<RnDir, List<Vector2>>();
            public float rightWayPos = 0f;
            public float leftWayPos = 0f;


            public void Update(RnModel model)
            {
                if (model == null)
                    return;

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
                    vertexNormals[RnDir.Right] = lane.RightWay?.GetVertexNormals().Select(v => v.Xz()).ToList() ?? new List<Vector2>();
                    vertexNormals[RnDir.Left] = lane.LeftWay?.GetVertexNormals().Select(v => v.Xz()).ToList() ?? new List<Vector2>();
                }
                if (rightWayPos != 0f && lane.RightWay != null)
                {
                    for (var i = 0; i < lane.RightWay.Count; i++)
                    {
                        lane.RightWay.GetPoint(i).Vertex += (vertexNormals[RnDir.Right][i]).Xay() * rightWayPos;
                    }

                    rightWayPos = 0f;
                }

                if (leftWayPos != 0f && lane.LeftWay != null)
                {
                    for (var i = 0; i < lane.LeftWay.Count; i++)
                    {
                        lane.LeftWay.GetPoint(i).Vertex += (vertexNormals[RnDir.Left][i]).Xay() * leftWayPos;
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

        [Serializable]
        private class LinkEdit
        {
            [SerializeField]
            private ulong targetLinkId = ulong.MaxValue;

            [Serializable]
            public class ShowInfo
            {
                [NonSerialized]
                public ulong targetLinkId = ulong.MaxValue;

                public int prevId = -1;

                public int nextId = -1;

                public int nowLeftLaneCount = -1;
                public int nowRightLaneCount = -1;

                // 左側レーン数
                public int leftLaneCount = -1;
                // 右側レーン数
                public int rightLaneCount = -1;

                public bool changeLaneCount = false;

                // 中央分離帯幅
                public float medianWidth = 0;

            }
            public ShowInfo showInfo = new ShowInfo();

            public void Update(RnModel model)
            {
                if (model == null)
                    return;
                var link = model.Links.FirstOrDefault(l => l.DebugMyId == targetLinkId);
                if (link == null)
                    return;
                var linkGroup = link.CreateLinkGroupOrDefault();
                if (linkGroup == null)
                    return;
                if (showInfo.targetLinkId != targetLinkId)
                {
                    showInfo.targetLinkId = targetLinkId;
                    showInfo.leftLaneCount = linkGroup.GetLeftLaneCount();
                    showInfo.rightLaneCount = linkGroup.GetRightLaneCount();
                    showInfo.medianWidth = link.GetMedianWidth();
                }

                showInfo.prevId = (int)(link.Prev?.DebugMyId ?? ulong.MaxValue);
                showInfo.nextId = (int)(link.Next?.DebugMyId ?? ulong.MaxValue);
                showInfo.nowLeftLaneCount = link.GetLeftLaneCount();
                showInfo.nowRightLaneCount = link.GetRightLaneCount();

                if (showInfo.changeLaneCount)
                {
                    if (showInfo.leftLaneCount != linkGroup.GetLeftLaneCount())
                    {
                        linkGroup.SetLeftLaneCount(showInfo.leftLaneCount);
                    }
                    if (showInfo.rightLaneCount != linkGroup.GetRightLaneCount())
                    {
                        linkGroup.SetRightLaneCount(showInfo.rightLaneCount);
                    }
                    showInfo.changeLaneCount = false;
                }
                if (Math.Abs(showInfo.medianWidth - link.GetMedianWidth()) > 1e-1f)
                {
                    link.SetMedianWidth(showInfo.medianWidth);
                }
            }
        }
        [SerializeField] LinkEdit linkEdit = new LinkEdit();

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
            if (model == null)
                return;
            laneEdit.Update(model);
            linkEdit.Update(model);
        }
    }
}