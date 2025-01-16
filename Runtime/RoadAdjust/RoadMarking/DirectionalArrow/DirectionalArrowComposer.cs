using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking.DirectionalArrow
{
    /// <summary>
    /// 路面標示として、交差点手前の矢印を生成します。
    /// </summary>
    internal class DirectionalArrowComposer
    {
        private IRrTarget target;
        private const float ArrowMeshHeightOffset = 0.07f; // 経験的に道路に埋まらないくらいの高さ
        
        /// <summary> 矢印を停止線からどの距離に置くか </summary>
        private const float ArrowPositionOffset = 4.5f;

        public DirectionalArrowComposer(IRrTarget target)
        {
            this.target = target;
        }

        public List<RoadMarkingInstance> Compose()
        {
            var ret = new List<RoadMarkingInstance>();


            foreach (var road in target.Roads())
            {
                var nextIntersection = road.Next as RnIntersection;
                var prevIntersection = road.Prev as RnIntersection;

                // roadでnextとprevを見る、isReverseに応じて
                foreach (var lane in road.MainLanes)
                {
                    // 不正なレーンは無視する
                    if (lane?.IsValidWay == false)
                    {
                        Debug.Log("Skipping invalid lane.");
                        continue;
                    }

                    if (lane.NextBorder != null)
                    {
                        var inter = lane.IsReverse ? prevIntersection : nextIntersection;
                        var nextArrow = GenerateArrow(lane.NextBorder, inter, ArrowPosition(lane, true, out var isSucceedP), ArrowAngle(lane, true, out bool isSucceedA));
                        if(isSucceedP && isSucceedA) ret.Add(nextArrow);
                    }

                    if (lane.PrevBorder != null)
                    {
                        var inter = lane.IsReverse ? nextIntersection : prevIntersection;
                        var prevArrow = GenerateArrow(lane.PrevBorder, inter, ArrowPosition(lane, false, out var isSucceedP), ArrowAngle(lane, false, out var isSucceedA));
                        if(isSucceedP && isSucceedA) ret.Add(prevArrow);
                    }


                }

            }
            return ret;
        }

        /// <summary>
        /// 車線タイプを判別し、矢印モデルを生成します。
        /// <paramref name="rotation"/>は矢印モデルをy軸を中心に何度回転させるかです。
        /// </summary>
        private RoadMarkingInstance GenerateArrow(RnWay laneBorder, RnIntersection intersection, Vector3 position, float rotation)
        {
            if (intersection == null) return null;
            var type = ArrowType(laneBorder, intersection);
            var meshInstance = type.ToMeshInstance();
            if (meshInstance == null)
            {
                return null;
            }

            meshInstance.RotateYAxis(rotation);
            meshInstance.Translate(position + Vector3.up * ArrowMeshHeightOffset);
            return meshInstance;
        }

        private Vector3 ArrowPosition(RnLane lane, bool isNext, out bool isSucceed)
        {
            // 左のWayと右のWayの平均をとります。
            int wayCount = 0;
            var posSum = Vector3.zero;
            var leftWay = lane.LeftWay;
            if (leftWay != null && leftWay.Count > 0)
            {
                posSum += leftWay.PositionAtDistance(ArrowPositionOffset, isNext ^ leftWay.IsReversed);
                wayCount++;
            }

            var rightWay = lane.RightWay;
            if (rightWay != null && rightWay.Count > 0)
            {
                posSum += rightWay.PositionAtDistance(ArrowPositionOffset, isNext ^ rightWay.IsReversed);
                wayCount++;
            }
            
            if (wayCount == 0)
            {
                Debug.Log("Skipping because way count is 0.");
                isSucceed = false;
                return Vector3.zero;
            }

            isSucceed = true;
            return posSum / wayCount;
        }



        /// <summary>
        /// 矢印モデルを、y軸を中心に何度回転すれば良いかを返します。
        /// </summary>
        private float ArrowAngle(RnLane lane, bool isNext, out bool isSucceed)
        {
            // レーンの左右で平均を取ります。
            int wayCount = 0;
            float angleSum = 0f;
            var leftWay = lane.LeftWay;
            if (leftWay != null && leftWay.Count > 1)
            {
                angleSum += ArrowAngleOneWay(leftWay, isNext);
                wayCount++;
            }

            var rightWay = lane.RightWay;
            if (rightWay != null && rightWay.Count > 1)
            {
                angleSum += ArrowAngleOneWay(rightWay, isNext);
                wayCount++;
            }
            
            if (wayCount == 0)
            {
                Debug.Log("Skipping Angle because way count is 0.");
                isSucceed = false;
                return 0;
            }
            
            isSucceed = true;
            return angleSum / wayCount;

        }

        private float ArrowAngleOneWay(RnWay way, bool isNext)
        {
            // wayの頂点数が2以上であることが前提です。頂点数1以下は渡さないようにしてください。
            var diff = isNext ^ way.IsReversed
                ? way.GetPoint(way.Count - 1).Vertex - way.GetPoint(way.Count - 2).Vertex
                : way.GetPoint(0).Vertex - way.GetPoint(1).Vertex;
            return Vector2.SignedAngle(diff.Xz(), Vector2.up);
        }


        private DirectionalArrowType ArrowType(RnWay laneBorder, RnIntersection intersection)
        {
            if (intersection == null) return DirectionalArrowType.None;
            bool containStraight = false;
            bool containLeft = false;
            bool containRight = false;
            foreach (var track in intersection.Tracks)
            {
                if (!track.FromBorder.IsSameLineSequence(laneBorder)) continue;
                var turnT = track.TurnType;
                containLeft |= turnT.IsLeft();
                containRight |= turnT.IsRight();
                containStraight |= turnT == RnTurnType.Straight;
            }

            if (containStraight && containLeft && containRight) return DirectionalArrowType.None;
            if (containStraight && containLeft) return DirectionalArrowType.StraightAndLeft;
            if (containStraight && containRight) return DirectionalArrowType.StraightAndRight;
            if (containStraight) return DirectionalArrowType.Straight;
            if (containRight) return DirectionalArrowType.Right;
            if (containLeft) return DirectionalArrowType.Left;

            return DirectionalArrowType.None;
        }

    }

    internal enum DirectionalArrowType
    {
        None, Straight, Left, Right, StraightAndLeft, StraightAndRight
    }

    internal static class DirectionalArrowTypeExtension
    {
        private const string ModelFolder = "ArrowModel/";
        public static RoadMarkingInstance ToMeshInstance(this DirectionalArrowType type)
        {
            if (type == DirectionalArrowType.None) return null;
            // return new RoadMarkingInstance(Resources.Load<Mesh>(ModelFolder + "RoadMarkArrowStraight"), RoadMarkingMaterial.Yellow);;

            var prefab = type switch
            {
                DirectionalArrowType.Straight => Resources.Load<GameObject>(ModelFolder + "RoadMarkArrowStraight"),
                DirectionalArrowType.Left => Resources.Load<GameObject>(ModelFolder + "RoadMarkArrowLeft"),
                DirectionalArrowType.Right => Resources.Load<GameObject>(ModelFolder + "RoadMarkArrowRight"),
                DirectionalArrowType.StraightAndLeft => Resources.Load<GameObject>(ModelFolder + "RoadMarkArrowStraightLeft"),
                DirectionalArrowType.StraightAndRight => Resources.Load<GameObject>(ModelFolder + "RoadMarkArrowStraightRight"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            if (prefab == null)
            {
                Debug.LogError("Prefab is not found.");
                return null;
            }

            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError("mesh filter is not found.");
                return null;
            }

            var mesh = meshFilter.sharedMesh;
            return new RoadMarkingInstance(mesh, RoadMarkingMaterial.White);
        }
    }
}