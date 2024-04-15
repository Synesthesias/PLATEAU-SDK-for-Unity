using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    /// <summary>
    /// 地物型を基準にマテリアル分けのキーを検索します。
    /// </summary>
    internal class TypeSearcher : ISearcher<CityObjectTypeHierarchy.Node>
    {
        private SearchArg searchArg;

        public TypeSearcher(SearchArg searchArg)
        {
            this.searchArg = searchArg;
        }
        
        /// <summary>
        /// 引数とその子に含まれるCityObjectTypeを列挙します。
        /// </summary>
        public CityObjectTypeHierarchy.Node[] Search()
        {
            HashSet<CityObjectType> found = new();
            foreach(var target in searchArg.TargetObjs.Select(obj => obj.transform))
            {
                var cityObjGroups = target.GetComponentsInChildren<PLATEAUCityObjectGroup>();
                foreach (var cityObjGroup in cityObjGroups)
                {
                    var meshFilter = cityObjGroup.GetComponent<MeshFilter>();
                    if (meshFilter == null) continue;
                    var mesh = meshFilter.sharedMesh;
                    if (mesh == null || mesh.vertices.Length <= 0) continue;

                    var indicesInMesh = SearchIndicesInMesh(mesh);
                    
                    var cityObjs = cityObjGroup.GetAllCityObjects();
                    foreach (var cityObj in cityObjs)
                    {
                        // CityObjectIndexのうち、mesh中に実際には存在しないものを除きます。
                        var coi = cityObj.IndexInMesh;
                        if (!indicesInMesh.Contains(coi)) continue;
                        // 追加します
                        found.Add(cityObj.CityObjectType);
                    }
                }
                
            }
            
            return found.Select((t) =>
            {
                var typeNode = CityObjectTypeHierarchy.GetNodeByType(t);
                if (typeNode == null) throw new ArgumentOutOfRangeException(nameof(t), $"Unknown Type: {t.ToString()}");
                return typeNode;
            }).ToArray();
        }

        HashSet<CityObjectIndex> SearchIndicesInMesh(UnityEngine.Mesh mesh)
        {
            HashSet<CityObjectIndex> indicesInMesh = new();
            var uv4 = mesh.uv4;
            int len = uv4.Length;
            for (int i = 0; i < len; i++)
            {
                var vec = uv4[i];
                indicesInMesh.Add(new CityObjectIndex(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y)));
                
            }

            return indicesInMesh;
        }
    }
}