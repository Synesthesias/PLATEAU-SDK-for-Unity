using System.Collections.Generic;
using System.IO;
using System.Linq;
using PLATEAU.Geometries;
using PLATEAU.Interop;
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
        public static Model Convert(GameObject go)
        {
            var trans = go.transform;
            var model = Model.Create();
            ConvertRecursive(null, trans, model);
            return model;
        }

        /// <summary>
        /// Transform とその子を再帰的に <see cref="Node"/> に変換します。
        /// Unityのゲームオブエジェクトのヒエラルキーと <see cref="Node"/> の木構造が対応するようにします。
        /// </summary>
        /// <param name="parentNode"> <paramref name="trans"/> の親 Transform に対応する親 Node です。親がない（ルート）のときは null にします。</param>
        /// <param name="trans">このゲームオブジェクトとその子を再帰的に <see cref="Node"/> にします。</param>
        /// <param name="model"> <paramref name="parentNode"/> が null のとき、<see cref="Node"/> は <paramref name="model"/> に追加されます。</param>
        private static void ConvertRecursive(Node parentNode, Transform trans, Model model)
        {
            var node = GameObjToNode(trans);

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
                ConvertRecursive(node, childTrans, model);
            }
        }

        private static Node GameObjToNode(Transform trans)
        {
            var node = Node.Create(trans.name);
            var meshFilter = trans.GetComponent<MeshFilter>();
            if (meshFilter == null) return node;
            var unityMesh = meshFilter.sharedMesh;
            if (unityMesh == null) return node;
            
            var dllMesh = ConvertMesh(unityMesh, trans.GetComponent<MeshRenderer>());
            
            int subMeshCount = unityMesh.subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = unityMesh.GetSubMesh(i);
                if (subMesh.indexCount == 0) continue;
                int startId = subMesh.indexStart;
                int endId = startId + subMesh.indexCount - 1;
                Assert.IsTrue(startId < endId);
                Assert.IsTrue(endId < dllMesh.IndicesCount);
                dllMesh.AddSubMesh("", startId, endId); // TODO ここにテクスチャパスが来るようにする
            }
            node.SetMeshByCppMove(dllMesh);
            
            return node;
        }

        private static PolygonMesh.Mesh ConvertMesh(Mesh unityMesh, MeshRenderer meshRenderer)
        {
            var vertices =
                unityMesh
                    .vertices
                    .Select(vert => new PlateauVector3d(vert.x, vert.y, vert.z))
                    .ToArray();
            var indices =
                unityMesh.triangles
                    .Select(id => (uint)id)
                    .ToArray();
            var uv1 =
                unityMesh
                    .uv
                    .Select(uv => new PlateauVector2f(uv.x, uv.y))
                    .ToArray();

            Material[] materials = null;
            if (meshRenderer != null) materials = meshRenderer.sharedMaterials;

            int subMeshCount = unityMesh.subMeshCount;
            var dllSubMeshes = new List<SubMesh>();
            for (int i = 0; i < subMeshCount; i++)
            {
                var unitySubMesh = unityMesh.GetSubMesh(i);
                int startIndex = unitySubMesh.firstVertex;
                int endIndex = startIndex + unitySubMesh.indexCount - 1;

                // テクスチャパスは、Unityシーン内のテクスチャの名前に記載してあるので取得します。
                string texturePath = "";
                if (materials != null && i < materials.Length)
                {
                    var material = materials[i];
                    if (material == null) continue;
                    var texture = material.mainTexture;
                    if (texture == null) continue;
                    texturePath = Path.Combine(PathUtil.plateauSrcFetchDir, texture.name);
                }
                dllSubMeshes.Add(SubMesh.Create(startIndex, endIndex, texturePath));
            }

            var dllMesh = PolygonMesh.Mesh.Create(unityMesh.name);
            dllMesh.MergeMeshInfo(
                vertices, indices, uv1,
                dllSubMeshes.ToArray(),
                CoordinateSystem.EUN, true);
            return dllMesh;
        }
    }
}
