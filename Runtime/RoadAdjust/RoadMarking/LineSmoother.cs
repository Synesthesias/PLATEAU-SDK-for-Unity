using PLATEAU.CityInfo;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadAdjust.RoadMarking
{

    /// <summary>
    /// 道路ネットワーク中にある線を滑らかにします。
    /// </summary>
    internal class RoadNetworkLineSmoother
    {
        /// <summary>
        /// 線を滑らかにします。
        /// </summary>
        public void Smooth(IRrTarget target, ISmoothingStrategy smoothingStrategy)
        {
            
            var smoother = new LineSmoother(smoothingStrategy.ShouldSubdivide());
            foreach (var road in target.Roads())
            {
                var roadSrc = road.TargetTrans.FirstOrDefault();
                foreach (var sideWalk in road.SideWalks)
                {
                    
                    var inside = sideWalk.InsideWay;
                    if (inside != null && smoothingStrategy.ShouldSmoothRoadSidewalkInside(roadSrc))
                    {
                        smoother.Smooth(inside);
                    }

                    var outside = sideWalk.OutsideWay;
                    if (outside != null && smoothingStrategy.ShouldSmoothSidewalkOutside())
                    {
                        smoother.Smooth(outside);
                    }
                }

                foreach (var lane in road.AllLanesWithMedian)
                {
                    foreach (var way in lane.AllWays)
                    {
                        smoother.Smooth(way);
                    }
                }
            }

            foreach (var intersection in target.Intersections())
            {
                foreach (var sideWalk in intersection.SideWalks)
                {
                    var inside = sideWalk.InsideWay;
                    if (inside != null && smoothingStrategy.ShouldSmoothIntersectionSidewalkInside())
                    {
                        smoother.Smooth(inside);
                    }

                    var outside = sideWalk.OutsideWay;
                    if (outside != null && smoothingStrategy.ShouldSmoothSidewalkOutside())
                    {
                        smoother.Smooth(outside);
                    }
                }

                foreach (var edge in intersection.Edges.Where(e => !e.IsBorder))
                {
                    smoother.Smooth(edge.Border);
                }
            }
        }
    }

    /// <summary>
    /// どこの線を滑らかにし、どこを滑らかにしないかが道路生成の用途によって異なるため、サブクラスで設定するためのインターフェイスです。
    /// </summary>
    internal interface ISmoothingStrategy
    {
        bool ShouldSmoothSidewalkOutside();
        bool ShouldSmoothRoadSidewalkInside(PLATEAUCityObjectGroup src);
        bool ShouldSmoothIntersectionSidewalkInside();
        
        /// <summary> <see cref="LineSmoother"/>に渡す設定値です。"/> </summary>
        bool ShouldSubdivide();
    }

    /// <summary>
    /// 3D都市モデルの道路から道路ネットワークを生成する場合に使用する設定です。
    /// 道路線を滑らかにするにあたって、外形線など3D都市モデルに直接由来する線は、元の形状を尊重したいというクライアント要望のために滑らかにしません。
    /// 一方で、類推された線はは3D都市モデルに直接由来しないので滑らかにします。例えばLOD1の道路で類推された歩道の内側の線は滑らかにします。
    /// </summary>
    internal class SmoothingStrategyRespectOriginal : ISmoothingStrategy
    {
        public bool ShouldSmoothSidewalkOutside() => false;
        public bool ShouldSmoothRoadSidewalkInside(PLATEAUCityObjectGroup src)
        {
            if (src == null) return true;
            if (src.Lod < 2) return true;
            return false;
        }
        
        public bool ShouldSmoothIntersectionSidewalkInside() => true; // 交差点のカーブはカクカク感が目立つのでスムーズに
        public bool ShouldSubdivide() => true;
    }

    /// <summary>
    /// ユーザーが新しく道路を作成する場合に使用する設定です。
    /// ユーザーが指定した形状が数少ない点からなる線であっても綺麗になるよう、すべて滑らかにします。
    /// </summary>
    internal class SmoothingStrategySmoothAll : ISmoothingStrategy
    {
        public bool ShouldSmoothSidewalkOutside() => true;
        public bool ShouldSmoothRoadSidewalkInside(PLATEAUCityObjectGroup src) => true;
        public bool ShouldSmoothIntersectionSidewalkInside() => true;
        public bool ShouldSubdivide() => false;
    }
    
    /// <summary>
    /// 線を滑らかにします。
    /// </summary>
    internal class LineSmoother
    {
        private const float SubDivideDistance = 3f;
        private const float SmoothResolutionDistance = 0.5f;
        private const float OptimizeAngleThreshold = 2f; // 度数法
        private readonly bool doSubDivide;

        /// <summary>
        /// </summary>
        /// <param name="doSubDivide">
        /// 線を滑らかにする前に細分化するかどうかを指定します。
        /// スプライン補間では、点が離れている場合に元の線からのズレが大きくなりがちです。
        /// なので、trueの場合、点の間隔を細かくしてからスプライン補間を行うことで元の線からのズレを抑えます。
        ///
        /// 推奨設定値：
        /// 道路ネットワークから道路メッシュを生成する用途では、線を滑らかにはしますが元の線からあまり逸脱してほしくないのでtrueにします。
        /// ユーザー自身が指定した線から道路メッシュを生成する用途では、ユーザーが作った数少ない点から滑らかな道路を生成したいのでfalseにします。(点が少ないときにtrueだと直線的になる)
        /// </param>
        public LineSmoother(bool doSubDivide)
        {
            this.doSubDivide = doSubDivide;
        }


        public Vector3[] Smooth(IEnumerable<Vector3> lineArg)
        {
            var line = lineArg.ToArray();
            
            // スプライン補間だと点が離れている場合に元の線からのズレが大きくなりがちなので、
            // 点を細かくしてからスプライン補間を行います。
            var lineA = doSubDivide ? SubDivide(line) : line;
            
            var lineB = SmoothBySpline(lineA);
            var lineC = Optimize(lineB);
            return lineC;
        }

        public void Smooth(RnWay way)
        {
            var smoothed = Smooth(way.Points.Select(p => p.Vertex)).Select(v => new RnPoint(v));
            way.Points = smoothed;
        }

        private Vector3[] SubDivide(Vector3[] line)
        {
            if (line.Length <= 1) return line;
            var nextPoints = new List<Vector3>();
            for (int i = 0; i < line.Length - 1; i++)
            {
                var p1 = line[i];
                var p2 = line[i + 1];
                var dir = p2 - p1;
                var len = dir.magnitude;
                if (len <= SubDivideDistance)
                {
                    nextPoints.Add(p1);
                    continue;
                }

                var num = Mathf.FloorToInt(len / SubDivideDistance);
                for (int j = 0; j < num; j++)
                {
                    var t = j / (float)num;
                    var p = p1 + dir * t;
                    nextPoints.Add(p);
                }
            }

            nextPoints.Add(line[^1]);
            return nextPoints.ToArray();   
        }
        
        private Vector3[] SmoothBySpline(Vector3[] line)
        {
            if (line.Length <= 1) return line;
            float sumDistance = 0;
            for(int i=0; i<line.Length-1; i++)
            {
                sumDistance += Vector3.Distance(line[i], line[i + 1]);
            }
            
            if (sumDistance <= 0) return line;
            float resolutionPercent = SmoothResolutionDistance / sumDistance;
            // Unityのスプラインを生成します。
            var spline = new Spline();
            foreach (var p in line)
            {
                spline.Add(new BezierKnot(p), TangentMode.AutoSmooth);
            }

            // スプラインをもとに補間します。
            var nextPoints = new List<Vector3>();
            for(float t=0; t<=1; t+=resolutionPercent)
            {
                var p = spline.EvaluatePosition(t);
                nextPoints.Add(p);
            }
            nextPoints.Add(spline.EvaluatePosition(1));

            return nextPoints.ToArray();
        }

        private Vector3[] Optimize(Vector3[] line)
        {
            if(line.Length <= 2) return line;
            // 隣り合う3点で角度に変化がない場合、中間点を削除します。
            var shouldRemove = new bool[line.Length];
            shouldRemove[0] = false;
            shouldRemove[^1] = false;
            float angleDiffSum = 0f;
            for (int i = 0; i < line.Length - 2; i++)
            {
                var v1 = line[i];
                var v2 = line[i + 1];
                var v3 = line[i + 2];
                angleDiffSum += Vector3.Angle(v2 - v1, v3 - v2);
                if(angleDiffSum >= OptimizeAngleThreshold)
                {
                    shouldRemove[i + 1] = false;
                    angleDiffSum = 0;
                    continue;
                }

                shouldRemove[i + 1] = true;
            }
            
            var nextLine = new List<Vector3>();
            for(int i=0; i<line.Length; i++)
            {
                if (!shouldRemove[i])
                {
                    nextLine.Add(line[i]);
                }
            }
            return nextLine.ToArray();
        }
    }
}