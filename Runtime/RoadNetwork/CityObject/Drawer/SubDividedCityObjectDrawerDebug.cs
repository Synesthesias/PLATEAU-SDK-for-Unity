using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Mesh.Drawer
{
    public class SubDividedCityObjectDrawerDebug
    {
        // --------------------
        // start:フィールド
        // --------------------
        public bool visible = false;
        public int meshColorNum = 16;
        public bool showVertexIndex = false;
        public int showVertexIndexFontSize = 8;
        // --------------------
        // end:フィールド
        // --------------------


        internal void DrawMesh(SubDividedCityObject.Mesh mesh, SubDividedCityObject.SubMesh subMesh, Matrix4x4 mat, Color? color = null,
            float duration = 0f, bool depthTest = true)
        {
            for (var j = 0; j < subMesh.Triangles.Count; j += 3)
            {
                var v0 = mesh.Vertices[subMesh.Triangles[j]];
                var v1 = mesh.Vertices[subMesh.Triangles[j + 1]];
                var v2 = mesh.Vertices[subMesh.Triangles[j + 2]];

                var v = new[]
                {
                    mat * v0,
                    mat * v1,
                    mat * v2
                }.Select(x => new Vector3(x.x, x.y, x.z));
                DebugEx.DrawLines(v, true, color, duration, depthTest);
            }
        }

        public void DrawConvertedCityObject(List<SubDividedCityObject> cityObjects)
        {
            if (!visible)
                return;
            var index = 0;
            foreach (var item in cityObjects)
            {
                if (item.Visible == false)
                {
                    // インデックスは進めておかないとvisible切り替わるたびに色代わるの辛い
                    index += item.Meshes.Sum(m => m.SubMeshes.Count);
                    continue;
                }

                foreach (var mesh in item.Meshes)
                {
                    if (showVertexIndex)
                    {
                        //foreach (var v in mesh.Vertices)
                        for (var i = 0; i < mesh.Vertices.Count; ++i)
                        {
                            var v = mesh.Vertices[i];
                            DebugEx.DrawString($"{i}", v.PutY(v.y + i * 0.01f), fontSize: showVertexIndexFontSize);
                        }
                    }

                    foreach (var subMesh in mesh.SubMeshes)
                    {
                        DrawMesh(mesh, subMesh, Matrix4x4.identity, color: DebugEx.GetDebugColor(index, meshColorNum));
                        index++;
                    }
                }
            }
        }
    }
}