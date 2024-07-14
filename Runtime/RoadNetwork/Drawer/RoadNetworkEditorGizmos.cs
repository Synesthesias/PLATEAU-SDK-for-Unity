using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU
{
    public class RoadNetworkEditorGizmos : MonoBehaviour
    {
        public Vector3[] linePairs = new Vector3[0];
        public Color color = Color.red;

        public void OnDrawGizmos()
        {
            if (linePairs.Length >= 2 && linePairs.Length % 2 == 0)
            {
                var col = Gizmos.color;
                Gizmos.color = color;
                //Handles.DrawLines(linePairs);
                Gizmos.DrawLineList(linePairs);
                Gizmos.color = col;
            }
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
