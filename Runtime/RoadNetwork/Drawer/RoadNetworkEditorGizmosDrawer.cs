using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
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


        void DrawDashedArrows(IEnumerable<Vector3> vertices, bool isLoop = false, Color? color = null,
            float lineLength = 3f, float spaceLength = 1f)
        {
            const float yOffset = 0.0f;
            DebugEx.DrawDashedArrows(vertices.Select(v => v.PutY(v.y + yOffset)), isLoop, color, lineLength, spaceLength);
        }


    }
}
