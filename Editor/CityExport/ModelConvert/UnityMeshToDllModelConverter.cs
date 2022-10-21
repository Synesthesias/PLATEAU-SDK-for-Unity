using System.Linq;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.PolygonMesh;
using UnityEditor.UI;
using UnityEngine;
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
            var node = ConvertGameObj(trans);
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

        private static Node ConvertGameObj(Transform trans)
        {
            var node = Node.Create(trans.name);
            var meshFilter = trans.GetComponent<MeshFilter>();
            if (meshFilter == null) return node;
            var unityMesh = meshFilter.sharedMesh;
            if (unityMesh == null) return node;
            var dllMesh = ConvertMesh(unityMesh);
            node.SetMeshByCppMove(dllMesh);
            return node;
        }

        private static PolygonMesh.Mesh ConvertMesh(Mesh unityMesh)
        {
            var vertices =
                unityMesh
                    .vertices
                    .Select(vert => new PlateauVector3d(vert.x, vert.y, vert.z))
                    .ToArray();
            var indices =
                unityMesh
                    .GetIndices(0) // TODO 複数の subMesh に対応
                    .Select(id => (uint)id)
                    .ToArray();
            var uv1 =
                unityMesh
                    .uv
                    .Select(uv => new PlateauVector2f(uv.x, uv.y))
                    .ToArray();

            var dllMesh = PolygonMesh.Mesh.Create(unityMesh.name);
            dllMesh.MergeMeshInfo(vertices, indices, uv1, CoordinateSystem.EUN, true);
            return dllMesh;
        }
    }
}
