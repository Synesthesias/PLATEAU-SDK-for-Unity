using JetBrains.Annotations;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.CityObject.Drawer;
using PLATEAU.RoadNetwork.Drawer;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Graph.Drawer;
using PLATEAU.RoadNetwork.Mesh;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

namespace PLATEAU.RoadNetwork.Factory
{
    /// <summary>
    /// 道路構造生成のための中間データ
    /// </summary>
    [Serializable]
    public class RsFactoryMidStageData
    {
        [Serializable]
        public class SubDividedCityObjectWrap
        {
            public SubDividedCityObjectDrawerDebug drawer = new SubDividedCityObjectDrawerDebug();
            public List<SubDividedCityObject> cityObjects = new List<SubDividedCityObject>();
        }

        [Serializable]
        public class TranMeshWrap : TesterDrawParam
        {
            public bool loop = true;
            public float mergeEpsilon = 0.2f;
            public int mergeCellLength = 2;
            public RRoadTypeMask showRoadTypeMask = RRoadTypeMask.Empty;
            public List<RoadNetworkTranMesh> tranMeshes = new List<RoadNetworkTranMesh>();
        }

        // --------------------
        // start:フィールド
        // --------------------
        [SerializeField] public bool saveTmpData = false;


        //[SerializeField] public RGraphWrap rGraph = new RGraphWrap();

        [SerializeField] public SubDividedCityObjectWrap convertedCityObjects = new SubDividedCityObjectWrap();

        [SerializeField] public SubDividedCityObjectWrap mergedConvertedCityObjects = new SubDividedCityObjectWrap();

        [SerializeField] public TranMeshWrap tranMesh = new TranMeshWrap();


        // --------------------
        // end:フィールド
        // --------------------

        /// <summary>
        /// PLATEAUCityObjectGroupを変換する
        /// </summary>
        /// <returns></returns>
        public async Task<List<SubDividedCityObject>> ConvertCityObjectAsync(List<PLATEAUCityObjectGroup> targets)
        {
            convertedCityObjects.cityObjects = (await RnEx.ConvertCityObjectsAsync(targets)).ConvertedCityObjects;
            return convertedCityObjects.cityObjects;
        }

        public List<SubDividedCityObject> CreateVertexMergedConvertCityObjectAsync(float cellSize,
            int cellLength)
        {
            if (convertedCityObjects.cityObjects == null)
                return convertedCityObjects.cityObjects;
            var ret = RnEx.MergeVertices(convertedCityObjects.cityObjects, cellSize, cellLength);
            if (saveTmpData == false)
                convertedCityObjects.cityObjects = new List<SubDividedCityObject>();
            foreach (var m in ret.SelectMany(c => c.Meshes))
                m.Separate();
            mergedConvertedCityObjects.cityObjects = ret;

            return ret;
        }

        /// <summary>
        /// ConvertedCityObjectからTranMeshを生成する
        /// </summary>
        /// <returns></returns>
        public List<RoadNetworkTranMesh> CreateTranMeshes(float cellSize)
        {
            var ret = new List<RoadNetworkTranMesh>();
            foreach (var c in mergedConvertedCityObjects.cityObjects)
            {
                var lodLevel = c.CityObjectGroup.GetLodLevel();
                var roadType = c.GetRoadType(false);
                foreach (var mesh in c.Meshes)
                {
                    foreach (var s in mesh.SubMeshes)
                    {
                        var vertices = GeoGraph2D.ComputeMeshOutlineVertices(mesh, s, a => a.Xz(), cellSize);
                        ret.Add(new RoadNetworkTranMesh(c.CityObjectGroup, roadType, lodLevel, vertices));
                    }
                }
            }

            if (saveTmpData == false)
            {
                mergedConvertedCityObjects.cityObjects = new List<SubDividedCityObject>();
            }

            tranMesh.tranMeshes = ret;
            return ret;
        }

        public RGraph CreateGraph(RGraphFactory graphFactory, GameObject target = null)
        {
            var graph = graphFactory.CreateGraph(convertedCityObjects.cityObjects);
            if (target)
            {
                if (saveTmpData)
                {
                    target.GetOrAddComponent<PLATEAURGraph>().Graph = graph;
                }
                else
                {
                    foreach (var comp in target.GetComponents<PLATEAURGraph>().ToList())
                    {
                        UnityEngine.Object.DestroyImmediate(comp);
                    }
                }

            }
            return graph;
        }

        public async Task CreateAll(List<PLATEAUCityObjectGroup> targets)
        {
            await ConvertCityObjectAsync(targets);
            CreateVertexMergedConvertCityObjectAsync(0.1f, 10);
            CreateTranMeshes(0.1f);
        }

        private void DrawTarnMesh()
        {
            if (tranMesh.visible == false)
                return;
            foreach (var t in tranMesh.tranMeshes)
            {
                if (t.Vertices == null)
                    continue;
                if (t.visible == false)
                    continue;

                if (t.RoadType.HasAnyFlag(tranMesh.showRoadTypeMask) == false)
                    continue;
                DebugEx.DrawLines(t.Vertices, tranMesh.loop, tranMesh.color);
            }
        }

        public void DebugDraw()
        {
            convertedCityObjects.drawer.DrawConvertedCityObject(convertedCityObjects.cityObjects);
            mergedConvertedCityObjects.drawer.DrawConvertedCityObject(mergedConvertedCityObjects.cityObjects);
            DrawTarnMesh();
        }
    }
}