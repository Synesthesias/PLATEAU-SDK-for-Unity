using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.Assertions;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.Editor.CityExport.ModelConvert
{
    /// <summary>
    /// Unityのシーンに配置されたモデルを <see cref="Model"/> に変換します。
    /// <see cref="Model"/> とは、ゲームエンジンとDLL側でモデルデータをやりとりするための中間データ構造です。
    /// </summary>
    internal static class UnityMeshToDllModelConverter
    {
        public delegate PlateauVector3d VertexConvertFunc(Vector3 src);
        
        /// <summary>
        /// 引数で与えられたゲームオブジェクトとその子(再帰的)を <see cref="Model"/> に変換して返します。
        /// </summary>
        /// <param name="go">変換対象ゲームオブジェクトのルートです。</param>
        /// <param name="includeTexture"></param>
        /// <param name="exportDisabledGameObj">false のとき、非Activeのものは対象外とします。</param>
        /// <param name="doInvertTriangles">ポリゴン内の頂点番号の配置を逆転させることで、ポリゴンの表裏を逆転させます。</param>
        /// <param name="meshAxis">座標系です。</param>
        /// <param name="vertexConvertFunc">頂点座標を変換するメソッドで、 Vector3 から PlateauVector3d に変換する方法を指定します。</param>
        public static Model Convert(GameObject go, bool includeTexture, bool exportDisabledGameObj, bool doInvertTriangles, CoordinateSystem meshAxis, VertexConvertFunc vertexConvertFunc)
        {
            var trans = go.transform;
            var model = Model.Create();
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var childTrans = trans.GetChild(i);
                ConvertRecursive(null, childTrans, model, includeTexture, exportDisabledGameObj, doInvertTriangles, meshAxis, vertexConvertFunc);
            }
            
            return model;
        }

        /// <summary>
        /// Transform とその子を再帰的に <see cref="Node"/> に変換します。
        /// Unityのゲームオブエジェクトのヒエラルキーと <see cref="Node"/> の木構造が対応するようにします。
        /// </summary>
        /// <param name="parentNode"> <paramref name="trans"/> の親 Transform に対応する親 Node です。親がない（ルート）のときは null にします。</param>
        /// <param name="trans">このゲームオブジェクトとその子を再帰的に <see cref="Node"/> にします。</param>
        /// <param name="model"> <paramref name="parentNode"/> が null のとき、<see cref="Node"/> は <paramref name="model"/> に追加されます。</param>
        /// <param name="includeTexture"></param>
        /// <param name="exportDisabledGameObj">false のとき、ActiveでないGameObjectは対象から除外します。</param>
        /// <param name="doInvertTriangles"></param>
        /// <param name="meshAxis"></param>
        /// <param name="vertexConvertFunc"></param>
        private static void ConvertRecursive(Node parentNode, Transform trans, Model model, bool includeTexture, bool exportDisabledGameObj, bool doInvertTriangles, CoordinateSystem meshAxis, VertexConvertFunc vertexConvertFunc)
        {
            if ((!trans.gameObject.activeInHierarchy) && (!exportDisabledGameObj)) return;

            // メッシュを変換して Node を作ります。
            var node = GameObjToNode(trans, includeTexture, doInvertTriangles, meshAxis, vertexConvertFunc);

            if (parentNode == null)
            {
                // ルートのときは Model に Node を追加します。
                model.AddNodeByCppMove(node);
                node.Dispose();
                node = model.GetRootNodeAt(model.RootNodesCount - 1);
                
            }
            else
            {
                // ルートでないときは Node に Node を追加します。
                parentNode.AddChildNodeByCppMove(node);
                node.Dispose();
                node = parentNode.GetChildAt(parentNode.ChildCount - 1);
            }

            int numChild = trans.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                ConvertRecursive(node, childTrans, model, includeTexture, exportDisabledGameObj, doInvertTriangles, meshAxis, vertexConvertFunc);
            }
        }

        private static Node GameObjToNode(Transform trans, bool includeTexture, bool doInvertTriangles, CoordinateSystem meshAxis, VertexConvertFunc vertexConvertFunc)
        {
            var node = Node.Create(trans.name);
            var meshFilter = trans.GetComponent<MeshFilter>();
            if (meshFilter == null) return node;
            var unityMesh = meshFilter.sharedMesh;
            if (unityMesh == null) return node;
            if (unityMesh.vertexCount <= 0 || unityMesh.subMeshCount <= 0 || unityMesh.triangles.Length <= 0) return node;

            var dllMesh = ConvertMesh(unityMesh, trans.GetComponent<MeshRenderer>(), includeTexture, doInvertTriangles, meshAxis, vertexConvertFunc);
            
            int subMeshCount = unityMesh.subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = unityMesh.GetSubMesh(i);
                if (subMesh.indexCount == 0) continue;
                int startId = subMesh.indexStart;
                int endId = startId + subMesh.indexCount - 1;
                Assert.IsTrue(startId < endId);
                Assert.IsTrue(endId < dllMesh.IndicesCount);
            }
            node.SetMeshByCppMove(dllMesh);
            
            return node;
        }

        private static PolygonMesh.Mesh ConvertMesh(Mesh unityMesh, MeshRenderer meshRenderer, bool includeTexture, bool doInvertTriangles, CoordinateSystem meshAxis, VertexConvertFunc vertexConvertFunc)
        {
            var vertices =
                unityMesh
                    .vertices
                    .Select(vert => vertexConvertFunc(vert))
                    .ToArray();
            var indices =
                unityMesh.triangles
                    .Select(id => (uint)id)
                    .ToArray();
            if (doInvertTriangles)
            {
                InvertTriangles(indices);
            }
            var uv1 =
                unityMesh
                    .uv
                    .Select(uv => new PlateauVector2f(uv.x, uv.y))
                    .ToArray();

            Material[] materials = null;
            if (meshRenderer != null) materials = meshRenderer.sharedMaterials;

            int subMeshCount = unityMesh.subMeshCount;
            var dllSubMeshes = new List<SubMesh>();
            if (includeTexture)
            {
                for (int i = 0; i < subMeshCount; i++)
                {
                    var unitySubMesh = unityMesh.GetSubMesh(i);
                    int startIndex = unitySubMesh.indexStart;
                    int endIndex = startIndex + unitySubMesh.indexCount - 1;
                    if (startIndex >= endIndex) continue;
                    Assert.IsTrue(startIndex < endIndex);
                    Assert.IsTrue(endIndex < indices.Length);
                    Assert.IsTrue(startIndex < indices.Length);
                    Assert.IsTrue(0 <= startIndex);

                    // テクスチャパスは、Unityシーン内のテクスチャの名前に記載してあるので取得します。
                    string texturePath = "";
                    if (materials != null && i < materials.Length)
                    {
                        texturePath = GetTexturePathFromMaterialName(materials[i]);
                    }
                    dllSubMeshes.Add(SubMesh.Create(startIndex, endIndex, texturePath));
                
                }
            }
            else
            { // テクスチャを含めない設定のとき、サブメッシュはただ1つです。
                dllSubMeshes.Add(SubMesh.Create(0, indices.Length - 1, ""));
            }
            
            
            Assert.AreEqual(uv1.Length, vertices.Length);

            var dllMesh = PolygonMesh.Mesh.Create(unityMesh.name);
            dllMesh.MergeMeshInfo(
                vertices, indices, uv1,
                dllSubMeshes.ToArray(),
                CoordinateSystem.EUN, meshAxis, true);
            // 補足:
            // 上の行で MergeMeshInfo に渡す実引数 includeTexture が常に true になっていますが、それで良いです。
            // 上の処理で テクスチャを含める/含めない の設定に即した SubMesh がすでにできているので、
            // C++側で特別にテクスチャを除く処理は不必要だからです。
            
            return dllMesh;
        }
        
        private static string GetTexturePathFromMaterialName(Material mat)
        {
            if (mat == null) return "";
            var tex = mat.mainTexture;
            if (tex == null) return "";
            return Path.Combine(PathUtil.PLATEAUSrcFetchDir, tex.name);
        }

        private static void InvertTriangles(IList<uint> indices)
        {
            Assert.IsTrue(indices.Count % 3 == 0);
            int numTriangle = indices.Count / 3;
            for (int i = 0; i < numTriangle; i++)
            {
                // ポリゴン内の順番をスワップ
                (indices[3 * i], indices[3 * i + 2]) = (indices[3 * i + 2], indices[3 * i]);
            }
        }
    }
}
