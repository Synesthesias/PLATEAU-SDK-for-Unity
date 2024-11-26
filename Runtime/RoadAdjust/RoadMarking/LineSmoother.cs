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
        public void Smooth(RnModel model)
        {
            var smoother = new LineSmoother();
            foreach (var road in model.Roads)
            {
                foreach (var sideWalk in road.SideWalks)
                {
                    foreach (var way in sideWalk.AllWays)
                    {
                        smoother.Smooth(way);
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

            foreach (var intersection in model.Intersections)
            {
                foreach (var sideWalk in intersection.SideWalks)
                {
                    foreach (var way in sideWalk.AllWays)
                    {
                        smoother.Smooth(way);
                    }
                }

                foreach (var edge in intersection.Edges)
                {
                    smoother.Smooth(edge.Border);
                }
            }
        }
    }
    
    /// <summary>
    /// 線を滑らかにします。
    /// </summary>
    internal class LineSmoother
    {
        private const float SubDivideDistance = 3f;
        private const float SmoothResolutionDistance = 0.5f;
        private const float OptimizeAngleThreshold = 2f; // 度数法


        public Vector3[] Smooth(IEnumerable<Vector3> lineArg)
        {
            var line = lineArg.ToArray();
            // スプライン補間だと点が離れている場合に元の線からのズレが大きくなりがちなので、
            // 点を細かくしてからスプライン補間を行います。
            var lineA = SubDivide(line);
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