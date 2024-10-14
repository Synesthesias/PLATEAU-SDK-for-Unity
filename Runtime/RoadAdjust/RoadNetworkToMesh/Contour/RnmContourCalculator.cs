using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 複数の線を受け取り、それらの線をつないで多角形 <see cref="RnmContour"/> を形成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    internal class RnmContourCalculator
    {
        private List<RnmLine> lines = new ();
        private ConditionalLogger logger = new ConditionalLogger(() => true); // デバッグのときだけここをtrueにしてください
        private const float VerticesWeldDistThreshold = 2f;
        private GameObject sourceObject;

        public RnmContourCalculator(GameObject sourceObject)
        {
            this.sourceObject = sourceObject;
        }

        public void AddLine(IEnumerable<Vector3> line)
        {
            var lineArray = line.ToArray();
            if (lineArray.Length <= 0) return;
            lines.Add(new RnmLine(lineArray));
        }
        
        public void AddRangeLine(IEnumerable<IEnumerable<Vector3>> linesArg)
        {
            foreach (var line in linesArg)
            {
                AddLine(line);
            }
        }

        public RnmContour Calculate()
        {
            if (lines.Count == 0)
            {
                logger.Log("skipping RnmContour because no lines exist.");
                return new RnmContour(sourceObject);
            }
            RemoveDuplicateOrReverseLine();
            // RemoveDuplicateVertices(VerticesWeldDistThreshold);
            SortByNearestEdgeDist();
            var contour = new RnmContour(sourceObject);
            // 最初の線を追加します。
            var line = lines[0];
            contour.AddVertices(line.Vertices);
            line.IsProcessed = true;
            bool lineDirectionLast = true; // 次に繋げるのが末尾方向ならtrue, 最初方向ならfalseです。
            
            // 線の末尾について、もっとも近い点を探して繋いでいきます。
            while (lines.Any(l => !l.IsProcessed))
            {
                var remaining = lines.Where(l => !l.IsProcessed).ToArray();
                int minID = -1;
                float minDist = float.MaxValue;
                var v = lineDirectionLast ? line[^1] : line[0]; // 繋げたい線の起点
                bool rConnectedDirectionLast = false;
                // 未処理の線の中で、最も近い線を探します。
                for (int i = 0; i < remaining.Length; i++)
                {
                    var r = remaining[i];
                    var distFirst = Vector3.Distance(v, r[0]);
                    var distLast = Vector3.Distance(v, r[^1]);
                    if (distFirst < minDist)
                    {
                        minDist = distFirst;
                        minID = i;
                        rConnectedDirectionLast = false;
                    }
                    if (distLast < minDist)
                    {
                        minDist = distLast;
                        minID = i;
                        rConnectedDirectionLast = true;
                    }
                }
                var minLine = remaining[minID];
                // 線を繋ぎます。
                if (rConnectedDirectionLast)
                {
                    contour.AddVertices(minLine.Vertices.Reverse());
                }
                else
                {
                    contour.AddVertices(minLine.Vertices);
                }

                line = minLine;
                minLine.IsProcessed = true;
                lineDirectionLast = !rConnectedDirectionLast;
            }

            // 後処理です
            if (contour.Count < 3)
            {
                logger.Log($"skipping because vertex count = {contour.Count}");
                return new RnmContour(sourceObject);
            }

            if (!contour.IsClockwise())
            {
                contour.Reverse();
            }
            return contour;
        }
        

        private void SortByNearestEdgeDist()
        {
            (RnmLine line, float dist)[] dists = new (RnmLine line, float dist)[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                dists[i] = (lines[i], NearestEdgeDist(lines[i]));
            }

            lines = dists.OrderBy(d => d.dist).Select(d => d.line).ToList();            
        }

        private float NearestEdgeDist(RnmLine line)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == line) continue;
                var line2 = lines[i];
                if (line.Count <= 0) continue;
                foreach (var v1 in new Vector3[] { line[0], line[^1] })
                {
                    if (line2.Count <= 0) continue;
                    foreach (var v2 in new Vector3[] { line2[0], line2[^1] })
                    {
                        minDist = Math.Min(minDist, Vector3.Distance(v1, v2));
                    }
                }

            }

            return minDist;
        }
        

        /// <summary> 線が一致する、または逆順にしたら一致するものを排除します </summary>
        private void RemoveDuplicateOrReverseLine()
        {
            var newLines = new List<RnmLine>(lines.Count);
            for (int i = 0; i < lines.Count; i++)
            {
                var lineI = lines[i];
                bool isDuplicate = false;
                for (int j = i + 1; j < lines.Count; j++)
                {
                    var lineJ = lines[j];
                    if (lineI.IsSameOrReverseWith(lineJ))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if(!isDuplicate) newLines.Add(lineI);
            }
            lines = newLines;
        }


        private void RemoveDuplicateVertices(float threshold)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line1 = lines[i];
                var nextLine = new List<Vector3>(line1.Count);
                foreach (var v1 in line1)
                {
                    bool isDuplicate = false;
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        var line2 = lines[j];
                        foreach (var v2 in line2)
                        {
                            if (Vector3.Distance(v1, v2) < threshold)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                        if (isDuplicate) break;
                    }
                    if (!isDuplicate) nextLine.Add(v1);
                }
                lines[i] = new RnmLine(nextLine);
            }

            lines = lines.Where(l => l.Count >= 1).ToList();
        }
        
        
    }
}