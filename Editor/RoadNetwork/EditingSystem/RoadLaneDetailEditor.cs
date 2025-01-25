using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路編集で詳細モードがONになっている場合の処理です。
    /// </summary>
    internal class RoadLaneDetailEditor : LineSelectEdit.ICreatedLineReceiver
    {
        private LineSelectEdit lineSelectEdit;
        private RoadNetworkEditTarget editTarget;
        private const bool DoSubdivide = false;

        public RoadLaneDetailEditor()
        {
            lineSelectEdit = new LineSelectEdit(this);
        }

        public void Draw(RnRoadGroup roadGroup, RoadNetworkEditTarget editTargetArg)
        {
            editTarget = editTargetArg;
            var targetLines = new List<IEditTargetLine>();
            foreach (var road in roadGroup.Roads)
            {
                targetLines.AddRange(EditTargetRoadWay.CollectRoadLineEditTarget(road));
            }
            lineSelectEdit.Draw(targetLines.ToArray());
            
        }
        
        public void OnLineCreated(Spline createdSpline, IEditTargetLine targetLineBase)
        {
            var targetLine = targetLineBase as EditTargetRoadWay ;
            targetLine.Apply(targetLine.Road, createdSpline);

            var reproduceTarget = new RrTargetRoadBases(editTarget.RoadNetwork, new[] { targetLine.Road });
            new RoadReproducer().Generate(reproduceTarget, CrosswalkFrequency.All, DoSubdivide);
        }

        
        
        /// <summary>
        /// 編集対象の線のうち、道路のWayです。
        /// </summary>
        private class EditTargetRoadWay : IEditTargetLine
        {
            public RnWay Way { get; }
            public RnRoad Road { get; }

            public EditTargetRoadWay(RnWay way, RnRoad road)
            {
                Way = way;
                Road = road;
            }
            
            public static IEnumerable<IEditTargetLine> CollectRoadLineEditTarget(RnRoad road)
            {
                foreach (var lane in road.AllLanesWithMedian)
                {
                    foreach (var w in lane.BothWays)
                    {
                        yield return new EditTargetRoadWay(w, road);
                    }
                }

                foreach (var s in road.SideWalks)
                {
                    yield return new EditTargetRoadWay(s.OutsideWay, road);
                }
            }
            

            public void Apply(RnRoadBase roadBase, Spline createdSpline)
            {
                // 適用後のLineStringを生成します。
                var positions = createdSpline
                    .Knots
                    .Select(k => k.Position)
                    .Select(f => new Vector3(f.x, f.y, f.z))
                    .Select(v => new RnPoint(v));
                var lineString = new RnLineString(positions);
                var reverse = lineString.Points;
                reverse.Reverse();
                var lineStringReverse = new RnLineString(reverse);
                
                // 同じWayをすべて見つけます。
                var correspondWays = Road.AllWays().Where(w => Way.IsSameLineSequence(w)).ToArray();
                // 車道と歩道の間の線は、逆順もチェックしないと発見できません
                var correspondWaysReverse = Road.AllWays().Where(w => Way.IsSameLineSequenceReverse(w)).ToArray();
                
                // 適用します。
                foreach (var w in correspondWays)
                {
                    w.LineString = lineString;
                }

                foreach (var w in correspondWaysReverse)
                {
                    w.LineString = lineStringReverse;
                }
            }

            public Vector3[] Line
            {
                get => Way.Points.Select(p => p.Vertex).ToArray();
            }
        }

        
    }
}