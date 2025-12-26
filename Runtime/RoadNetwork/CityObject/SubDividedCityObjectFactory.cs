using PLATEAU.CityAdjust.NonLibData;
using PLATEAU.CityAdjust.NonLibDataHolder;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.Native;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using CityObjectList = PLATEAU.PolygonMesh.CityObjectList;
using Mesh = UnityEngine.Mesh;

namespace PLATEAU.RoadNetwork.CityObject
{
    public class SubDividedCityObjectFactory
    {
        /*
         * UnityMeshToDllModelConverterからコピペして作成している
         */
        private class CityObjectEntry
        {
            /// <summary>
            /// もとのPLATEAUCityObjectGroup
            /// </summary>
            public PLATEAUCityObjectGroup CityObjectGroup { get; }
            
            /// <summary>
            /// CityObjectGroupのtransformをキャッシュしたもの(高速化の為)
            /// </summary>
            public Transform CachedTransform { get; }

            /// <summary>
            /// PLATEAUContourMeshを考慮したMesh.
            /// 元の都市モデルのメッシュがそのまま入っている
            /// </summary>
            public Mesh SourceMesh { get; }

            /// <summary>
            /// 現在のCityObjectGroupが持つMeshFilterのメッシュ.
            /// 高さ合わせ機能やサブディビジョンサーフィス等で加工されている可能性もある。なにもされていない場合はUnityMeshと同じ。
            /// </summary>
            public Mesh CurrentMesh { get; }

            private CityObjectEntry(PLATEAUCityObjectGroup cog, Mesh sourceMesh, Mesh currentMesh)
            {
                CityObjectGroup = cog;
                CachedTransform = cog ? cog.transform : null;
                SourceMesh = sourceMesh;
                CurrentMesh = currentMesh;    
            }
            
            public static CityObjectEntry Create(PLATEAUCityObjectGroup cog, bool useContourMesh)
            {
                var currentMesh = cog.GetComponent<MeshFilter>()?.sharedMesh;
                if (!currentMesh)
                    return null;
                if (currentMesh.triangles.Length % 3 != 0)
                {
                    Debug.LogWarning("Meshの三角形の数が3の倍数ではありません");
                    return null;
                }

                for (int i = 0; i < currentMesh.subMeshCount; i++)
                {
                    var submesh = currentMesh.GetSubMesh(i);
                    if (submesh.indexCount % 3 != 0)
                    {
                        Debug.LogWarning("SubMeshの三角形の数が3の倍数ではありません");
                        return null;
                    }
                }
                var sourceMesh = currentMesh;
                if (useContourMesh)
                {
                    var contourMesh = cog.GetComponent<PLATEAUContourMesh>();
                    if (contourMesh)
                    {
                        var m = contourMesh.contourMesh;
                        sourceMesh = new UnityEngine.Mesh
                        {
                            vertices = m.vertices,
                            triangles = m.triangles,
                            uv4 = m.uv4,
                            uv = new Vector2[m.uv4.Length],
                            colors = new Color[m.uv4.Length]
                        };
                    }
                }

                return new CityObjectEntry(cog, sourceMesh, currentMesh);
            }

        }

        /// <summary>
        /// 引数で与えられたゲームオブジェクトとその子(再帰的)を <see cref="Model"/> に変換して返します。
        /// </summary>
        /// <param name="srcTransforms">変換対象ゲームオブジェクトのルートです。</param>
        /// <param name="unityMeshToDllSubMeshConverter"></param>
        /// <param name="exportDisabledGameObj">false のとき、非Activeのものは対象外とします。</param>
        /// <param name="vertexConverter">頂点座標を変換するメソッドで、 Vector3 から PlateauVector3d に変換する方法を指定します。</param>
        /// /// <param name="invertMesh">true : Mesh Normalを反転します</param>
        private static Model Convert(List<CityObjectEntry> srcTransforms, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            bool exportDisabledGameObj, AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh = false)
        {
            var model = Model.Create();
            foreach (var trans in srcTransforms)
            {
                ConvertRecursive(null, trans, model, unityMeshToDllSubMeshConverter, exportDisabledGameObj, vertexConverter, invertMesh);
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
        private static void ConvertRecursive(Node parentNode, CityObjectEntry trans, Model model, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            bool exportDisabledGameObj, AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh)
        {
            bool activeInHierarchy = trans.CachedTransform.gameObject.activeInHierarchy;
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
        }

        private static Node GameObjToNode(CityObjectEntry trans, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter,
            AdderAndCoordinateSystemConverter vertexConverter, bool invertMesh)
        {
            // ノード生成します。
            var node = Node.Create(trans.CachedTransform.name);
            node.IsActive = trans.CachedTransform.gameObject.activeInHierarchy;
            var localPos = vertexConverter.Convert(trans.CachedTransform.localPosition).ToPlateauVector();
            node.LocalPosition = localPos;

            // スケールの座標変換の考え方
            // 例えば、UnityでLocalScaleが(2,3,4)だったとする。これをWUNに変換するとき、XYZの値を入れ替えて(2,4,3)としたい。
            // そこで単純に座標系変換処理をかけると(-2,4,3)となり、マイナスが余計である。
            // 余計なマイナスを取り除くために、(1,1,1)を同じ座標系に変換して(-1,1,1)とし、それでアダマール積をとることで座標変換のマイナスを除く。
            node.LocalScale = vertexConverter.ConvertOnlyCoordinateSystem(trans.CachedTransform.localScale).ToPlateauVector() *
                              vertexConverter.ConvertOnlyCoordinateSystem(Vector3.one).ToPlateauVector();

            var srcEuler = trans.CachedTransform.localRotation.eulerAngles;
            var convertedEuler = vertexConverter.ConvertOnlyCoordinateSystem(srcEuler);
            if (invertMesh)
            {
                // メッシュが裏返るような座標軸に変換する場合、Rotationを反転します。
                convertedEuler *= -1;
            }
            node.LocalRotation = Quaternion.Euler(convertedEuler).ToPlateauQuaternion();


            PolygonMesh.Mesh nativeMesh = null;
            // メッシュを変換します。
            nativeMesh = ConvertMesh(
                trans.SourceMesh, trans.CityObjectGroup, unityMeshToDllSubMeshConverter,
                vertexConverter, invertMesh);

            int subMeshCount = trans.SourceMesh.subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = trans.SourceMesh.GetSubMesh(i);
                if (subMesh.indexCount == 0) continue;
                int startId = subMesh.indexStart;
                int endId = startId + subMesh.indexCount - 1;
                Assert.IsTrue(startId < endId);
                Assert.IsTrue(endId < nativeMesh.IndicesCount);
            }


            // CityObjectListを作って渡します。
            if (trans.CityObjectGroup != null && nativeMesh != null)
            {
                AttachCityObjectGroupToNativeMesh(trans.CityObjectGroup, nativeMesh);
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
            using var cityObjList = PolygonMesh.CityObjectList.Create();
            var allCityObjs = cityObjGroup.GetAllCityObjects();
            foreach (var cityObj in allCityObjs)
            {
                int primaryID = cityObj.CityObjectIndex[0];
                int atomicID = cityObj.CityObjectIndex[1];
                cityObjList.Add(new CityObjectIndex(primaryID, atomicID), cityObj.GmlID);
            }

            nativeMesh.CityObjectList = cityObjList;
        }

        private static PolygonMesh.Mesh ConvertMesh(Mesh unityMesh, PLATEAUCityObjectGroup cog, IUnityMeshToDllSubMeshConverter unityMeshToDllSubMeshConverter, VertexConverterBase vertexConverter, bool invertMesh)
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


            if (invertMesh)
            {
                for (int i = 0; i < plateauIndices.Length; i += 3)
                {
                    var first = plateauIndices[i];
                    var third = plateauIndices[i + 2];
                    plateauIndices[i] = third;
                    plateauIndices[i + 2] = first;
                }
            }

            var dllSubMeshes = unityMeshToDllSubMeshConverter.Convert(unityMesh, null);
            Assert.AreEqual(plateauUv1.Length, plateauVertices.Length);

            var dllMesh = PolygonMesh.Mesh.Create(
                plateauVertices, plateauIndices, plateauUv1, plateauUv4,
                dllSubMeshes.ToArray());
            // 補足:
            // 上の行で MergeMeshInfo に渡す実引数 includeTexture が常に true になっていますが、それで良いです。
            // 上の処理で テクスチャを含める/含めない の設定に即した SubMesh がすでにできているので、
            // C++側で特別にテクスチャを除く処理は不必要だからです。

            dllMesh.SetVertexColors(vertexColors);


            // CityObjectListを追加します
            if (cog != null)
            {
                using var col = CityObjectList.Create();
                foreach (var co in cog.GetAllCityObjects())
                {
                    var objId = co.CityObjectIndex;
                    col.Add(new CityObjectIndex(objId[0], objId[1]), co.GmlID);
                }

                dllMesh.CityObjectList = col;
            }

            return dllMesh;
        }

        public static bool TryGetHeight(Vector2 v, Mesh mesh, out float height)
        {
            height = 0f;
            var v3 = new Vector3[3];
            var v2 = new Vector2[3];
            var d = new Vector2[3];
            var e = new Vector2[3];
            var cross = new float[3];
            // TODO : 全探索しているので効率悪い
            for (var index = 0; index < mesh.triangles.Length; index += 3)
            {
                for (var x = 0; x < 3; x++)
                {
                    v3[x] = mesh.vertices[mesh.triangles[index + x]];
                    v2[x] = v3[x].Xz();
                }

                // 辺のベクトル
                for (var x = 0; x < 3; ++x)
                {
                    d[x] = v2[(x + 1) % 3] - v2[x];
                    e[x] = v2[x] - v;

                    if (e[x].sqrMagnitude < 0.01f)
                    {
                        height = v3[x].y;
                        return true;
                    }

                    cross[x] = Vector2Ex.Cross(e[x], d[x]);
                }

                // 三角形の内部にある場合
                if (cross.All(c => c >= 0) || cross.All(c => c <= 0))
                {
                    var n = Vector3.Cross(v3[1] - v3[0], v3[2] - v3[1]).normalized;
                    var a = v.Xay() - v3[0];
                    var nd = Vector3.Dot(n, a);
                    var dd = a - nd * n;
                    height = (v3[0] + dd).y;
                    return true;
                }
            }

            return false;
        }

        [Serializable]
        internal class ConvertCityObjectResult
        {
            public List<SubDividedCityObject> ConvertedCityObjects { get; } = new List<SubDividedCityObject>();
        }


        /// <summary>
        /// cityObjectGroupsをSubDividedCityObjectに変換する
        /// </summary>
        /// <param name="cityObjectGroups"></param>
        /// <param name="useContourMesh"></param>
        /// <returns></returns>
        internal static ConvertCityObjectResult ConvertCityObjects(IEnumerable<PLATEAUCityObjectGroup> cityObjectGroups, bool useContourMesh = true)
        {
            using var progressBar = new ProgressDisplayDialogue();
            using var _ = new DebugTimer("ConvertCityObjects");
            // NOTE : CityGranularityConverterを参考
            var cityObjectEntries = new List<CityObjectEntry>();
            foreach (var cityObjectGroup in cityObjectGroups)
            {
                var info = CityObjectEntry.Create(cityObjectGroup, useContourMesh);
                if (info != null)
                {
                    cityObjectEntries.Add(info);
                }
            }
            // 最小単位に分解する
            var nativeOption = new GranularityConvertOption(ConvertGranularity.PerAtomicFeatureObject, 1);

            
            // 属性情報を記憶しておく
            var transformList = new UniqueParentTransformList(cityObjectEntries.Select(c => c.CachedTransform).ToArray());
            var attributes = new GmlIdToSerializedCityObj();
            attributes.ComposeFrom(transformList);

            var unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithEmptyMaterial();
            
            
            var subObjects = new List<SubDividedCityObject>();
            
            // 進行度を表示する為に総数を出す
            var allCount = cityObjectEntries.Sum(c => c.CityObjectGroup.PrimaryCityObjects.Count());
            var currentWorkIndex = 0;
            
            // https://synesthesias.slack.com/archives/C0261M64C15/p1762011824752139?thread_ts=1758790969.985829&cid=C0261M64C15
            // #NOTE : LOD2に属する道路モデルがLOD3のPLATEAUCityObjectGroupの属性情報にも入っている模様
            //       : そのため、SubDividedCityObjectGroupの親のPLATEAUCityObjectGroupがどれになるのかが判断が難しい
            //       : なので、個別に分解したうえで、最後にメッシュを含まないものだけ抽出するようにする
            foreach (var co in cityObjectEntries)
            {
                // ゲームオブジェクトを共通ライブラリのModelに変換します。
                using var srcModel = Convert(
                    new List<CityObjectEntry>{co},
                    unityMeshToDllSubMeshConverter,
                    // 非表示のゲームオブジェクトも対象に含めます。なぜなら、LOD0とLOD1のうちLOD1だけがActiveになっているという状況で、変換後もToolkitsのLOD機能を使えるようにするためです。
                    true,
                    VertexConverterFactory.NoopConverter());

                // 共通ライブラリの機能でモデルを分割・結合します。
                var converter = new GranularityConverter();
                using var dstModel = converter.Convert(srcModel, nativeOption);
                var getter = new SerializedCityObjectGetterFromDict(attributes, dstModel);
                var attrHelper = new AttributeDataHelper(getter, true);

                // 一つの大きなSubDividedCityObjectを作る
                var entireSubDividedCityObject = new SubDividedCityObject(dstModel, attrHelper);
                
                // 高速化の為に参照テーブルを作っておく
                Dictionary<string, SubDividedCityObject> nameToSubDividedCityObject = new Dictionary<string, SubDividedCityObject>();
                foreach (var child in entireSubDividedCityObject.GetAllChildren())
                {
                    if (child == null)
                        continue;
                    nameToSubDividedCityObject.TryAdd(child.Name, child);
                }
                
                if (co.CachedTransform)
                {
                    // これもないと主要地物インポートの時に外れる道路があった
                    Apply(co, nameToSubDividedCityObject.GetValueOrDefault(co.CachedTransform.name),co.CachedTransform.name);
                
                    // 主要地物単位でグルーピングする
                    foreach (var root in co.CityObjectGroup.PrimaryCityObjects)
                    {
                        progressBar.SetProgress("最小地物分解", 100f * currentWorkIndex++ / allCount, "オブジェクト分解中");
                        Apply(co, nameToSubDividedCityObject.GetValueOrDefault(root.GmlID),  root.GmlID);
                    }
                }
               
                // 最小単位(子の存在しない)かつメッシュが存在するものだけを抽出する
                // #NOTE : さすがにメッシュがあって同じGmlIDを持つものは存在しない前提
                subObjects.AddRange(entireSubDividedCityObject.GetAllChildren()
                    .Where(c => c.Children.Any() == false && c.Meshes.Any()));
            }
            
            var ret = new ConvertCityObjectResult();
            ret.ConvertedCityObjects.AddRange(subObjects);
            for (var i = 0; i < ret.ConvertedCityObjects.Count; ++i)
            {
                var c = ret.ConvertedCityObjects[i];
                progressBar.SetProgress("最小地物分解", 100f * i / ret.ConvertedCityObjects.Count, "頂点チェック中");
                // 全く同じ頂点を結合する
                foreach (var m in c.Meshes)
                    m.VertexReduction();
            }

            return ret;
            
            // subDividedCityObjectに対応するCityObjectGroup/RnCItyObjectGroupKeyの紐づけを行う
            // また、メッシュが高さ合わせされている場合にそっちから高さ情報を取ってくる
            void Apply(CityObjectEntry co, SubDividedCityObject subDividedCityObject, string gmlId)
                {
                    if (subDividedCityObject == null)
                        return;
                    
                    subDividedCityObject.SetCityObjectGroup(co.CityObjectGroup, new RnCityObjectGroupKey(gmlId));
                    if (co.CurrentMesh != co.SourceMesh)
                    {
                        try
                        {
                            var co1 = co;

                            void Visit(SubDividedCityObject subCog)
                            {
                                foreach (var m in subCog.Meshes)
                                {
                                    for (var i = 0; i < m.Vertices.Count; i++)
                                    {
                                        var v = m.Vertices[i];
                                        if (TryGetHeight(v.Xz(), co1.CurrentMesh, out var height))
                                        {
                                            v.y = height;
                                        }
                                        else
                                        {
                                            DebugEx.LogError($"Failed to get height / {subCog.Name}");
                                        }

                                        m.Vertices[i] = v;
                                    }
                                }

                                foreach (var c in subCog.Children)
                                {
                                    Visit(c);
                                }
                            }
                            Visit(subDividedCityObject);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
        }


    }
}