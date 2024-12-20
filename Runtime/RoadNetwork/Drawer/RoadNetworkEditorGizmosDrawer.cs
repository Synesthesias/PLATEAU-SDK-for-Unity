using PLATEAU.Editor.RoadNetwork.EditingSystemSubMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 道路ネットワークの編集機能のギズモを描画する
    /// </summary>
    public class RoadNetworkEditorGizmos : MonoBehaviour
    {
        private List<ILaneLineDrawer> lines = new();

        public void Clear()
        {
            lines.Clear();
        }

        public void SetLine(IEnumerable<ILaneLineDrawer> linesArg)
        {
            lines = linesArg.ToList();
        }

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            var preZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;


            foreach (var l in lines)
            {
                l.Draw();
            }
            Handles.zTest = preZTest;
#endif
        }
    }
}
