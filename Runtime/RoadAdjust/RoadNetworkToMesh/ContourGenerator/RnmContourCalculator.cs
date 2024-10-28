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
        private readonly ConditionalLogger logger = new (() => false); // デバッグのときだけここをtrueにしてください
        private RnmMaterialType material;

        public RnmContourCalculator(RnmMaterialType material)
        {
            this.material = material;
        }
        

        public void AddLine(IEnumerable<Vector3> line, Vector2 startUV1, Vector2 endUV1)
        {
            var lineArray = line.ToArray();
            if (lineArray.Length <= 1) return;
            lines.Add(new RnmLine(lineArray, startUV1, endUV1));
        }

        public void AddRangeLine(IEnumerable<RnmLine> linesArg)
        {
            lines.AddRange(linesArg);
        }
        

        public RnmContour Calculate()
        {
            if (lines.Count == 0)
            {
                logger.Log("skipping RnmContour because no lines exist.");
                return new RnmContour(material);
            }
            RemoveDuplicateOrReverseLine();
            // RemoveDuplicateVertices(VerticesWeldDistThreshold);
            SortByNearestEdgeDist();
            var contour = new RnmContour(material);
            // 最初の線を追加します。
            var line = lines[0];
            line.IsProcessed = true;
            bool lineDirectionLast; // 次に繋げるのが末尾方向ならtrue, 最初方向ならfalseです。
            if (FirstDirection(lines.ToArray(), line))
            {
                contour.AddVertices(line);
                lineDirectionLast = true; 
            }
            else
            {
                contour.AddVertices(line.Reverse());
                lineDirectionLast = false;
            }
            
            
            // 線の末尾について、もっとも近い点を探して繋いでいきます。
            while (lines.Any(l => !l.IsProcessed))
            {
                var remaining = lines.Where(l => !l.IsProcessed).ToArray();
                
                int minID = NearestLine(remaining, lineDirectionLast, line, out bool rConnectedDirectionLast);
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
                return new RnmContour(material);
            }

            if (!contour.IsClockwise())
            {
                contour.Reverse();
            }
            return contour;
        }
        
        /// <summary>
        /// 未処理の線の中で、最も近い線を探します。
        /// </summary>
        private int NearestLine(RnmLine[] remaining, bool lineDirectionLast, RnmLine line, out bool rConnectedDirectionLast)
        {
            int minID = -1;
            float minDist = float.MaxValue;
            rConnectedDirectionLast = false;
            var v = lineDirectionLast ? line[^1] : line[0]; // 繋げたい線の起点
            for (int i = 0; i < remaining.Length; i++)
            {
                var r = remaining[i];
                var distFirst = Vector3.Distance(v.Position, r[0].Position);
                var distLast = Vector3.Distance(v.Position, r[^1].Position);
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
            return minID;
        }

        /// <summary>
        /// 計算で最初に追加する線を、どちら向きに追加すべきか
        /// </summary>
        private bool FirstDirection(RnmLine[] lines, RnmLine firstLine)
        {
            var firstV = firstLine[0];
            var firstVLast = firstLine[^1];
            float dist = float.MaxValue;
            float distLast = float.MaxValue;
            foreach (var l in lines.Where(line => line != firstLine))
            {
                dist = Math.Min(dist, Vector3.Distance(firstV.Position, l[0].Position));
                dist = Math.Min(dist, Vector3.Distance(firstV.Position, l[^1].Position));
                distLast = Math.Min(distLast, Vector3.Distance(firstVLast.Position, l[0].Position));
                distLast = Math.Min(distLast, Vector3.Distance(firstVLast.Position, l[^1].Position));
            }

            return distLast < dist;
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
                foreach (var v1 in new Vector3[] { line[0].Position, line[^1].Position })
                {
                    if (line2.Count <= 0) continue;
                    foreach (var v2 in new Vector3[] { line2[0].Position, line2[^1].Position })
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
                var nextLine = new List<RnmVertex>(line1.Count);
                foreach (var v1 in line1)
                {
                    bool isDuplicate = false;
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        var line2 = lines[j];
                        foreach (var v2 in line2)
                        {
                            if (Vector3.Distance(v1.Position, v2.Position) < threshold)
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