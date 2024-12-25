using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークのリンクを表すクラス
    /// </summary>
    public class RoadNetworkElementLink : RoadNetworkElement
    {
        public static readonly string IDPrefix = "Link";

        public RoadNetworkElementNode UpNode { get; set; }

        public RoadNetworkElementNode DownNode { get; set; }

        public RoadNetworkElementLink Pair { get; set; }

        public bool IsReverse { get; set; }

        public int RoadNetworkIndex { get; private set; } = -1;

        public List<RoadNetworkElementLane> Lanes = new List<RoadNetworkElementLane>();

        public RoadNetworkElementLink(RoadNetworkContext context, string id, int index) : base(context, CreateID(id))
        {
            RoadNetworkIndex = index;

            // Note: Reverse判定がまだ行われていないためここではレーン生成は行わない
        }

        private static string CreateID(string id)
        {
            var newID = IDPrefix + id;

            return newID;
        }

        public RnDataRoad OriginLink
        {
            get
            {
                if (RoadNetworkIndex < 0)
                {
                    return null;
                }

                return roadNetworkContext.RoadNetworkGetter.GetRoadBases()[RoadNetworkIndex] as RnDataRoad;
            }
        }

        public PLATEAUCityObjectGroup OriginTran
        {
            get
            {
                return OriginLink?.TargetTran;
            }
        }

        private List<PLATEAUCityObjectGroup> runtimeTrans;

        public List<PLATEAUCityObjectGroup> RuntimeTrans
        {
            get
            {
                if (runtimeTrans == null)
                {
                    runtimeTrans = new List<PLATEAUCityObjectGroup>();

                    if (OriginTran.gameObject.activeInHierarchy)
                    {
                        runtimeTrans.Add(OriginTran);
                    }
                    else
                    {
                        // FIXME: OriginTranから分割されたレーンのオブジェクトを検索

                        var trans = new List<PLATEAUCityObjectGroup>();

                        GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>().ToList().ForEach(tran =>
                        {
                            if (tran.name == OriginTran.name && tran.gameObject.activeInHierarchy)
                            {
                                trans.Add(tran);
                            }
                        });

                        foreach (var lane in Lanes)
                        {
                            var geo = lane.GetGeometory();

                            var points = geo.Select(p => roadNetworkContext.GeoReference.Project(new GeoCoordinate(p.Latitude, p.Longitude, 0))).ToList();

                            var point = CalculateMidpoint(points.Select(p => new Vector3((float)p.X, (float)p.Y, (float)p.Z)).ToList());

                            foreach (var tran in trans)
                            {
                                if (IsPointInMesh(point, tran.GetComponent<MeshFilter>().sharedMesh, tran.transform))
                                {
                                    runtimeTrans.Add(tran);
                                }
                            }
                        }
                    }
                }

                return runtimeTrans;
            }
        }

        private Vector3 CalculateMidpoint(List<Vector3> lineString)
        {
            if (lineString == null || lineString.Count == 0)
            {
                return Vector3.zero;
            }

            Vector3 sum = Vector3.zero;

            foreach (var point in lineString)
            {
                sum += point;
            }

            return sum / lineString.Count;
        }

        private bool IsPointInMesh(Vector3 point, Mesh mesh, Transform meshTransform)
        {
            // 点をZX平面に投影
            Vector2 pointProjected = new Vector2(point.x, point.z);

            // メッシュの頂点と三角形インデックスを取得
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // 頂点をワールド座標に変換
            Vector3[] worldVertices = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                worldVertices[i] = meshTransform.TransformPoint(vertices[i]);
            }

            // 各三角形ごとに判定
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // 三角形の各頂点をZX平面に投影
                Vector2 v1 = new Vector2(worldVertices[triangles[i]].x, worldVertices[triangles[i]].z);
                Vector2 v2 = new Vector2(worldVertices[triangles[i + 1]].x, worldVertices[triangles[i + 1]].z);
                Vector2 v3 = new Vector2(worldVertices[triangles[i + 2]].x, worldVertices[triangles[i + 2]].z);

                // 点がこの三角形内にあるかを判定
                if (IsPointInTriangle(pointProjected, v1, v2, v3))
                {
                    return true; // 含まれる
                }
            }

            return false; // すべての三角形で含まれない
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            // 各辺のベクトルと点pへのベクトルを計算
            Vector2 edge1 = v2 - v1;
            Vector2 edge2 = v3 - v2;
            Vector2 edge3 = v1 - v3;

            Vector2 toPoint1 = p - v1;
            Vector2 toPoint2 = p - v2;
            Vector2 toPoint3 = p - v3;

            // 各辺での外積を計算 (z成分のみ必要)
            float cross1 = edge1.x * toPoint1.y - edge1.y * toPoint1.x;
            float cross2 = edge2.x * toPoint2.y - edge2.y * toPoint2.x;
            float cross3 = edge3.x * toPoint3.y - edge3.y * toPoint3.x;

            // 外積の符号が全て同じであれば、点は三角形内にある
            bool hasNegative = (cross1 < 0) || (cross2 < 0) || (cross3 < 0);
            bool hasPositive = (cross1 > 0) || (cross2 > 0) || (cross3 > 0);

            return !(hasNegative && hasPositive);
        }

        public Mesh Mesh
        {
            get
            {
                return OriginTran?.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        public void GenerateLane()
        {
            Lanes = new List<RoadNetworkElementLane>();

            if (OriginLink != null)
            {
                var lanesall = roadNetworkContext.RoadNetworkGetter.GetLanes();

                var lanes = GetOriginLanes();

                for (int i = 0; i < lanes.Count; i++)
                {
                    var lane = lanesall[lanes[i].ID];

                    Lanes.Add(new RoadNetworkElementLane(roadNetworkContext, ID.Replace(IDPrefix, ""), lane, i));
                }
            }
        }

        public List<GeoJSON.Net.Geometry.Position> GetGeometory()
        {
            // 空リンクの場合
            if (Lanes.Count == 0)
            {
                Vector3 upCoord;
                Vector3 downCoord;

                if (UpNode.IsVirtual || UpNode.OriginTran == null)
                {
                    upCoord = UpNode.Coord;
                }
                else
                {
                    var mesh = UpNode.OriginTran.GetComponent<MeshFilter>().sharedMesh;

                    upCoord = mesh.bounds.center;
                }

                if (DownNode.IsVirtual || DownNode.OriginTran == null)
                {
                    downCoord = DownNode.Coord;
                }
                else
                {
                    var mesh = UpNode.OriginTran.GetComponent<MeshFilter>().sharedMesh;

                    downCoord = mesh.bounds.center;
                }

                var upGeoCood = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(upCoord.x, upCoord.y, upCoord.z));
                var downGeoCood = roadNetworkContext.GeoReference.Unproject(new PlateauVector3d(downCoord.x, downCoord.y, downCoord.z));

                return new List<GeoJSON.Net.Geometry.Position>() { new GeoJSON.Net.Geometry.Position(upGeoCood.Latitude, upGeoCood.Longitude), new GeoJSON.Net.Geometry.Position(downGeoCood.Latitude, downGeoCood.Longitude) };
            }

            return Lanes[IsReverse ? 0 : Lanes.Count - 1].GetGeometory(false);
        }

        public double GetLaneLength()
        {
            if (Lanes.Count == 0)
            {
                return 0;
            }

            // 一番長いレーンの長さを返す
            return Lanes.Max(lane => lane.GetLength());
        }

        public List<RnID<RnDataLane>> GetOriginLanes()
        {
            List<RnID<RnDataLane>> lanes = new List<RnID<RnDataLane>>();

            var laneAll = roadNetworkContext.RoadNetworkGetter.GetLanes();

            foreach (var lane in OriginLink.MainLanes)
            {
                if (!lane.IsValid) continue;

                if (IsReverse == laneAll[lane.ID].IsReverse)
                {
                    lanes.Add(lane);
                }
            }

            return lanes;
        }

        public int GetLaneNum()
        {
            return GetOriginLanes().Count;
        }
    }
}