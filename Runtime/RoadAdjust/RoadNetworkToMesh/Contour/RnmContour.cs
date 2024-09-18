using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークから導かれる輪郭線であり、多角形を構成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    [Serializable]
    public class RnmContour
    {
        [SerializeField] private List<Vector3> vertices = new ();
        
        public RnmContour(IEnumerable<Vector3> vertices)
        {
            this.vertices = vertices.ToList();
        }
        
        public RnmContour(){}

        public int Count => vertices.Count;
        public Vector3 this[int index] => vertices[index];
        public void AddVertices(IEnumerable<Vector3> v) => vertices.AddRange(v);
    }

    /// <summary> <see cref="RnmContour"/>を複数保持します。 </summary>
    [Serializable]
    public class RnmContourList : IEnumerable<RnmContour>
    {
        [SerializeField] private List<RnmContour> contours = new();
        public int Count => contours.Count;
        public RnmContour this[int index] => contours[index];
        public void Add(RnmContour c) => contours.Add(c);

        public void AddRange(RnmContourList c)
        {
            foreach (var contour in c.contours) Add(contour);
        }

        public IEnumerator<RnmContour> GetEnumerator() => contours.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    /// <summary>
    /// 複数の線を受け取り、それらの線をつないで多角形 <see cref="RnmContour"/> を形成します。
    /// RnmはRoadNetworkToMeshの略です。
    /// </summary>
    public class RnmContourCalculator
    {
        private List<RnmLine> lines = new ();

        public void AddLine(IEnumerable<Vector3> line)
        {
            var lineArray = line.ToArray();
            if (lineArray.Length < 2) return; // 計算の都合上、2点は必要
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
            var contour = new RnmContour();
            // 最初の線を追加
            var line = lines[0];
            contour.AddVertices(line.Vertices);
            line.IsProcessed = true;
            bool lineDirectionLast = true; // 次に繋げるのが末尾方向ならtrue, 最初方向ならfalse
            
            // 線の末尾について、もっとも近い点を探して繋いでいく
            while (lines.Any(l => !l.IsProcessed))
            {
                var remaining = lines.Where(l => !l.IsProcessed).ToArray();
                int minID = -1;
                float minDist = float.MaxValue;
                var v = lineDirectionLast ? line[^1] : line[0];
                bool rConnectedDirectionLast = false;
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

            return contour;
        }

        /// <summary> 線1つに計算用のデータを付与したデータ構造です。 </summary>
        private class RnmLine
        {
            public Vector3[] Vertices { get; }
            public bool IsProcessed { get; set; } = false;
            
            public RnmLine(Vector3[] vertices)
            {
                Vertices = vertices;
            }
            
            public Vector3 this[int index] => Vertices[index];
            public int Count => Vertices.Length;
        }
    }
}