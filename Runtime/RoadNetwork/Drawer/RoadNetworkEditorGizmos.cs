using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU
{
    public class RoadNetworkEditorGizmos : MonoBehaviour
    {
        public List<Vector3> intersectionConnectionLinePairs = new List<Vector3>();
        public List<List<Vector3>> intersectionConnectionLinePairs2 = new List<List<Vector3>>();
        public Color connectionColor = Color.blue;

        //public List<Vector3> intersections = new List<Vector3>();
        //public Color intersectionColor = Color.green;
        //public float intersectionRadius = 4.0f;

        //public float btnSize = 2.0f;

        public void OnDrawGizmos()
        {
            var preCol = Gizmos.color;

            var nParis = intersectionConnectionLinePairs.Count;
            if (nParis >= 2 && nParis % 2 == 0)
            {
                Gizmos.color = connectionColor;
                //Handles.DrawLines(linePairs);
                Gizmos.DrawLineList(intersectionConnectionLinePairs.ToArray());
            }

            var nParis2 = intersectionConnectionLinePairs2.Count;
            if (nParis2 >= 2)
            {
                Gizmos.color = connectionColor;
                foreach (var item in intersectionConnectionLinePairs2)
                {
                    Gizmos.DrawLineList(item.ToArray());
                }
                //Handles.DrawLines(linePairs);
            }

            //    var ee = intersectionConnectionLinePairs.GetEnumerator();
            //    while (ee.MoveNext())
            //    {
            //        var p1 = ee.Current;
            //        if (ee.MoveNext())
            //        {
            //            var p2 = ee.Current;
            //            var btnP = (p1 + p2) / 2.0f;
            //            if (Handles.Button(btnP, Quaternion.identity, btnSize, btnSize, Handles.SphereHandleCap)){
            //                Debug.Log("Button Clicked");
            //            }
            //        }
            //    }

            //    if (intersections.Count > 0)
            //    {
            //        Gizmos.color = intersectionColor;
            //        foreach (var i in intersections)
            //        {
            //            Gizmos.DrawSphere(i, intersectionRadius);
            //        }
            //    }


            Gizmos.color = preCol;
        }


        //// Start is called before the first frame update
        //void Start()
        //{

        //}

        //// Update is called once per frame
        //void Update()
        //{

        //}
    }
}
