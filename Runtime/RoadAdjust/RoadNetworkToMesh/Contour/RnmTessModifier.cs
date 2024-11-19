using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// <see cref="RnmContour"/>からテッセレートしたあと、そのメッシュに変更を加えたいとき、どのように変更すれば良いかを定義します。
    /// </summary>
    internal interface IRnmTessModifier
    {
        void Apply(RnmTessMesh mesh, RnmContour contour);
    }
    
    /// <summary>
    /// メッシュを盛り上げて段差にします。 
    /// </summary>
    internal class RnmTessModifierPileUp : IRnmTessModifier
    {
        private Vector3[] edge;
        private float moveHeight;
        
        /// <summary>
        /// 盛り上げる高さと、どの線に沿って側面ポリゴンを貼れば良いかを指定します。
        /// </summary>
        public RnmTessModifierPileUp(Vector3[] edge, float moveHeight)
        {
            this.edge = edge;
            this.moveHeight = moveHeight;
        }
        
        public void Apply(RnmTessMesh mesh, RnmContour contour)
        {
            var modifiers = new List<IRnmTessModifier>
            {
                // 既存のメッシュを上に移動します。
                new RnmTessModifierMove(Vector3.up * moveHeight, contour),
                // 盛り上げた側面にポリゴンを追加します。
                new RnmTessModifierExtentLine(edge, Vector3.up * moveHeight)
            };

            foreach (var modifier in modifiers)
            {
                modifier.Apply(mesh, contour);
            }
        }
    }

    /// <summary>
    /// 線baseLineと、それを指定方向に指定距離だけ動かした線extentLineを結んで線ポリゴンを追加します。
    /// </summary>
    internal class RnmTessModifierExtentLine : IRnmTessModifier
    {
        private Vector3[] baseLine;
        private Vector3 extentVector;
        
        public RnmTessModifierExtentLine(Vector3[] baseLine, Vector3 extentVector)
        {
            this.baseLine = baseLine;
            this.extentVector = extentVector;
        }

        public void Apply(RnmTessMesh mesh, RnmContour contour)
        {
            var extentLine = baseLine.Select(b => b + extentVector).ToArray();
            int baseCount = baseLine.Length;
            
            // baseLineとextentLineの点を交互につないで線ポリゴンの頂点配列とします。
            var verts = new Vector3[baseCount * 2];
            for(int i = 0; i < baseCount; i++)
            {
                verts[i * 2] = baseLine[i];
                verts[i * 2 + 1] = extentLine[i];
            }

            var triangles = new int[(baseCount - 1) * 6];
            for (int i = 0; i < baseCount - 1; i++)
            {
                int v0 = i * 2;
                int v1 = i * 2 + 1;
                int v2 = i * 2 + 2;
                int v3 = i * 2 + 3;
                int tID = i * 6;
                triangles[tID] = v0;
                triangles[tID + 1] = v2;
                triangles[tID + 2] = v1;
                triangles[tID + 3] = v1;
                triangles[tID + 4] = v2;
                triangles[tID + 5] = v3;
            }

            var subMesh = new RnmTessSubMesh(verts, triangles, new Vector2[verts.Length],
                RnmMaterialType.IntersectionCarLane, contour.CopyWithoutModifier());
            mesh.Add(subMesh);
        }
    }

    /// <summary>
    /// 指定量だけメッシュを平行移動します。
    /// </summary>
    internal class RnmTessModifierMove : IRnmTessModifier
    {
        private Vector3 offset;
        private RnmContour targetContour;
        
        public RnmTessModifierMove(Vector3 offset, RnmContour targetContour)
        {
            this.offset = offset;
            this.targetContour = targetContour;
        }
        
        public void Apply(RnmTessMesh mesh, RnmContour contour)
        {
            foreach (var subMesh in mesh.SubMeshes)
            {
                if (subMesh.Contour != targetContour) continue;
                int vertCount = subMesh.Vertices.Length;
                for(int i=0; i<vertCount; i++)
                {
                    subMesh.Vertices[i] += offset;
                }
            }
        }
    }
}