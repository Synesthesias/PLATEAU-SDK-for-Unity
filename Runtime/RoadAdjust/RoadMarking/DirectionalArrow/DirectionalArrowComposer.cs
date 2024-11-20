using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
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
        
        public DirectionalArrowComposer(IRrTarget target)
        {
            this.target = target;
        }

        public List<RoadMarkingInstance> Generate()
        {
            var ret = new List<RoadMarkingInstance>();
            
            
            foreach (var road in target.Roads())
            {
                var nextIntersection = road.Next as RnIntersection;
                var prevIntersection = road.Prev as RnIntersection;
                
                // roadでnextとprevを見る、isReverseに応じて
                foreach (var lane in road.MainLanes)
                {
                    if (lane.NextBorder != null)
                    {
                        var inter = lane.IsReverse ? prevIntersection : nextIntersection;
                        var nextArrow = GenerateArrow(lane.NextBorder, inter, ArrowPosition(lane, true));
                        ret.Add(nextArrow);
                    }

                    if (lane.PrevBorder != null)
                    {
                        var inter = lane.IsReverse ? nextIntersection : prevIntersection;
                        var prevArrow = GenerateArrow(lane.PrevBorder, inter, ArrowPosition(lane, false));
                        ret.Add(prevArrow);
                    }
                    
                    
                }
                
            }
            return ret;
        }

        private RoadMarkingInstance GenerateArrow(RnWay laneBorder, RnIntersection intersection, Vector3 position)
        {
            if (intersection == null) return null;
            var type = ArrowType(laneBorder, intersection);
            var meshInstance = type.ToMeshInstance();
            if (meshInstance == null)
            {
                Debug.LogError("mesh instance is null.");
                return null;
            }
            
            meshInstance.Translate(position);
            return meshInstance;
        }

        private Vector3 ArrowPosition(RnLane lane, bool isNext)
        {
            var border = isNext ? lane.NextBorder : lane.PrevBorder;
            if (border.Count == 0)
            {
                Debug.Log("Skipping because border count is 0.");
                return Vector3.zero;
            }

            if(border.Count == 1)
            {
                return border[0];
            }

            return (border[0] + border[^1]) / 2f;
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