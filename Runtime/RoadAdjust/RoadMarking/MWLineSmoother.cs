using PLATEAU.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// <see cref="MWLine"/>を滑らかにします。
    /// MWはMarkedWayの略です。
    /// </summary>
    public class MWLineSmoother
    {
        private const float SubDivideDistance = 3f;
        private const float SmoothResolutionDistance = 1.5f;
        private const float OptimizeAngleThreshold = 2f; // 度数法


        public void Smooth(MWLine line)
        {
            // スプライン補間だと点が離れている場合に元の線からのズレが大きくなりがちなので、
            // 点を細かくしてからスプライン補間を行います。
            SubDivide(line);
            SmoothBySpline(line);
            Optimize(line);
        }

        private void SubDivide(MWLine line)
        {
            var nextPoints = new List<Vector3>();
            for (int i = 0; i < line.Count - 1; i++)
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
            line.Points = nextPoints.ToArray();   
        }
        
        private void SmoothBySpline(MWLine line)
        {
            float len = line.SumDistance();
            if (len <= 0) return;
            float resolutionPercent = SmoothResolutionDistance / len;
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

            line.Points = nextPoints.ToArray();
        }

        private void Optimize(MWLine line)
        {
            // 隣り合う3点で角度に変化がない場合、中間点を削除します。
            var shouldRemove = new bool[line.Count];
            shouldRemove[0] = false;
            shouldRemove[^1] = false;
            float angleDiffSum = 0f;
            for (int i = 0; i < line.Count - 2; i++)
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
            for(int i=0; i<line.Count; i++)
            {
                if (!shouldRemove[i])
                {
                    nextLine.Add(line[i]);
                }
            }
            line.Points = nextLine.ToArray();
        }
    }
}