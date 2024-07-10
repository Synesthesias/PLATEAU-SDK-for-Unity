using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
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
        
        /// <summary>
        /// 引数で与えられたゲームオブジェクトとその子(再帰的)を <see cref="Model"/> に変換して返します。
        /// </summary>
        /// <param name="srcTransforms">変換対象ゲームオブジェクトのルートです。</param>
        /// <param name="unityMeshToDllSubMeshConverter"></param>
        /// <param name="exportDisabledGameObj">false のとき、非Activeのものは対象外とします。</param>
        /// <param name="vertexConverter">頂点座標を変換するメソッドで、 Vector3 から PlateauVector3d に変換する方法を指定します。</param>
        /// /// <param name="invertMesh">true : Mesh Normalを反転します</param>
        public static Model Convert(UniqueParentTransformList srcTransforms, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            bool exportDisabledGameObj, AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh = false)
        {
            var model = Model.Create();
            foreach(var trans in srcTransforms.Get)
            {
                ConvertRecursive(null, trans, model, unityMeshToDllSubMeshConverter , exportDisabledGameObj, vertexConverter, invertMesh);
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
        /// <param name="vertexConverter"></param>
        /// <param name="invertMesh">true : Mesh Normalを反転します</param>
        private static void ConvertRecursive(Node parentNode, Transform trans, Model model, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            bool exportDisabledGameObj, AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh)
        {
            bool activeInHierarchy = trans.gameObject.activeInHierarchy;
            if ((!activeInHierarchy) && (!exportDisabledGameObj)) return;

            // メッシュを変換して Node を作ります。
            var node = GameObjToNode(trans, unityMeshToDllSubMeshConverter, vertexConverter, invertMesh);

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
                ConvertRecursive(node, childTrans, model, unityMeshToDllSubMeshConverter, exportDisabledGameObj, vertexConverter, invertMesh);
            }
        }

        private static Node GameObjToNode(Transform trans, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh)
        {
            // ノード生成します。
            var node = Node.Create(trans.name);
            node.IsActive = trans.gameObject.activeInHierarchy;
            var localPos = vertexConverter.Convert(trans.localPosition).ToPlateauVector();
            node.LocalPosition = localPos;
            
            // スケールの座標変換の考え方
            // 例えば、UnityでLocalScaleが(2,3,4)だったとする。これをWUNに変換するとき、XYZの値を入れ替えて(2,4,3)としたい。
            // そこで単純に座標系変換処理をかけると(-2,4,3)となり、マイナスが余計である。
            // 余計なマイナスを取り除くために、(1,1,1)を同じ座標系に変換して(-1,1,1)とし、それでアダマール積をとることで座標変換のマイナスを除く。
            node.LocalScale = vertexConverter.ConvertOnlyCoordinateSystem(trans.localScale).ToPlateauVector() *
                              vertexConverter.ConvertOnlyCoordinateSystem(Vector3.one).ToPlateauVector();

            var srcEuler = trans.localRotation.eulerAngles;
            var convertedEuler = vertexConverter.ConvertOnlyCoordinateSystem(srcEuler);
            if (invertMesh)
            {
                // メッシュが裏返るような座標軸に変換する場合、Rotationを反転します。
                convertedEuler *= -1;
            }
            node.LocalRotation = Quaternion.Euler(convertedEuler).ToPlateauQuaternion();
                
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
                nativeMesh = ConvertMesh(
                    unityMesh, trans.GetComponent<MeshRenderer>(), unityMeshToDllSubMeshConverter,
                    vertexConverter, invertMesh);
            
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
            VertexConverterBase vertexConverter, bool invertMesh)
        {
            // 頂点
            var vertCount = unityMesh.vertexCount;
            var unityVertices = unityMesh.vertices;
            var plateauVertices = new PlateauVector3d[vertCount];
            for (int i = 0; i < vertCount; i++)
            {
                plateauVertices[i] = vertexConverter.Convert(unityVertices[i]).ToPlateauVector();
            }
            
            // indices
            var unityIndices = unityMesh.triangles;
            var indexCount = unityIndices.Length;
            var plateauIndices = new uint[indexCount];
            for (int i = 0; i < indexCount; i++)
            {
                plateauIndices[i] = (uint)unityIndices[i];
            }
            
            // uv1
            var unityUv1 = unityMesh.uv;
            var uv1Count = unityUv1.Length;
            var plateauUv1 = new PlateauVector2f[uv1Count];
            for (int i = 0; i < uv1Count; i++)
            {
                var u = unityUv1[i];
                plateauUv1[i] = new PlateauVector2f(u.x, u.y);
            }
            
            // uv4
            var unityUv4 = unityMesh.uv4;
            var uv4Count = unityUv4.Length;
            var plateauUv4 = new PlateauVector2f[uv4Count];
            for (int i = 0; i < uv4Count; i++)
            {
                var u = unityUv4[i];
                plateauUv4[i] = new PlateauVector2f(u.x, u.y);
            }
            
            // vertexColor
            var unityColors = unityMesh.colors;
            var vertColorCount = unityColors.Length;
            var vertexColors = new PlateauVector3d[vertColorCount];
            for (int i = 0; i < vertColorCount; i++)
            {
                var color = unityColors[i];
                vertexColors[i] = new PlateauVector3d(color.r, color.g, color.b);
            }
            
            // error check
            if(uv1Count != vertCount) Debug.LogError("uv1 count does not match.");
            if(uv4Count != vertCount) Debug.LogError("uv4 count does not match.");
            if(vertColorCount != vertCount) Debug.LogError("vert color count does not match.");

            if (invertMesh)
            {
                for(int i = 0; i < plateauIndices.Length; i += 3)
                {
                    var first = plateauIndices[i];
                    var third = plateauIndices[i+2];
                    plateauIndices[i] = third; 
                    plateauIndices[i+2] = first;
                }
            }

            Material[] materials = null;
            if (meshRenderer != null) materials = meshRenderer.sharedMaterials;

            var dllSubMeshes = unityMeshToDllSubMeshConverter.Convert(unityMesh, meshRenderer);
            
            
            Assert.AreEqual(plateauUv1.Length, plateauVertices.Length);

            var dllMesh = PolygonMesh.Mesh.Create(
                plateauVertices, plateauIndices, plateauUv1, plateauUv4,
                dllSubMeshes.ToArray());
            // 補足:
            // 上の行で MergeMeshInfo に渡す実引数 includeTexture が常に true になっていますが、それで良いです。
            // 上の処理で テクスチャを含める/含めない の設定に即した SubMesh がすでにできているので、
            // C++側で特別にテクスチャを除く処理は不必要だからです。
            
            dllMesh.SetVertexColors(vertexColors);
            
            return dllMesh;
        }
    }
}
