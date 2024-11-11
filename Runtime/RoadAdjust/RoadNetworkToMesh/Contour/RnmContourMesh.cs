using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// <see cref="RnmContour"/>を複数保持して1つのメッシュに相当するものです。
    /// </summary>
    [Serializable]
    internal class RnmContourMesh : IEnumerable<RnmContour>
    {
        [SerializeField] private List<RnmContour> contours = new();
        [SerializeField] private GameObject sourceObject;

        public GameObject SourceObject => sourceObject;

        public RnmContourMesh(GameObject sourceObject) { this.sourceObject = sourceObject; }

        public RnmContourMesh(GameObject sourceObject, IEnumerable<RnmContour> contours)
            : this(sourceObject)
        {
            this.contours = contours.ToList();
        }

        public RnmContourMesh(GameObject sourceObject, RnmContour contour)
            : this(sourceObject)
        {
            this.contours = new List<RnmContour> { contour };
        }

        public int Count => contours.Count;
        public RnmContour this[int index] => contours[index];
        public void Add(RnmContour c) => contours.Add(c);

        public void AddRange(RnmContourMesh c)
        {
            foreach (var contour in c.contours) Add(contour);
        }

        public IEnumerator<RnmContour> GetEnumerator() => contours.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary> <see cref="RnmContourMesh"/>を複数保持します。 </summary>
    [Serializable]
    internal class RnmContourMeshList : IEnumerable<RnmContourMesh>
    {
        [SerializeField] private List<RnmContourMesh> meshes = new();
        public int Count => meshes.Count;

        public RnmContourMeshList(){}

        public RnmContourMeshList(IEnumerable<RnmContourMesh> contourMeshes)
        {
            this.meshes = contourMeshes.ToList();
        }
        
        public RnmContourMesh this[int index] => meshes[index];
        public void Add(RnmContourMesh c) => meshes.Add(c);

        public void AddRange(RnmContourMeshList c)
        {
            foreach (var cMesh in c.meshes) Add(cMesh);
        }
        

        public IEnumerator<RnmContourMesh> GetEnumerator() => meshes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}