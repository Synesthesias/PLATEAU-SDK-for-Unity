using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking.Crosswalk
{
    internal class CrosswalkComposer
    {
        /// <summary> 横断歩道を停止線から何メートル奥に設置するか </summary>
        private const float PositionOffset = 3.5f;
        /// <summary> 横断歩道の幅 </summary>
        private const float CrosslineWidth = 4f;
        /// <summary> 横断歩道の破線の実線部と空白部のインターバル </summary>
        private const float CrosslineDashInterval = 0.45f;
        /// <summary> 横断歩道の端点を歩道から最低何メートル離すか </summary>
        private const float LineOffset = 0.1f;
        
        
        public List<CrosswalkInstance> Compose(IRrTarget target, CrosswalkFrequency crosswalkFrequency)
        {
            var ret = new List<CrosswalkInstance>();
            var placementRule = crosswalkFrequency.ToPlacementRule();
            foreach (var road in target.Roads())
            {
                if (!placementRule.ShouldPlace(road)) continue;
                if (road.Next is RnIntersection nextIntersection)
                {
                    var nextBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Next, null));
                    var crosswalk = GenerateCrosswalk(nextBorder, nextIntersection, road, ReproducedRoadDirection.Next);
                    if(crosswalk != null) ret.Add(crosswalk);
                }

                if (road.Prev is RnIntersection prevIntersection)
                {
                    var prevBorder = new MWLine(road.GetMergedBorder(RnLaneBorderType.Prev, null));
                    var crosswalk = GenerateCrosswalk(prevBorder, prevIntersection, road, ReproducedRoadDirection.Prev);
                    if(crosswalk != null) ret.Add(crosswalk);
                }
            }
            
            return ret;
        }

        /// <summary>
        /// <paramref name="intersection"/>の<paramref name="border"/>側に交差点を1つ生成します。
        /// 生成できない場合はnullを返します。
        /// </summary>
        private CrosswalkInstance GenerateCrosswalk(MWLine border, RnIntersection intersection, RnRoadBase srcRoad, ReproducedRoadDirection direction)
        {
            // 横断歩道を結ぶべき場所として、歩道と歩道を結んだ線を求めます。
            var linePositions = ShiftStopLine(border, intersection, PositionOffset);
            if (linePositions == null) return null;
            var lineWay = new RnWay(new RnLineString(linePositions.Select(p => new RnPoint(p))));

            // 横断歩道を車道の中心に配置するための計算です。
            float lineLen = Vector3.Distance(linePositions[0], linePositions[^1]);
            float lineLenMinusOffset = lineLen - LineOffset * 2f;
            int dashCount = Mathf.FloorToInt(lineLenMinusOffset / CrosslineDashInterval); // 破線の単位がいくつ入るか
            if (dashCount % 2 == 0) dashCount--; // 破線が空白部で終わる場合、末尾の空白を除く
            if (dashCount <= 0) return null;
            float crosswalkLen = CrosslineDashInterval * dashCount + 0.01f;
            float crosswalkOffset = (lineLen - crosswalkLen) / 2;
            var crosswalkPositions = new[]
            {
                lineWay.PositionAtDistance(crosswalkOffset, false),
                lineWay.PositionAtDistance(crosswalkOffset, true)
            };
            // 横断歩道を1本の破線として生成します。
            var crosswalk = new DashedLineMeshGenerator(RoadMarkingMaterial.White, true, CrosslineWidth, CrosslineDashInterval).GenerateMeshInstance(crosswalkPositions);
            return new CrosswalkInstance(crosswalk, srcRoad, direction);
        }


        /// <summary>
        /// 停止線を<paramref name="positionOffset"/>メートルだけ奥に動かした線を返します。
        /// 戻り値は2点となります。
        /// </summary>
        private Vector3[] ShiftStopLine(MWLine border, RnIntersection intersection, float positionOffset)
        {
            if (border.Count <= 1) return null;
            const float Threshold = 0.1f;
            var stop1 = border[0];
            var stop2 = border[^1];
            // 交差点で道路と接していない外形のうち、該当道路と接する線がどこかを探します
            Vector3 shift1 = stop1;
            Vector3 shift2 = stop2;
            bool shift1Found = false;
            bool shift2Found = false;
            foreach (var edge in intersection.Edges.Where(n => n.Road == null))
            {
                var e = edge.Border;
                var e1 = edge.Border[0];
                var e2 = edge.Border[^1];
                if (Vector3.Distance(e1, stop1) < Threshold)
                {
                    shift1 = e.PositionAtDistance(positionOffset, false);
                    shift1Found = true;
                }
                else if (Vector3.Distance(e1, stop2) < Threshold)
                {
                    shift2 = e.PositionAtDistance(positionOffset, false);
                    shift2Found = true;
                }
                else if (Vector3.Distance(e2, stop1) < Threshold)
                {
                    shift1 = e.PositionAtDistance(positionOffset, true);
                    shift1Found = true;
                }
                else if (Vector3.Distance(e2, stop2) < Threshold)
                {
                    shift2 = e.PositionAtDistance(positionOffset, true);
                    shift2Found = true;
                }
            }

            if (!shift1Found || !shift2Found)
            {
                return null;
            }
            return new[] { shift1, shift2 };
        }

        /// <summary> 横断歩道のメッシュ情報と付随する情報を保持します。 </summary>
        internal class CrosswalkInstance
        {
            public RoadMarkingInstance RoadMarkingInstance { get; private set; }
            public RnRoadBase SrcRoad { get; private set; }
            public ReproducedRoadDirection Direction { get; private set; }

            public CrosswalkInstance(RoadMarkingInstance roadMarkingInstance, RnRoadBase srcRoad, ReproducedRoadDirection direction)
            {
                RoadMarkingInstance = roadMarkingInstance;
                SrcRoad = srcRoad;
                Direction = direction;
            }
        }
    }
}