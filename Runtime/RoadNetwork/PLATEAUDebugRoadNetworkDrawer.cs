using JetBrains.Annotations;
using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace PLATEAU.Packages.PlateauUnitySDK.Runtime.RoadNetwork
{
    public class PLATEAUDebugRoadNetworkDrawer : MonoBehaviour
    {
        public float cellSize = 0.01f;

        [Serializable]
        public class Cell
        {
            public Vector3Int CellNo;
            public List<Lane> Lanes = new List<Lane>();
        }

        public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();

        public List<Cell> Cell2Groups = new List<Cell>();

        public List<Lane> Lanes = new List<Lane>();

        void Update()
        {
            //foreach (var t in targets ?? new List<PLATEAUCityObjectGroup>())
            //{
            //    Draw(t);
            //}

            foreach (var l in Lanes)
            {
                var c = l.GetDrawCenterPoint();
                // Debug.DrawLine(c, c + Vector3.up * 1);
                // foreach (var con in l.Connected)
                //     DebugUtil.DrawArrow(c, con.GetDrawCenterPoint());

                foreach (var way in l.Ways)
                {
                    DebugUtil.DrawArrows(way.Vertices, false);
                }
            }
        }

        public void Draw(PLATEAUCityObjectGroup cityObjectGroup)
        {
            var collider = cityObjectGroup.GetComponent<MeshCollider>();
            var cMesh = collider.sharedMesh;
            var isClockwise = PolygonUtil.IsClockwise(cMesh.vertices.Select(v => new Vector2(v.x, v.y)));
            if (isClockwise)
            {
                DebugUtil.DrawArrows(cMesh.vertices.Select(v => v + Vector3.up * 0.2f));
            }
            else
            {
                DebugUtil.DrawArrows(cMesh.vertices.Reverse().Select(v => v + Vector3.up * 0.2f));
            }
        }

        public void CreateNetwork()
        {
            Cell2Groups.Clear();
            // 頂点の一致判定のためにセル単位に切り捨て
            // #TODO : 近いけど隣のセルになる場合を考慮

            var cSize = Vector3.one * cellSize;
            Lanes.Clear();
            foreach (var cityObjectGroup in targets)
            {
                var comp = cityObjectGroup.GetComponent<MeshCollider>();
                if (!comp)
                    continue;
                var lane = new Lane { CityObjectGroup = cityObjectGroup };
                if (PolygonUtil.IsClockwise(comp.sharedMesh.vertices.Select(x => x.Xz())))
                {
                    lane.Vertices.AddRange(comp.sharedMesh.vertices);
                }
                else
                {
                    lane.Vertices.AddRange(comp.sharedMesh.vertices.Reverse());
                }

                foreach (var v in lane.Vertices)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var c = Cell2Groups.FirstOrDefault(x => x.CellNo == cellNo);
                    if (c == null)
                    {
                        c = new Cell
                        {
                            CellNo = cellNo
                        };
                        Cell2Groups.Add(c);
                    }
                    if (c.Lanes.Contains(lane) == false)
                        c.Lanes.Add(lane);
                }

                Lanes.Add(lane);
            }

            foreach (var c in Cell2Groups)
            {
                if (c.Lanes.Count <= 1)
                    continue;
                foreach (var a in c.Lanes)
                {
                    foreach (var b in c.Lanes)
                    {
                        if (a == b)
                            continue;
                        if (a.Connected.Contains(b) == false)
                            a.Connected.Add(b);
                        if (b.Connected.Contains(a) == false)
                            b.Connected.Add(a);
                    }
                }
            }

            foreach (var lane in Lanes)
            {
                List<Lane> GetNeighborLane(Vector3 v)
                {
                    var cellNo = v.RevScaled(cSize).ToVector3Int();
                    var c = Cell2Groups.First(x => x.CellNo == cellNo);
                    return c.Lanes.Where(l => l != lane).ToList();
                }
                var neighbors = lane.Vertices.Select(GetNeighborLane).ToList();

                // #TODO : 同じレーンが偶数回出てくる前提
                //       : 

                // 隣接レーンが一つもない場合は孤立
                if (neighbors.Any(n => n.Any()) == false)
                {
                    var way = new Way();
                    way.Vertices.AddRange(lane.Vertices);
                    lane.Ways.Add(way);
                    continue;
                }

                var startIndex = Enumerable.Range(0, neighbors.Count).First(i => neighbors[i].Any());

                var nextIndex = Enumerable.Range(startIndex + 1, neighbors.Count)
                    .Select(i => i % neighbors.Count)
                    .First(i => neighbors[i] != null);

                if (neighbors[startIndex] != neighbors[nextIndex])
                    startIndex = nextIndex;

                var v1 = lane.Vertices[startIndex];
                var currentWay = new Way();
                currentWay.Vertices.Add(v1);
                var currentIndex = startIndex;
                List<int> indices = new List<int> { startIndex };
                foreach (var tmp in Enumerable.Range(1, neighbors.Count))
                {
                    var i = (tmp + startIndex) % neighbors.Count;
                    currentWay.Vertices.Add(lane.Vertices[i]);
                    indices.Add(i);

                    var toNeighbor = neighbors[i];
                    // 隣接している点があったら切り替え
                    if (toNeighbor.Any() == false)
                        continue;

                    var fromIndex = indices[0];

                    currentWay = new Way();
                    currentWay.Vertices.Add(lane.Vertices[i]);

                    var fromNeighbors = neighbors[fromIndex];
                    var neighbor = toNeighbor.FirstOrDefault(n => fromNeighbors.Contains(n));

                    // 共通の隣接レーンがあるとエッジ
                    if (neighbor != null)
                    {
                        var edge = new Edge
                        {
                            NeighborLane = neighbor
                        };
                        edge.Vertices.AddRange(indices.Select(a => lane.Vertices[a]));
                        lane.Edges.Add(edge);
                    }
                    else
                    {
                        var way = new Way();
                        way.PrevLanes.AddRange(fromNeighbors);
                        way.NextLanes.AddRange(toNeighbor);
                        way.Vertices.AddRange(indices.Select(a => lane.Vertices[a]));
                        lane.Ways.Add(way);
                    }
                    indices = new List<int>();
                }
            }

        }
    }
}