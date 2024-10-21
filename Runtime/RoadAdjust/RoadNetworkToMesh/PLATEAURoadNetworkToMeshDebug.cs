using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    public class PLATEAURoadNetworkToMeshDebug : MonoBehaviour
    {
        [SerializeField] private RnmContourMeshList contoursMeshList;
        private const float ArrowLength = 3f;
        private const float ArrowAngle = 10f;
        private static readonly Color ArrowColor1 = Color.green;
        private static readonly Color ArrowColor2 = new Color(0f, 0.6f, 0f);

        internal void Init(RnmContourMeshList contoursMeshListArg)
        {
            contoursMeshList = contoursMeshListArg;
        }

        internal void Init(RnmContourMesh contourMeshArg)
        {
            var list = new RnmContourMeshList();
            list.Add(contourMeshArg);
            contoursMeshList = list;
        }

        private void OnDrawGizmos()
        {
            if (contoursMeshList == null) return;
            
            foreach (var contourMesh in contoursMeshList)
            {
                foreach (var contour in contourMesh)
                {
                    if (contour.Count == 0) continue;
                    for (int i = 0; i < contour.Count - 1; i++)
                    {
                        DrawArrow(contour[i], contour[i + 1]);
                    }
                    DrawArrow(contour[^1], contour[0]);
                }
                
            }
        }

        private void DrawArrow(Vector3 start, Vector3 end)
        {
            var prevColor = Gizmos.color;

            // 本体
            Gizmos.color = ArrowColor1;
            Gizmos.DrawLine(start, end);

            // 矢印の先
            Gizmos.color = ArrowColor2;
            var rotated = Quaternion.Euler(0, 180-ArrowAngle, 0) * (end - start);
            rotated = rotated.normalized * ArrowLength;
            Gizmos.DrawLine(end, end + rotated);
            
            var rotated2 = Quaternion.Euler(0, -(180-ArrowAngle), 0) * (end - start);
            rotated2 = rotated2.normalized * ArrowLength;
            Gizmos.DrawLine(end, end + rotated2);
            
            Gizmos.color = prevColor;
        }
    }
}