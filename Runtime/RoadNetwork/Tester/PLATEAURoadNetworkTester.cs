using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;

namespace PLATEAU.RoadNetwork
{
    [Serializable]
    public class PLATEAURoadNetworkTester : MonoBehaviour, IRoadNetworkObject
    {
        [SerializeField] private RoadNetworkDrawerDebug drawer = new RoadNetworkDrawerDebug();

        [field: SerializeField] private RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        [field: SerializeField] public RnModel RoadNetwork { get; set; }

        [Serializable]
        private class TesterTranMeshDrawParam : TesterDrawParam
        {
            public bool loop = true;
            public float mergeEpsilon = 0.2f;
            public int mergeCellLength = 2;
        }

        [SerializeField] private TesterTranMeshDrawParam showTranMesh;

        [Serializable]
        private class TesterConvertedCityObjectDrawParam : TesterDrawParam
        {
            public int meshColorNum = 16;
            public bool showVertexIndex = false;
            public int showVertexIndexFontSize = 8;
        }

        [SerializeField] private TesterConvertedCityObjectDrawParam showConvertedCityObject;

        [SerializeField] private TesterConvertedCityObjectDrawParam showMergedConvertedCityObject;

        [Serializable]
        public class SplitCityObjectTestParam
        {
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
            public bool doDestroySrcObject = false;
        }
        public SplitCityObjectTestParam splitCityObjectTestParam = new SplitCityObjectTestParam();

        [SerializeField]
        internal List<ConvertedCityObject> convertedCityObjects;

        [SerializeField]
        internal List<ConvertedCityObject> mergedConvertedCityObjects;

        [SerializeField]
        private List<RoadNetworkTranMesh> tranMeshes;

        [Serializable]
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();
        [SerializeField] private bool targetAll = false;
        public string targetPresetName = "";

        [Serializable]
        public enum CreateMode
        {
            ConvertCityObject,
            MergeConvertCityObject,
            SeparateConvertCityObject,
            TranMesh,
            RoadNetwork
        }
        [SerializeField]
        private CreateMode createMode = CreateMode.ConvertCityObject;


        internal void DrawMesh(ConvertedCityObject.ConvertedMesh mesh, ConvertedCityObject.SubMesh subMesh, Matrix4x4 mat, Color? color = null,
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

        private void DrawConvertedCityObject(TesterConvertedCityObjectDrawParam p, List<ConvertedCityObject> cityObjects)
        {
            if (!p.visible)
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
                    if (p.showVertexIndex)
                    {
                        //foreach (var v in mesh.Vertices)
                        for (var i = 0; i < mesh.Vertices.Count; ++i)
                        {
                            var v = mesh.Vertices[i];
                            DebugEx.DrawString($"{i}", v.PutY(v.y + i * 0.01f), fontSize: p.showVertexIndexFontSize);
                        }
                    }

                    foreach (var subMesh in mesh.SubMeshes)
                    {
                        DrawMesh(mesh, subMesh, Matrix4x4.identity, color: DebugEx.GetDebugColor(index, p.meshColorNum));
                        index++;
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);

            DrawConvertedCityObject(showConvertedCityObject, convertedCityObjects);
            DrawConvertedCityObject(showMergedConvertedCityObject, mergedConvertedCityObjects);

            if (showTranMesh.visible)
            {
                foreach (var t in tranMeshes)
                {
                    if (t.Vertices == null)
                        continue;
                    if (t.visible == false)
                        continue;
                    DebugEx.DrawLines(t.Vertices, showTranMesh.loop, showTranMesh.color);
                }
            }
        }

        public async Task SplitCityObjectAsync()
        {
            var p = splitCityObjectTestParam;
            convertedCityObjects = (await RnEx.ConvertCityObjectsAsync(p.targets)).ConvertedCityObjects;
        }

        private List<PLATEAUCityObjectGroup> GetTargetCityObjects()
        {
            var ret = targetAll
                ? (IList<PLATEAUCityObjectGroup>)GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                : savedTargets
                    .FirstOrDefault(s => s.name == targetPresetName)
                    ?.targets;
            if (ret == null)
                return new List<PLATEAUCityObjectGroup>();

            return ret
                .Where(c => c.transform.childCount == 0)
                .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                .Distinct()
                .ToList();
        }

        private async Task<List<ConvertedCityObject>> ConvertCityObjectAsync()
        {
            var targets = GetTargetCityObjects();
            return (await RnEx.ConvertCityObjectsAsync(targets)).ConvertedCityObjects;
        }

        private List<RoadNetworkTranMesh> CreateTranMeshes(float epsilon)
        {
            var ret = new List<RoadNetworkTranMesh>();
            foreach (var c in mergedConvertedCityObjects)
            {
                var root = c.CityObjects.rootCityObjects[0];
                bool IsTarget(out int lodLevel)
                {
                    lodLevel = 0;
                    // LOD1
                    if (root.AttributesMap.TryGetValue("tran:class", out var tranClass))
                    {
                        if (tranClass.StringValue == "道路")
                        {
                            lodLevel = 1;
                            return true;
                        }
                    }

                    if (root.AttributesMap.TryGetValue("tran:function", out var tranFunction))
                    {
                        if (new string[] { "車道部", "車道交差部" }.Contains(tranFunction.StringValue))
                        {
                            lodLevel = 3;
                            return true;
                        }
                    }

                    return false;
                }

                var isRoad = IsTarget(out var lodLevel);

                foreach (var mesh in c.Meshes)
                {
                    foreach (var s in mesh.SubMeshes)
                    {
                        var vertices = GeoGraph2D.ComputeMeshOutlineVertices(mesh, s, a => a.Xz(), epsilon);
                        ret.Add(new RoadNetworkTranMesh(c.CityObjectGroup, isRoad, lodLevel, vertices));
                    }
                }
            }

            return ret;
        }

        private void MergeVertices()
        {
            try
            {
                mergedConvertedCityObjects = convertedCityObjects.Select(c => c.DeepCopy()).ToList();
                var vertexTable = GeoGraphEx.MergeVertices(
                    mergedConvertedCityObjects.SelectMany(c => c.Meshes.SelectMany(m => m.Vertices)),
                    showTranMesh.mergeEpsilon, showTranMesh.mergeCellLength);
                foreach (var m in mergedConvertedCityObjects.SelectMany(c => c.Meshes))
                {
                    m.Merge(vertexTable);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task CreateNetwork()
        {
            switch (createMode)
            {
                case CreateMode.ConvertCityObject:
                    convertedCityObjects = await ConvertCityObjectAsync();
                    break;
                case CreateMode.MergeConvertCityObject:
                    MergeVertices();
                    break;
                case CreateMode.SeparateConvertCityObject:
                    foreach (var m in mergedConvertedCityObjects.SelectMany(c => c.Meshes))
                    {
                        m.Separate();
                    }
                    break;
                case CreateMode.TranMesh:
                    tranMeshes = CreateTranMeshes(Factory.cellSize);
                    break;
                case CreateMode.RoadNetwork:
                    RoadNetwork = await Factory.CreateNetworkAsync(tranMeshes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}