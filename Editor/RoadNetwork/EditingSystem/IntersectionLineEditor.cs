using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 交差点の線の形状を編集します。
    /// </summary>
    internal class IntersectionLineEditor : LineSelectEdit.ICreatedLineReceiver
    {
        private RoadNetworkEditTarget target;
        private LineSelectEdit lineSelectEdit;
        

        public IntersectionLineEditor(RoadNetworkEditTarget target)
        {
            this.target = target;
            this.lineSelectEdit = new LineSelectEdit(this);
        }

        /// <summary> 毎フレームのシーンビューへの描画 </summary>
        public void Draw()
        {
            var intersectionEdit = target.SelectedRoadNetworkElement as EditorData<RnIntersection>;
            var intersection = intersectionEdit?.Ref;
            if (intersection == null) return;

            var lines = IEditTargetLine.ComposeFromIntersection(intersection);
            lineSelectEdit.Draw(lines);
            
        }

        

        /// <summary> 線の編集を確定したとき、それを道路ネットワークに適用します。 </summary>
        public void OnLineCreated(Spline createdSpline, IEditTargetLine targetLine)
        {

            var targetIntersection = (target.SelectedRoadNetworkElement as EditorData<RnIntersection>)?.Ref;
            targetLine.Apply(targetIntersection, createdSpline);

            var reproduceTarget = new RrTargetRoadBases(target.RoadNetwork, new[] { targetIntersection });
            new RoadReproducer().Generate(reproduceTarget, CrosswalkFrequency.All);
            
        }
        
        
    }
    
    /// <summary> 交差点で編集対象となる線のうち、トラックです。 </summary>
    internal class EditTargetTrack : IEditTargetLine
    {
        public RnTrack Track { get; set; }

        public EditTargetTrack(RnTrack track)
        {
            Track = track;
        }

        public void Apply(RnRoadBase roadBase, Spline createdSpline)
        {
            Track.Spline = createdSpline;
        }

        public Vector3[] Line
        {
            get => Track.Spline.Knots.Select(k => k.Position).Select(f => new Vector3(f.x, f.y, f.z)).ToArray();
        }
    }

    /// <summary> 交差点で編集対象となる線のうち、歩道の内側です。 </summary>
    internal class EditTargetIntersectionShape : IEditTargetLine
    {
        private RnWay Way { get; set; }

        public EditTargetIntersectionShape(RnWay way)
        {
            Way = way;
        }


        /// <summary> 編集した線を道路ネットワークに適用します。 </summary>
        public void Apply(RnRoadBase roadBase, Spline createdSpline)
        {
            var intersection = roadBase as RnIntersection;
            // 交差点のEdgeの線に対して適用するのと同じく、対応するSidewalkを探して同じように適用します。
            // Sidewalkへ適用しないと、白線は変わるのに歩道の形は変わらなくなります。
            var correspondSidewalkInside =
                intersection
                    ?.SideWalks
                    .Select(sw => sw.InsideWay)
                    .FirstOrDefault(way => Way.IsSameLineSequence(way));

            var positions = createdSpline.Knots.Select(k => new RnPoint(k.Position)).ToArray();
            var line = new RnLineString(positions);

            // 交差点のEdgeに線を適用します。
            Way.LineString = line;

            // 歩道に線に適用します。
            if (correspondSidewalkInside != null)
            {
                correspondSidewalkInside.LineString = line;
            }
        }

        public Vector3[] Line
        {
            get => Way.LineString.Points.Select(p => p.Vertex).Select(v => new Vector3(v.x, v.y, v.z)).ToArray();
        }
    }
}