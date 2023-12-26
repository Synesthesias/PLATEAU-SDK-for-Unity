using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using UnityEngine;
using UnityEngine.Assertions;
using CityObjectList = PLATEAU.PolygonMesh.CityObjectList;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.CityExport.ModelConvert
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
        /// <param name="gameObjs">変換対象ゲームオブジェクトのルートです。</param>
        /// <param name="unityMeshToDllSubMeshConverter"></param>
        /// <param name="exportDisabledGameObj">false のとき、非Activeのものは対象外とします。</param>
        /// <param name="vertexConvertFunc">頂点座標を変換するメソッドで、 Vector3 から PlateauVector3d に変換する方法を指定します。</param>
        /// /// <param name="invertMesh">true : Mesh Normalを反転します</param>
        public static Model Convert(IEnumerable<GameObject> gameObjs, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter, bool exportDisabledGameObj, VertexConvertFunc vertexConvertFunc, bool InvertMesh = false)
        {
            var model = Model.Create();
            foreach(var go in gameObjs)
            {
                var trans = go.transform;
                ConvertRecursive(null, trans, model, unityMeshToDllSubMeshConverter , exportDisabledGameObj, vertexConvertFunc, InvertMesh);
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
        /// <param name="unityMeshToDllSubMeshConverter"></param>
        /// <param name="exportDisabledGameObj">false のとき、ActiveでないGameObjectは対象から除外します。</param>
        /// <param name="vertexConvertFunc"></param>
        /// <param name="invertMesh">true : Mesh Normalを反転します</param>
        private static void ConvertRecursive(Node parentNode, Transform trans, Model model, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            bool exportDisabledGameObj, VertexConvertFunc vertexConvertFunc, bool invertMesh)
        {
            bool activeInHierarchy = trans.gameObject.activeInHierarchy;
            if ((!activeInHierarchy) && (!exportDisabledGameObj)) return;

            // メッシュを変換して Node を作ります。
            var node = GameObjToNode(trans, unityMeshToDllSubMeshConverter, vertexConvertFunc, invertMesh);

            if (parentNode == null)
            {
                // ルートのときは Model に Node を追加します。
                model.AddNodeByCppMove(node);
                node = model.GetRootNodeAt(model.RootNodesCount - 1);
                
            }
            else
            {
                // ルートでないときは Node に Node を追加します。
                parentNode.AddChildNodeByCppMove(node);
                node = parentNode.GetChildAt(parentNode.ChildCount - 1);
            }

            node.IsActive = activeInHierarchy; 

            int numChild = trans.childCount;
            for (int i = 0; i < numChild; i++)
            {
                var childTrans = trans.GetChild(i);
                ConvertRecursive(node, childTrans, model, unityMeshToDllSubMeshConverter, exportDisabledGameObj, vertexConvertFunc, invertMesh);
            }
        }

        private static Node GameObjToNode(Transform trans, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            VertexConvertFunc vertexConvertFunc, bool invertMesh)
        {
            // ノード生成します。
            var node = Node.Create(trans.name);
            node.IsActive = trans.gameObject.activeInHierarchy;
                
            // ゲームオブジェクトにメッシュがあるかどうか判定します。
            bool hasMesh = false;
            Mesh unityMesh = null;
            var meshFilter = trans.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                unityMesh = meshFilter.sharedMesh;
                if (unityMesh != null)
                {
                    hasMesh = true;
                }
            }

            PolygonMesh.Mesh nativeMesh = null;
            if (hasMesh)
            {
                // メッシュを変換します。
                nativeMesh = ConvertMesh(unityMesh, trans.GetComponent<MeshRenderer>(), unityMeshToDllSubMeshConverter, vertexConvertFunc, invertMesh);
            
                int subMeshCount = unityMesh.subMeshCount;
                for (int i = 0; i < subMeshCount; i++)
                {
                    var subMesh = unityMesh.GetSubMesh(i);
                    if (subMesh.indexCount == 0) continue;
                    int startId = subMesh.indexStart;
                    int endId = startId + subMesh.indexCount - 1;
                    Assert.IsTrue(startId < endId);
                    Assert.IsTrue(endId < nativeMesh.IndicesCount);
                }
            }
            
            // メッシュがなくとも、PLATEAUCityObjectGroupがあれば、CityObjectListを付与するための空のメッシュを用意します。
            var cityObjGroup = trans.GetComponent<PLATEAUCityObjectGroup>();
            if ((!hasMesh) && cityObjGroup != null)
            {
                nativeMesh = PolygonMesh.Mesh.Create("");
            }
            
            // CityObjectListを作って渡します。
            if (cityObjGroup != null && nativeMesh != null)
            {
                AttachCityObjectGroupToNativeMesh(cityObjGroup, nativeMesh);
            }
            
            if (nativeMesh != null)
            {
                node.SetMeshByCppMove(nativeMesh);
            }
            
            return node;
        }

        /// <summary>
        /// <see cref="PLATEAUCityObjectGroup"/>を<see cref="CityObjectList"/>に変換し、それをnativeMeshに追加します。
        /// </summary>
        private static void AttachCityObjectGroupToNativeMesh(PLATEAUCityObjectGroup cityObjGroup, PolygonMesh.Mesh nativeMesh)
        {
            using var cityObjList = CityObjectList.Create();
            var allCityObjs = cityObjGroup.GetAllCityObjects();
            foreach (var cityObj in allCityObjs)
            {
                int primaryID = cityObj.CityObjectIndex[0];
                int atomicID = cityObj.CityObjectIndex[1];
                cityObjList.Add(new CityObjectIndex(primaryID, atomicID), cityObj.GmlID);
            }

            nativeMesh.CityObjectList = cityObjList;
        }

        private static PolygonMesh.Mesh ConvertMesh(Mesh unityMesh, MeshRenderer meshRenderer,
            IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            VertexConvertFunc vertexConvertFunc, bool invertMesh)
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
            var uv1 =
                unityMesh
                    .uv
                    .Select(uv => new PlateauVector2f(uv.x, uv.y))
                    .ToArray();
            var uv4 =
                unityMesh
                    .uv4
                    .Select(uv => new PlateauVector2f(uv.x, uv.y))
                    .ToArray();

            if (invertMesh)
            {
                for(int i = 0; i < indices.Length; i += 3)
                {
                    var first = indices[i];
                    var third = indices[i+2];
                    indices[i] = third; 
                    indices[i+2] = first;
                }
            }

            Material[] materials = null;
            if (meshRenderer != null) materials = meshRenderer.sharedMaterials;

            var dllSubMeshes = unityMeshToDllSubMeshConverter.Convert(unityMesh, meshRenderer);
            
            
            Assert.AreEqual(uv1.Length, vertices.Length);

            var dllMesh = PolygonMesh.Mesh.Create(
                vertices, indices, uv1, uv4,
                dllSubMeshes.ToArray());
            // 補足:
            // 上の行で MergeMeshInfo に渡す実引数 includeTexture が常に true になっていますが、それで良いです。
            // 上の処理で テクスチャを含める/含めない の設定に即した SubMesh がすでにできているので、
            // C++側で特別にテクスチャを除く処理は不必要だからです。
            
            return dllMesh;
        }
    }
}
