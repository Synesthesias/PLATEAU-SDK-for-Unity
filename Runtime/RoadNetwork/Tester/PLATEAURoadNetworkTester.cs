using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Factory;
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
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();

        [SerializeField] private bool targetAll = false;

        [Serializable]
        public class SplitCityObjectTestParam
        {
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
            public bool doDestroySrcObject = false;
        }
        public SplitCityObjectTestParam splitCityObjectTestParam = new SplitCityObjectTestParam();

        public string targetPresetName = "";

        [SerializeField]
        internal RoadNetworkEx.ConvertCityObjectResult model;


        public void DrawPlateauMesh(PolygonMesh.Mesh mesh, Matrix4x4 mat, Color? color = null,
            float duration = 0f, bool depthTest = true)
        {

            var keys = mesh.CityObjectList.GetAllKeys();
            for (var i = 0; i < mesh.CityObjectList.Length; ++i)
            {
                var primaryId = mesh.CityObjectList.GetPrimaryID(i);

            }

            foreach (var index in keys)
            {
                var atomicId = mesh.CityObjectList.GetAtomicID(index);
            }

            for (var i = 0; i < mesh.SubMeshCount; ++i)
            {
                var subMesh = mesh.GetSubMeshAt(i);
                for (var j = subMesh.StartIndex; j <= subMesh.EndIndex; j += 3)
                {
                    var v0 = mesh.GetVertexAt(mesh.GetIndiceAt(j));
                    var v1 = mesh.GetVertexAt(mesh.GetIndiceAt(j + 1));
                    var v2 = mesh.GetVertexAt(mesh.GetIndiceAt(j + 2));

                    var v = new[]
                    {
                        mat * v0.ToUnityVector(),
                        mat * v1.ToUnityVector(),
                        mat * v2.ToUnityVector()
                    }.Select(x => new Vector3(x.x, x.y, x.z));
                    DebugEx.DrawLines(v, true, color, duration, depthTest);
                }
            }

        }

        public void DrawPlateauPolygonMeshNode(Matrix4x4 parentMatrix, PolygonMesh.Node node, Color? color = null,
            float duration = 0f, bool depthTest = true)
        {
            var pos = node.LocalPosition.ToUnityVector();
            var rot = node.LocalRotation.ToUnityQuaternion();
            var scale = node.LocalScale.ToUnityVector();
            var mat = parentMatrix * Matrix4x4.TRS(pos, rot, scale);
            DrawPlateauMesh(node.Mesh, mat, color);
            for (var i = 0; i < node.ChildCount; ++i)
            {
                DrawPlateauPolygonMeshNode(mat, node.GetChildAt(i), color, duration, depthTest);
            }
        }

        public void DrawPlateauPolygonMeshModel(PLATEAU.PolygonMesh.Model model, Color? color = null, float duration = 0f, bool depthTest = true)
        {
            for (var i = 0; i < model.RootNodesCount; ++i)
            {
                var node = model.GetRootNodeAt(i);
                DrawPlateauPolygonMeshNode(Matrix4x4.identity, node, color);
            }
        }
        public void OnDrawGizmos()
        {
            drawer.Draw(RoadNetwork);
            if (model != null && model.Model != null)
                DrawPlateauPolygonMeshModel(model.Model);
        }

        public async Task SplitCityObjectAsync()
        {
            var p = splitCityObjectTestParam;
            // 分割結合の設定です。
            // https://project-plateau.github.io/PLATEAU-SDK-for-Unity/manual/runtimeAPI.html
            var conf = new GranularityConvertOptionUnity(new GranularityConvertOption(MeshGranularity.PerAtomicFeatureObject, 1),
                p.targets.Select(t => t.gameObject).ToArray(), p.doDestroySrcObject);
            model = await RoadNetworkEx.ConvertCityObjectsAsync(conf);
            //var d = await new CityGranularityConverter().ConvertAsync(conf);
        }

        public async Task CreateNetwork()
        {
            if (targetAll)
            {
                var allTargets = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                    .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                    .ToList();

                RoadNetwork = await Factory.CreateNetworkAsync(allTargets);
            }
            else
            {
                // 重複は排除する
                var targets = savedTargets.FirstOrDefault(s => s.name == targetPresetName);
                if (targets != null)
                {
                    targets.targets = targets.targets.Distinct().ToList();
                    RoadNetwork = await Factory.CreateNetworkAsync(targets.targets);
                }
            }
        }

    }
}