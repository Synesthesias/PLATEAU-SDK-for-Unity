using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// ギズモの描画システム
    /// ギズモ描画を行う処理を生成する。
    /// 実際の描画はMonoBehaviourを継承したギズモ描画クラスを用意したのでそこで行う。
    /// </summary>
    internal class EditingSystemGizmos
    {
        /// <summary>
        /// 描画コマンド群
        /// </summary>
        private List<System.Action> drawFuncs = new List<Action>();

        // 交差間の繋がり(RnRoadGroup)を表現する頂点リスト
        private List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();

        // 選択中のwayを表現する頂点リスト
        private List<Vector3> selectingWay = new List<Vector3>();

        // 左側、右側車線のwayを表現する頂点リスト
        private List<List<Vector3>> leftLaneWayList = new List<List<Vector3>>();
        private List<List<Vector3>> rightLaneWayList = new List<List<Vector3>>();

        // 中央分離帯
        private List<List<Vector3>> medianWayList = new List<List<Vector3>>();

        // 歩道
        private List<List<Vector3>> sideWalks = new List<List<Vector3>>();

        private List<RnWay> mainLaneCenterWay = new List<RnWay>();

        // 選択中のwayをスライドした時の結果表示
        private List<List<Vector3>> slideDummyWayList = new List<List<Vector3>>();

        // 交差点の外周
        private List<List<Vector3>> intersectionOutline = new List<List<Vector3>>();
        // 交差点と道路の境界
        private List<List<Vector3>> intersectionBorder = new List<List<Vector3>>();

        // それぞれのwayの色

        private readonly Color selectingColorOffset = new Color(0.2f, 0.2f, 0.2f, 0);
        private readonly Color dummyColorOffset = new Color(-0.2f, -0.2f, -0.2f, 0);

        private readonly Color connectionColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
        private readonly Color selectingWayColor = Color.red;
                
        private readonly Color leftSideWayColor = Color.yellow;
        private readonly Color rightSideWayColor = Color.green;
        private readonly Color medianWayColor = Color.blue - new Color(0.1f, 0.1f, 0, 0);
                
        private readonly Color guideWayColor = Color.black;
        private readonly Color sideWalkColor = Color.magenta;
        private readonly Color slideDummyWayColor = Color.red + new Color(-0.2f, -0.2f, -0.2f, 0);
                
        private readonly Color intersectionOutlineColor = Color.gray;
        private readonly Color intersectionBorderColor = Color.gray + new Color(-0.2f, -0.2f, -0.2f, 0);

        private readonly Color mainLaneCenterWayColor = Color.cyan + new Color(0, -0.4f, -0.4f, 0);

        public EditingSystemGizmos()
        {

        }

        public void Clear()
        {
            drawFuncs.Clear();
            intersectionConnectionLinePairs.Clear();
            selectingWay.Clear();
            leftLaneWayList.Clear();
            rightLaneWayList.Clear();
            medianWayList.Clear();
            sideWalks.Clear();
            slideDummyWayList.Clear();
            intersectionOutline.Clear();
            intersectionBorder.Clear();
            mainLaneCenterWay.Clear();
        }

        /// <summary>
        /// 描画コマンド生成に必要なデータを更新する
        /// </summary>
        /// <param name="selectingElement"></param>
        /// <param name="highLightWay"></param>
        /// <param name="linkGroupEditorData"></param>
        /// <param name="slideDummyWay"></param>
        public void Update(
            object selectingElement,
            WayEditorData highLightWay,
            EditorDataList<EditorData<RnRoadGroup>> linkGroupEditorData,
            RnWay slideDummyWay)
        {
            if (linkGroupEditorData.TryGetCache("linkGroup", out IEnumerable<RoadGroupEditorData> eConn) == false)
            {
                Assert.IsTrue(false);
                return;
            }
            List<RoadGroupEditorData> connections = eConn.ToList();
            connections.Remove(null);

            // 仮　専用ノード間の繋がりを描画
            intersectionConnectionLinePairs.Clear();
            var pts = intersectionConnectionLinePairs;
            pts.Capacity = (connections.Count + 3) * 2;
            foreach (var item in connections)
            {
                if (item == null)
                    continue;

                var roadGroup = selectingElement as EditorData<RnRoadGroup>;
                if (item.RoadGroup == roadGroup)
                {
                    continue;
                }
                if (item.CacheRoadPosList == null)
                {
                    item.CacheRoadPosList = new List<Vector3>(item.ConnectionLinks.Count * 2);

                    foreach (var link in item.ConnectionLinks)
                    {
                        var allLanes = link.AllLanes.GetEnumerator();
                        Vector3 prevBorderPos = Vector3.zero;
                        Vector3 nextBorderPos = Vector3.zero;

                        if (allLanes.MoveNext())
                        {
                            var lane = allLanes.Current;
                            var prevpoints = lane.PrevBorder?.Points;
                            var nextpoints = lane.NextBorder?.Points;

                            if (prevpoints == null || nextpoints == null)   // 独立している道路がある場合
                            {
                                continue;
                            }

                            if (CalcCenterPos(prevpoints, out prevBorderPos) &&
                                CalcCenterPos(nextpoints, out nextBorderPos))
                            {
                                item.CacheRoadPosList.Add(prevBorderPos);
                                item.CacheRoadPosList.Add(nextBorderPos);
                            }
                        }
                    }
                }
                foreach (var p in item.CacheRoadPosList)
                {
                    pts.Add(p);
                }

            }

            // RoadGroupが選択されている
            rightLaneWayList.Clear();
            leftLaneWayList.Clear();
            sideWalks.Clear();
            medianWayList.Clear();
            if (selectingElement is EditorData<RnRoadGroup> roadGroupEditorData)
            {

                // 歩道の描画
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var sideWalk in road.SideWalks)
                    {
                        sideWalks.Add(sideWalk.OutsideWay.ToList());
                    }
                }

                var laneGroup = new LaneGroupEditorData(roadGroupEditorData.Ref);


                var wayEditorDataList = roadGroupEditorData.ReqSubData<WayEditorDataList>().Raw;

                // 車線のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.Main)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }
                        
                    var parent = wayEditorData.ParentLane;
                    Debug.Assert(parent != null);
                    if (parent.IsReverse == false)
                    {
                        leftLaneWayList.Add(way);
                    }
                    else
                    {
                        rightLaneWayList.Add(way);
                    }
                }

                // 車線の向きを描画
                mainLaneCenterWay.Clear();
                foreach (var road in roadGroupEditorData.Ref.Roads)
                {
                    foreach (var lane in road.MainLanes)
                    {
                        var centerWay = lane.CreateCenterWay();
                        if (centerWay == null)
                            continue;
                        mainLaneCenterWay.Add(centerWay);
                    }
                }



                // 歩道のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.SideWalk)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }

                    sideWalks.Add(way);
                }

                // 中央分離帯のwayを描画
                foreach (var wayEditorData in wayEditorDataList)
                {
                    // 選択中のwayは別の描画処理で対応するためスキップ
                    if (selectingElement == wayEditorData)
                    {
                        continue;
                    }

                    if (wayEditorData.Type != WayEditorData.WayType.Median)
                    {
                        continue;
                    }

                    var way = new List<Vector3>(wayEditorData.Ref.Points.Count());
                    var points = wayEditorData.Ref.Points;
                    foreach (var p in points)
                    {
                        way.Add(p);
                    }

                    medianWayList.Add(way);
                }



            }

            intersectionConnectionLinePairs = pts;

            selectingWay.Clear();
            if (highLightWay != null)
            {
                if (highLightWay is WayEditorData selectingWayData)
                {
                    selectingWay.AddRange(selectingWayData.Ref.ToList());
                }
            }

            slideDummyWayList.Clear();
            if (slideDummyWay != null)
            {
                slideDummyWayList.Add(slideDummyWay.ToList());
            }

            intersectionBorder.Clear();
            var intersectionEditorData = selectingElement as EditorData<RnIntersection>;
            if (intersectionEditorData != null)
            {
                foreach (var neighbor in intersectionEditorData.Ref.Neighbors)
                {
                    if (neighbor.Border != null)
                        intersectionOutline.Add(neighbor.Border.ToList());
                }
            }

        }


        /// <summary>
        /// 描画コマンドの生成
        /// </summary>
        /// <returns></returns>
        public List<Action> BuildDrawCommands()
        {
            drawFuncs.Clear();

            foreach (var sideWalk in sideWalks)
            {
                if (sideWalk == null)
                {
                    continue;
                }
                var sideWalkList = sideWalk.ToArray();
                drawFuncs.Add(() =>
                {
                    Gizmos.color = sideWalkColor;
                    Gizmos.DrawLineStrip(sideWalkList, false);
                });
            }

            var nParis = intersectionConnectionLinePairs.Count;
            if (nParis >= 2 && nParis % 2 == 0)
            {
                drawFuncs.Add(() =>
                {
                    Gizmos.color = connectionColor;
                    Gizmos.DrawLineList(intersectionConnectionLinePairs.ToArray());
                });
            }

            AddDrawFunc(ref drawFuncs, leftLaneWayList, leftSideWayColor);
            AddDrawFunc(ref drawFuncs, rightLaneWayList, rightSideWayColor);

            // 選択中のwayを描画
            if (selectingWay.Count >= 2)
            {
                drawFuncs.Add(() =>
                {
                    Gizmos.color = selectingWayColor;
                    {
                        Gizmos.DrawLineStrip(selectingWay.ToArray(), false);
                    }
                });
            }

            AddDrawFunc(ref drawFuncs, slideDummyWayList, slideDummyWayColor);

            AddDrawFunc(ref drawFuncs, intersectionOutline, intersectionOutlineColor);

            AddDrawFunc(ref drawFuncs, intersectionBorder, intersectionBorderColor);

            AddDrawFunc(ref drawFuncs, medianWayList, medianWayColor);

            if (mainLaneCenterWay.Count > 0)
            {
                drawFuncs.Add(()=>{
                    foreach (var centerWay in mainLaneCenterWay)
                    {
                        Gizmos.color = mainLaneCenterWayColor;
                        DrawDashedArrowsLocal(centerWay);
                    }
                });
            }

            void DrawDashedArrowsLocal(IEnumerable<Vector3> vertices, bool isLoop = false,
                float lineLength = 3f, float spaceLength = 1f)
            {
                const float yOffset = 0.5f;
                DrawDashedArrows(vertices.Select(v => v.PutY(v.y + yOffset)), isLoop, lineLength, spaceLength);
            }

            return drawFuncs;
        }

        private static void AddDrawFunc(ref List<Action> drawFuncs, List<List<Vector3>> lines, Color color)
        {
            if (lines.Count == 0)
            {
                return;
            }
            drawFuncs.Add(() =>
            {
                Gizmos.color = color;
                foreach (var line in lines)
                {
                    Gizmos.DrawLineStrip(line.ToArray(), false);
                }
            });

        }

        private static bool CalcCenterPos(IEnumerable<RnPoint> points, out UnityEngine.Vector3 v)
        {
            var nP = points.Count();
            if (nP <= 0)
            {
                v = Vector3.zero;
                return false;
            }
            var sum = Vector3.zero;
            foreach (var p in points)
            {
                sum += p.Vertex;
            }
            var borderPos = sum / nP;
            v = borderPos;
            return true;
        }


        public static void DrawDashedArrows(IEnumerable<Vector3> vertices, bool isLoop = false, float lineLength = 3f, float spaceLength = 1f)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawDashedArrow(e.Item1, e.Item2, lineLength, spaceLength);
        }


        /// <summary>
        /// デバッグで破線(矢印)を描画する
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isLoop"></param>
        /// <param name="color"></param>
        /// <param name="lineLength"></param>
        /// <param name="spaceLength"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawDashedArrows(IEnumerable<Vector3> vertices, bool isLoop = false, Color? color = null, float lineLength = 3f, float spaceLength = 1f,
            bool depthTest = true)
        {
            foreach (var e in GeoGraphEx.GetEdges(vertices, isLoop))
                DrawDashedArrow(e.Item1, e.Item2, color, lineLength, spaceLength, depthTest);
        }

        public static void DrawDashedArrow(Vector3 st, Vector3 en, float lineLength = 1f, float spaceLength = 0.2f, float arrowSize = 0.5f, Vector3? arrowUp = null)
        {
            var len = (en - st).magnitude;

            var n = len / (lineLength + spaceLength);
            if (n <= 0f)
                return;

            var offset = 1f / n;
            var s = offset * lineLength / (lineLength + spaceLength);

            for (var t = 0f; t < 1f; t += offset)
            {
                var p0 = Vector3.Lerp(st, en, t);
                var p1 = Vector3.Lerp(st, en, Mathf.Min(1f, t + s));
                DrawArrow(p0, p1, arrowSize, arrowUp);
            }
        }

        /// <summary>
        /// デバッグで破線を描画する
        /// </summary>
        /// <param name="st"></param>
        /// <param name="en"></param>
        /// <param name="color"></param>
        /// <param name="lineLength"></param>
        /// <param name="spaceLength"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="arrowColor"></param>
        public static void DrawDashedArrow(Vector3 st, Vector3 en
            , Color? color = null, float lineLength = 1f, float spaceLength = 0.2f, bool depthTest = true, float arrowSize = 0.5f, Vector3? arrowUp = null, Color? arrowColor = null)
        {
            var len = (en - st).magnitude;

            var n = len / (lineLength + spaceLength);
            if (n <= 0f)
                return;

            var offset = 1f / n;
            var s = offset * lineLength / (lineLength + spaceLength);

            for (var t = 0f; t < 1f; t += offset)
            {
                var p0 = Vector3.Lerp(st, en, t);
                var p1 = Vector3.Lerp(st, en, Mathf.Min(1f, t + s));
                DrawArrow(p0, p1, arrowSize, arrowUp, color, arrowColor: arrowColor, depthTest: depthTest);
            }
        }

        public static void DrawArrow(
              Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null)
        {
            Vector3 up = arrowUp ?? Vector3.up;

            DrawLine(start, end);
            if (arrowSize > 0f)
            {
                up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;
                var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
                var a2 = Quaternion.AngleAxis(-90, up) * a1;
                a1 = a1.normalized;
                a2 = a2.normalized;
                DrawLine(end + a1 * arrowSize, end);
                DrawLine(end + a2 * arrowSize, end);
            }
        }
        /// <summary>
        /// start -> endの方向に矢印を描画する
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="arrowSize"></param>
        /// <param name="arrowUp"></param>
        /// <param name="bodyColor"></param>
        /// <param name="arrowColor">矢印部分だけの色. nullの場合はcolorと同じ</param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawArrow(
            Vector3 start
            , Vector3 end
            , float arrowSize = 0.5f
            , Vector3? arrowUp = null
            , Color? bodyColor = null
            , Color? arrowColor = null
            , bool depthTest = true)
        {
            Vector3 up = arrowUp ?? Vector3.up;

            var bodyColorImpl = bodyColor ?? Color.white;

            DrawLine(start, end, bodyColorImpl, depthTest);
            if (arrowSize > 0f)
            {
                up = Vector3.Cross(end - start, Vector3.Cross(end - start, up)).normalized;
                var a1 = Quaternion.AngleAxis(45f, up) * (start - end);
                var a2 = Quaternion.AngleAxis(-90, up) * a1;
                a1 = a1.normalized;
                a2 = a2.normalized;
                var arrowColorImpl = arrowColor ?? bodyColorImpl;
                DrawLine(end + a1 * arrowSize, end, arrowColorImpl, depthTest);
                DrawLine(end + a2 * arrowSize, end, arrowColorImpl, depthTest);
            }
        }

        /// <summary>
        /// Debug.DrawLineのラッパー. デバッグ描画系をここに集約するため
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawLine(Vector3 start, Vector3 end)
        {
            Gizmos.DrawLine(start, end);
        }

        /// <summary>
        /// Debug.DrawLineのラッパー. デバッグ描画系をここに集約するため
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        /// <param name="depthTest"></param>
        public static void DrawLine(Vector3 start, Vector3 end, Color? color, bool depthTest)
        {
            Gizmos.color = color ?? Color.white;
            Handles.zTest = depthTest ? CompareFunction.Always : CompareFunction.LessEqual;
            Gizmos.DrawLine(start, end);
        }

    }
}
