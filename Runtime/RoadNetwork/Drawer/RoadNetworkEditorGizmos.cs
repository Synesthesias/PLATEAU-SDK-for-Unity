using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    public class RoadNetworkEditorGizmos : MonoBehaviour
    {
        private List<System.Action> drawFuncs = new List<System.Action>();
        public List<System.Action> DrawFuncs { get { return drawFuncs; } }

        //public List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();
        //public List<List<Vector3>> intersectionConnectionLinePairs2 = new List<List<Vector3>>();
        //public Color connectionColor = Color.blue;

        //public List<Vector3> intersections = new List<Vector3>();
        //public Color intersectionColor = Color.green;
        //public float intersectionRadius = 4.0f;

        //public float btnSize = 2.0f;

        public void OnDrawGizmos()
        {
            var preCol = Gizmos.color;


            foreach (var func in DrawFuncs)
            {
                func();
            }

            Gizmos.color = preCol;
        }

    }
}
