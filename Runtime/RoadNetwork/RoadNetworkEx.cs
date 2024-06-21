﻿using Newtonsoft.Json;
using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityGML;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using static PLATEAU.RoadNetwork.ConvertedCityObject;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class ConvertedCityObject
    {
        [Serializable]
        public class SubMesh
        {
            [field: SerializeField]
            public List<int> Triangles { get; set; } = new List<int>();


            public List<SubMesh> Separate()
            {
                Dictionary<int, HashSet<int>> vertexToTriangle = new Dictionary<int, HashSet<int>>();
                for (var i = 0; i < Triangles.Count; i += 3)
                {
                    var t = i / 3;
                    for (var x = 0; x < 3; x++)
                    {
                        vertexToTriangle.GetValueOrCreate(Triangles[i + x]).Add(t);
                    }
                }

                var tris = Enumerable.Range(0, Triangles.Count / 3).ToList();

                List<SubMesh> subMeshes = new List<SubMesh>();
                while (tris.Any())
                {
                    var t = tris[0];
                    var triIndices = new List<int> { t };
                    for (var a = 0; a < triIndices.Count; ++a)
                    {
                        var tt = triIndices[a];
                        tris.Remove(tt);
                        for (var i = 0; i < 3; i++)
                        {
                            var v = Triangles[tt * 3 + i];
                            foreach (var ttt in vertexToTriangle[v])
                            {
                                if (tris.Contains(ttt) && triIndices.Contains(ttt) == false)
                                    triIndices.Add(ttt);
                            }
                        }
                    }
                    var subMesh = new SubMesh();
                    subMesh.Triangles.AddRange(
                        triIndices.OrderBy(t => t)
                            .SelectMany(t => Enumerable.Range(0, 3).Select(x => Triangles[t * 3 + x])));
                    subMeshes.Add(subMesh);
                }

                return subMeshes;
            }
        }

        [Serializable]
        public class ConvertedMesh
        {
            [field: SerializeField]
            public List<Vector3> Vertices { get; set; } = new List<Vector3>();

            [field: SerializeField]
            public List<SubMesh> SubMeshes { get; set; } = new List<SubMesh>();

            public void Merge(float epsilon)
            {
                GeoGraph2D.MergeMeshVertex(Vertices, (a, b) => (a - b).Xz().magnitude, epsilon, out var vert, out var indices);
                Vertices = vert;

                foreach (var s in SubMeshes)
                {
                    s.Triangles = s.Triangles.Select(i => indices[i]).ToList();
                }
            }

            public void Separate()
            {
                var newSubMesh = new List<SubMesh>();
                foreach (var m in SubMeshes)
                {
                    newSubMesh.AddRange(m.Separate());
                }
                SubMeshes = newSubMesh;
            }
        }

        [field: SerializeField, ReadOnly]
        public string Name { get; private set; }

        // Inspectorで表示するためにシリアライズしておく
        [TextArea(1, 20)]
        [SerializeField, ReadOnly]
        private string serializedCityObjects;

        private CityInfo.CityObjectList cityObjects;

        [field: SerializeField]
        public bool Visible { get; set; } = true;

        [field: SerializeField]
        public List<ConvertedMesh> Meshes { get; set; } = new List<ConvertedMesh>();

        [field: SerializeField]
        public List<ConvertedCityObject> Children { get; set; } = new List<ConvertedCityObject>();

        [field: SerializeField]
        public PLATEAUCityObjectGroup CityObjectGroup { get; private set; }

        public CityInfo.CityObjectList CityObjects
        {
            get
            {
                if (cityObjects != null)
                    return cityObjects;

                if (!string.IsNullOrEmpty(serializedCityObjects))
                    cityObjects = JsonConvert.DeserializeObject<CityInfo.CityObjectList>(serializedCityObjects);

                return cityObjects;
            }
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Model"/> から変換して
        /// <see cref="ConvertedGameObjData"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        internal ConvertedCityObject(Model plateauModel, AttributeDataHelper attributeDataHelper)
        {
            Name = "Root";
            attributeDataHelper.SetId(Name);
            Children = new List<ConvertedCityObject>();
            for (int i = 0; i < plateauModel.RootNodesCount; i++)
            {
                var rootNode = plateauModel.GetRootNodeAt(i);
                // 再帰的な子の生成です。
                Children.Add(new ConvertedCityObject(rootNode, attributeDataHelper.Copy()));
            }
        }

        /// <summary>
        /// C++側の <see cref="PolygonMesh.Node"/> から変換して
        /// <see cref="ConvertedCityObject"/> を作ります。
        /// 子も再帰的に作ります。
        /// </summary>
        private ConvertedCityObject(Node plateauNode, AttributeDataHelper attributeDataHelper)
        {
            //MeshData = MeshConverter.Convert(plateauNode.Mesh, plateauNode.Name);


            Name = plateauNode.Name;
            attributeDataHelper.SetId(Name);
            if (plateauNode != null)
            {
                attributeDataHelper.SetCityObjectList(plateauNode.Mesh.CityObjectList);
                if (plateauNode.Mesh != null)
                {
                    var m = plateauNode.Mesh;
                    var vertices = new List<Vector3>(m.VerticesCount);
                    for (int i = 0; i < m.VerticesCount; i++)
                    {
                        var v = m.GetVertexAt(i).ToUnityVector();
                        vertices.Add(v);
                    }

                    var totalIndexNum = 0;
                    for (var i = 0; i < m.SubMeshCount; ++i)
                    {
                        var subMesh = m.GetSubMeshAt(i);
                        var num = subMesh.EndIndex - subMesh.StartIndex + 1;
                        Assert.IsTrue(num % 3 == 0, "invalid triangles");
                        totalIndexNum += num;
                    }

                    var subMeshes = new List<SubMesh>(m.SubMeshCount);
                    for (var i = 0; i < m.SubMeshCount; ++i)
                    {
                        var subMesh = m.GetSubMeshAt(i);
                        var s = new SubMesh();
                        for (var j = subMesh.StartIndex; j <= subMesh.EndIndex; j += 3)
                        {
                            s.Triangles.Add(m.GetIndiceAt(j));
                            s.Triangles.Add(m.GetIndiceAt(j + 1));
                            s.Triangles.Add(m.GetIndiceAt(j + 2));
                        }
                        subMeshes.Add(s);
                    }
                    Meshes.Add(new ConvertedMesh
                    {
                        Vertices = vertices,
                        SubMeshes = subMeshes
                    });
                }
            }


            cityObjects = attributeDataHelper.GetSerializableCityObject();
            for (int i = 0; i < plateauNode.ChildCount; i++)
            {
                var child = plateauNode.GetChildAt(i);
                Children.Add(new ConvertedCityObject(child, attributeDataHelper.Copy()));
                attributeDataHelper.AddOutsideChildren(child?.Name);
            }
            serializedCityObjects = JsonConvert.SerializeObject(cityObjects, Formatting.Indented);
        }

        public IEnumerable<ConvertedCityObject> GetAllChildren()
        {
            if (Children == null)
                yield break;
            foreach (var c in Children)
            {
                yield return c;
                foreach (var cc in c.GetAllChildren())
                {
                    yield return cc;
                }
            }
        }

        public void SetCityObjectGroup(PLATEAUCityObjectGroup group)
        {
            CityObjectGroup = group;
            foreach (var c in Children)
                c.SetCityObjectGroup(group);
        }
    }

    public static class RoadNetworkEx
    {
        public static void Replace<T>(IList<T> self, T before, T after) where T : class
        {
            for (var i = 0; i < self.Count; i++)
            {
                if (self[i] == before)
                    self[i] = after;
            }
        }

        public static void ReplaceLane(IList<RoadNetworkLane> self, RoadNetworkLane before, RoadNetworkLane after)
        {
            Replace(self, before, after);
            foreach (var lane in self)
                lane.ReplaceConnection(before, after);
        }

        public static void RemoveLane(IList<RoadNetworkLane> self, RoadNetworkLane lane)
        {
            self.Remove(lane);
            foreach (var l in self)
                l.RemoveConnection(lane);
        }

        /// <summary>
        /// ModelのRootNodeを返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static IEnumerable<Node> GetRootNodes(this Model self)
        {
            for (var i = 0; i < self.RootNodesCount; ++i)
                yield return self.GetRootNodeAt(i);
        }

        [Serializable]
        internal class ConvertCityObjectResult
        {
            public List<ConvertedCityObject> ConvertedCityObjects { get; } = new List<ConvertedCityObject>();
        }


        internal static async Task<ConvertCityObjectResult> ConvertCityObjectsAsync(IEnumerable<PLATEAUCityObjectGroup> cityObjectGroups, float epsilon = 0.1f)
        {
            // NOTE : CityGranularityConverterを参考
            var cityObjectGroupList = cityObjectGroups.ToList();
            var nativeOption = new GranularityConvertOption(MeshGranularity.PerAtomicFeatureObject, 1);
            var transformList = new UniqueParentTransformList(cityObjectGroupList.Select(c => c.transform).ToArray());

            // 属性情報を記憶しておく
            var attributes = GmlIdToSerializedCityObj.ComposeFrom(transformList);

            var unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithGameMaterial();

            // ゲームオブジェクトを共通ライブラリのModelに変換します。
            using var srcModel = UnityMeshToDllModelConverter.Convert(
                transformList,
                unityMeshToDllSubMeshConverter,
                true, // 非表示のゲームオブジェクトも対象に含めます。なぜなら、LOD0とLOD1のうちLOD1だけがActiveになっているという状況で、変換後もToolkitsのLOD機能を使えるようにするためです。
                VertexConverterFactory.NoopConverter());

            // 共通ライブラリの機能でモデルを分割・結合します。
            var converter = new GranularityConverter();
            var dstModel = converter.Convert(srcModel, nativeOption);
            var getter = new SerializedCityObjectGetterFromDict(attributes, dstModel);
            var attrHelper = new AttributeDataHelper(getter, nativeOption.Granularity, true);
            var cco = await Task.Run(() => new ConvertedCityObject(dstModel, attrHelper));

            foreach (var co in cityObjectGroupList)
            {
                var ccoChild = cco.GetAllChildren().FirstOrDefault(c => c.Name == co.name);
                if (ccoChild != null)
                {
                    ccoChild.SetCityObjectGroup(co);
                }
            }

            var ret = new ConvertCityObjectResult();
            ret.ConvertedCityObjects.AddRange(cco.GetAllChildren().Where(c => c.Children.Any() == false && c.Meshes.Any()));

            return ret;
        }
    }
}
