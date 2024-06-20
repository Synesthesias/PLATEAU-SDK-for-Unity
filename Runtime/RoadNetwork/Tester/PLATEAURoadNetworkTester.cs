using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
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

        [field: SerializeField] public RoadNetworkModel RoadNetwork { get; set; }

        [Serializable]
        class TesterTranMeshDrawParam : TesterDrawParam
        {
            public bool loop = true;
            public float mergeEpsilon = 0.2f;
        }

        [SerializeField] private TesterTranMeshDrawParam showTranMesh;
        [SerializeField] private TesterDrawParam showConvertedCityObject;

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

        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);
            if (showConvertedCityObject.visible)
            {
                foreach (var item in convertedCityObjects)
                {
                    if (item.Visible == false)
                        continue;
                    foreach (var mesh in item.Meshes)
                    {
                        foreach (var subMesh in mesh.SubMeshes.Select((v, i) => new { v, i }))
                            DrawMesh(mesh, subMesh.v, Matrix4x4.identity, color: DebugEx.GetDebugColor(subMesh.i, mesh.SubMeshes.Count));
                    }
                }
            }

            if (showTranMesh.visible)
            {
                foreach (var t in tranMeshes)
                {
                    if (t.Vertices == null)
                        continue;
                    DebugEx.DrawLines(t.Vertices, showTranMesh.loop, showTranMesh.color);
                }
            }
        }

        public async Task SplitCityObjectAsync()
        {
            var p = splitCityObjectTestParam;
            convertedCityObjects = (await RoadNetworkEx.ConvertCityObjectsAsync(p.targets)).ConvertedCityObjects;
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
            return (await RoadNetworkEx.ConvertCityObjectsAsync(targets)).ConvertedCityObjects;
        }

        private List<RoadNetworkTranMesh> CreateTranMeshes(float epsilon)
        {
            var ret = new List<RoadNetworkTranMesh>();
            foreach (var c in convertedCityObjects)
            {
                var root = c.CityObjects.rootCityObjects[0];
                bool IsTarget()
                {
                    // LOD1
                    if (root.AttributesMap.TryGetValue("tran:class", out var tranClass))
                    {
                        if (tranClass.StringValue == "道路")
                            return true;
                    }

                    if (root.AttributesMap.TryGetValue("tran:function", out var tranFunction))
                    {
                        if (new string[] { "車道部", "車道交差部" }.Contains(tranFunction.StringValue))
                            return true;
                    }

                    return false;
                }

                if (IsTarget() == false)
                    continue;
                foreach (var mesh in c.Meshes)
                {
                    foreach (var s in mesh.SubMeshes)
                    {
                        var vertices = GeoGraph2D.ComputeMeshOutlineVertices(mesh, s, a => a.Xz(), epsilon);
                        ret.Add(new RoadNetworkTranMesh(null, vertices));

                    }
                }
            }

            return ret;
        }

        public async Task CreateNetwork()
        {
            switch (createMode)
            {
                case CreateMode.ConvertCityObject:
                    convertedCityObjects = await ConvertCityObjectAsync();
                    break;
                case CreateMode.MergeConvertCityObject:
                    foreach (var m in convertedCityObjects.SelectMany(c => c.Meshes))
                    {
                        m.Merge(showTranMesh.mergeEpsilon);
                    }
                    break;
                case CreateMode.SeparateConvertCityObject:
                    foreach (var m in convertedCityObjects.SelectMany(c => c.Meshes))
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