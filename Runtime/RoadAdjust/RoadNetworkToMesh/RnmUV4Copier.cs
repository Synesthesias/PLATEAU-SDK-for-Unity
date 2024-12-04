using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路のUV4をコピーします。
    /// </summary>
    public class RnmUV4Copier
    {
        private const float DistanceMinDiff = 0.01f;

        public void Copy(IEnumerable<GameObject> srcObjsArg, GameObject dst)
        {
            // 必要なコンポーネントがなければ何もせず終了します。
            var srcObjs = srcObjsArg.ToArray();
            if (srcObjs.Length == 0 || dst == null) return;
            var dstMeshFilter = dst.GetComponent<MeshFilter>();
            if (dstMeshFilter == null) return;
            var dstMesh = dstMeshFilter.sharedMesh;
            if (dstMesh == null || dstMesh.vertexCount == 0) return;
            var dstVerts = dstMesh.vertices;
            var dstTriangles = dstMesh.triangles;
            
            // srcObjsが複数あるケースに対応するため、srcObjsを結合して1つのsrcとします。
            var srcVerts = new List<Vector3>();
            var srcTriangles = new List<int>();
            var srcUV4 = new SrcUV4List();
            foreach (var srcObj in srcObjs)
            {
                var srcMeshFilter = srcObj.GetComponent<MeshFilter>();
                if (srcMeshFilter == null) continue;
                var srcMesh = srcMeshFilter.sharedMesh;
                if (srcMesh == null) continue;
                if (srcMesh.vertexCount == 0) continue;
                if (srcMesh.uv4 == null) continue;
                int vertIDOffset = srcVerts.Count;
                srcVerts.AddRange(srcMesh.vertices);
                srcUV4.AddRange(srcMesh.uv4);
                srcTriangles.AddRange(srcMesh.triangles.Select(id => id + vertIDOffset));
            }
            

            

            // UV4コピーの方針:
            // 処理1 : dstの各頂点について、(X,Z)座標が近いsrcの頂点のUV4をコピーします。
            // ただし、同じ位置に重なっている複数頂点が異なるUV4を持っている場合が多いため、これだけでは不正確になります。
            // そこで、同じような距離にある頂点候補が複数ある箇所については、処理1の結果を未確定(候補複数)とします。
            // 処理2 : 未確定の頂点については、未確定頂点と辺で繋がっている確定頂点のUV4と近いものを参考UV4とし、
            //        距離が近い複数頂点のうちUV4が参考UV4に近いものを採用します。
            // 処理3 : それでも求まらない箇所は、重心でdstとsrcを比較して決めます。

            // 処理1
            var dstUV4 = new DstUV4List(dstVerts.Length);
            int srcUV4Count = srcUV4.Count;
            for (int i = 0; i < dstVerts.Length; i++)
            {
                var dstV = dstVerts[i];
                var nearestSrcID = UniqueNearestVertexID(dstV, srcVerts, srcUV4);
                if (0 <= nearestSrcID && nearestSrcID < srcUV4Count)
                {
                    dstUV4.Determine(i, srcUV4.Get(nearestSrcID));
                }
                else if (nearestSrcID >= srcUV4Count)
                {
                    Debug.LogError("Invalid nearestSrcID");
                }
                // nearestSrcIDが負の場合は未確定のままです。
            }

            // 処理2
            bool anyDetermined;
            do
            {
                anyDetermined = false;
                for (int dstVertID = 0; dstVertID < dstVerts.Length; dstVertID++)
                {
                    if (!dstUV4.IsDetermined(dstVertID)) continue;
                    for (int dstTriangleID = 0; dstTriangleID < dstTriangles.Length / 3; dstTriangleID++)
                    {
                        int determinedTri = -1;
                        for (int tri = 0; tri < 3; tri++)
                        {
                            if (dstTriangles[dstTriangleID * 3 + tri] == dstVertID)
                            {
                                determinedTri = tri;
                            }
                        }

                        if (determinedTri < 0)
                        {
                            continue;
                        }

                        int determinedVertID = dstTriangles[dstTriangleID * 3 + determinedTri];
                        for (int tri = 0; tri < 3; tri++)
                        {
                            if (tri == determinedTri) continue;
                            // 隣接頂点を発見
                            int dstAdjacentVertID = dstTriangles[dstTriangleID * 3 + tri];
                            if (dstUV4.IsDetermined(dstAdjacentVertID)) continue;
                            
                            var candidateSrcIDs = CandidateSrcVerticesID(dstVerts[dstAdjacentVertID], srcVerts)
                                .ToArray();
                            var minDiff = float.MaxValue;
                            var matchingUV4 = new UV4Int(-9997, -9997);
                            int matchingSrcVertID = -1;
                            foreach (var candidateID in candidateSrcIDs)
                            {
                                var uv4 = dstUV4[determinedVertID];
                                var diff = Vector2.Distance(srcUV4.Get(candidateID).ToFloat, uv4.ToFloat);
                                if (diff < minDiff)
                                {
                                    minDiff = diff;
                                    matchingUV4 = uv4;
                                    matchingSrcVertID = candidateID;
                                }
                            }

                            if (matchingSrcVertID >= 0)
                            {
                                dstUV4.Determine(dstAdjacentVertID, matchingUV4);
                                anyDetermined = true;
                            }
                        }
                    }
                }
            } while (anyDetermined);
            
            
            // 処理3
            for(int dstTriID=0 ; dstTriID < dstTriangles.Length / 3; dstTriID++)
            {
                int[] triDstVertIDs = new int[]
                {
                    dstTriangles[dstTriID * 3],
                    dstTriangles[dstTriID * 3 + 1],
                    dstTriangles[dstTriID * 3 + 2]
                };
                var dstV1 = dstVerts[triDstVertIDs[0]];
                var dstV2 = dstVerts[triDstVertIDs[1]];
                var dstV3 = dstVerts[triDstVertIDs[2]];
                bool allDetermined = triDstVertIDs.All(id => dstUV4.IsDetermined(id));
                if (allDetermined) continue;
                // 未確定の頂点のみで構成される三角形について
                var dstCenter = (dstV1 + dstV2 + dstV3) / 3;
                bool srcTriFound = false;
                for (int srcTriID = 0; srcTriID < srcTriangles.Count / 3; srcTriID++)
                {
                    var srcV1 = srcVerts[srcTriangles[srcTriID * 3]];
                    var srcV2 = srcVerts[srcTriangles[srcTriID * 3 + 1]];
                    var srcV3 = srcVerts[srcTriangles[srcTriID * 3 + 2]];
                    if (!IsInsideTriangle(dstCenter, srcV1, srcV2, srcV3))
                    {
                        continue;
                    }

                    srcTriFound = true;

                    var srcUV4Tri = new UV4Int[]
                    {
                        srcUV4.Get(srcTriangles[srcTriID * 3]),
                        srcUV4.Get(srcTriangles[srcTriID * 3 + 1]),
                        srcUV4.Get(srcTriangles[srcTriID * 3 + 2]),
                    };
                    for(int i=0; i<3; i++)
                    {
                        if (dstUV4.IsDetermined(triDstVertIDs[i])) continue;
                        dstUV4.Determine(triDstVertIDs[i], srcUV4Tri[i]);
                    }

                    break;
                }
                
                // それでも見つからなかったら、(0,0)にします
                if (!srcTriFound)
                {
                    for(int i=0; i<3; i++)
                    {
                        if (dstUV4.IsDetermined(triDstVertIDs[i])) continue;
                        dstUV4.Determine(triDstVertIDs[i], new UV4Int(0, 0));
                    }
                }
            }

            dstMesh.uv4 = dstUV4.UV4;
        }


        /// <summary>
        /// <paramref name="srcVertices"/>のうち、<paramref name="v"/>に一番近い点のインデックスを返します。
        /// ただし、一番近い点と距離が近しい点が複数あってUV4が異なる場合は、同定できないとして-1を返します。
        /// </summary>
        private int UniqueNearestVertexID(Vector3 v, List<Vector3> srcVertices, SrcUV4List srcUV4)
        {
            // 一番近い点を探します。
            var (nearestID, nearestDist) = NearestVertex(v, srcVertices);

            // 一番近い点と距離が近しい点を数えます。
            int matchCount = 0;
            var firstMatchUV4 = new UV4Int(-9996, -9996);
            for (int i = 0; i < srcVertices.Count; i++)
            {
                var dist = Vector2.Distance(v.Xz(), srcVertices[i].Xz());
                if (dist - nearestDist < DistanceMinDiff)
                {
                    matchCount++;
                    if (matchCount == 1)
                    {
                        firstMatchUV4 = srcUV4.Get(i);
                    }
                    if (matchCount >= 2)
                    {
                        if (Vector2.Distance(firstMatchUV4.ToFloat, srcUV4.Get(i).ToFloat) > 0.1)
                        {
                            return -1;
                        }
                    }
                }
            }

            return nearestID;
        }

        private IEnumerable<int> CandidateSrcVerticesID(Vector3 dstV, List<Vector3> srcVertices)
        {
            var (_, nearestDist) = NearestVertex(dstV, srcVertices);
            var candidates = new List<int>();
            for (int i = 0; i < srcVertices.Count; i++)
            {
                var dist = Vector2.Distance(dstV.Xz(), srcVertices[i].Xz());
                if (dist - nearestDist < DistanceMinDiff)
                {
                    candidates.Add(i);
                }

                if (dist - nearestDist < 0)
                {
                    Debug.LogError("Invalid distance");
                }
            }

            return candidates;
        }
        

        private (int nearestID, float nearestDist) NearestVertex(Vector3 v, List<Vector3> vertices)
        {
            float nearestDist = float.MaxValue;
            int nearestID = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                var dist = Vector2.Distance(v.Xz(), vertices[i].Xz());
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestID = i;
                }
            }

            return (nearestID, nearestDist);
        }

        /// <summary> UV4で重要なのは整数部分なので、整数だけでUV4を保存・比較する構造体を定義します。 </summary>
        private struct UV4Int
        {
            private int X { get; }
            private int Y { get; }

            public UV4Int(Vector2 uv4Float)
            {
                X = Mathf.RoundToInt(uv4Float.x);
                Y = Mathf.RoundToInt(uv4Float.y);
            }

            public UV4Int(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Vector2 ToFloat => new Vector2(X, Y);
            
            public override bool Equals(object obj)
            {
                if (obj is not UV4Int other) return false;
                return X == other.X && Y == other.Y;
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ (Y*1000).GetHashCode();
            }
        }

        

        private class DstUV4List
        {
            private class DstUV4
            {
                public UV4Int UV4 { get; private set; } = new(-9998, -9998);
                public bool Determined { get; private set; } = false;


                public void Determine(UV4Int uv4)
                {
                    UV4 = uv4;
                    Determined = true;
                }
            }
            
            private DstUV4[] uv4List;
            private HashSet<UV4Int> DeterminedUV4Set { get; }

            public DstUV4List(int vertCount)
            {
                uv4List = new DstUV4[vertCount];
                for (int i = 0; i < vertCount; i++)
                {
                    uv4List[i] = new DstUV4();
                }

                DeterminedUV4Set = new HashSet<UV4Int>();
            }


            public void Determine(int dstVertID, UV4Int uv4)
            {
                var v = uv4List[dstVertID];
                v.Determine(uv4);
                if (!DeterminedUV4Set.Contains(uv4))
                {
                    DeterminedUV4Set.Add(uv4);
                }
            }

            public bool IsDetermined(int vertID)
            {
                return uv4List[vertID].Determined;
            }

            public UV4Int this[int vertID]
            {
                get => uv4List[vertID].UV4;
            }
            
            public Vector2[] UV4 => uv4List.Select(u => u.UV4.ToFloat).ToArray();

            public int Count => uv4List.Length;
        }

        private class SrcUV4
        {
            public UV4Int UV4 { get; }

            public SrcUV4(Vector2 uv4)
            {
                UV4 = new UV4Int(uv4);
            }
        }

        private class SrcUV4List
        {
            private readonly List<SrcUV4> uv4List = new ();

            public void Add(Vector2 uv4FloatElement)
            {
                uv4List.Add(new SrcUV4(uv4FloatElement));
            }

            public void AddRange(Vector2[] uv4Float)
            {
                foreach (var uv4Elem in uv4Float)
                {
                    Add(uv4Elem);
                }
            }
            

            public UV4Int Get(int srcVertID)
            {
                return uv4List[srcVertID].UV4;
            }

            public int Count => uv4List.Count;

        }

        /// <summary> 三角形abcの内部にpがあるかです。XZ軸のみ考慮し、yは考慮しません。 </summary>
        private bool IsInsideTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = (b - a).Xz();
            var bp = (p - b).Xz();

            var bc = (c - b).Xz();
            var cp = (p - c).Xz();

            var ca = (a - c).Xz();
            var ap = (p - a).Xz();

            // 外積
            float c1 = ab.x * bp.y - ab.y * bp.x;
            float c2 = bc.x * cp.y - bc.y * cp.x;
            float c3 = ca.x * ap.y - ca.y * ap.x;

            return (c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0);
        }
    }
}