using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 道路ネットワークの編集機能のギズモを描画する
    /// </summary>
    public class RoadNetworkEditorGizmos : MonoBehaviour
    {
        private List<System.Action> drawFuncs = new List<System.Action>();
        public List<System.Action> DrawFuncs { get { return drawFuncs; } }

        public List<RnWay> LaneCeterWay { get; set; } = new List<RnWay>();

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            var preZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            var preCol = Gizmos.color;


            foreach (var func in DrawFuncs)
            {
                func();
            }

            Gizmos.color = preCol;
            Handles.zTest = preZTest;
#endif
        }
    }
}
