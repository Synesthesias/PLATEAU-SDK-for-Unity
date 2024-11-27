using PLATEAU.CityInfo;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュに変換します。
    /// </summary>
    public class RoadNetworkToMesh
    {
        private readonly RnModel srcModel;
        private readonly RnmLineSeparateType lineSeparateType;
        private static readonly bool DebugMode = false;
        
        public RoadNetworkToMesh(RnModel model, RnmLineSeparateType lineSeparateType)
        {
            if (model == null)
            {
                Debug.LogError("道路ネットワークがありません。");
            }
            this.srcModel = model;
            this.lineSeparateType = lineSeparateType;
        }

        public void Generate()
        {
            
            using var progressDisplay = new ProgressDisplayDialogue();
            
            progressDisplay.SetProgress("道路ネットワークを調整中", 10f, "");
            var model = new RnmModelAdjuster().Adjust(srcModel);
            
            progressDisplay.SetProgress("道路ネットワークをスムージング中", 20f, "");
            new RoadNetworkLineSmoother().Smooth(model);
            
            progressDisplay.SetProgress("道路ネットワークから輪郭線を生成中", 30f, "");
            // 生成すべき輪郭線を定義します。
            IRnmContourGenerator[] contourGenerators;
            switch (lineSeparateType)
            {
                case RnmLineSeparateType.Combine:
                    contourGenerators = new IRnmContourGenerator[]
                    {
                        new RnmContourGeneratorRoadCombine(), // 道路
                        new RnmContourGeneratorIntersectionCombine() // 交差点(結合)
                    };
                    break;
                case RnmLineSeparateType.Separate:
                    contourGenerators = new IRnmContourGenerator[]
                    {
                        new RnmContourGeneratorCarLane(), // 車道
                        new RnmContourGeneratorSidewalk(), // 歩道
                        new RnmContourGeneratorIntersectionSeparate() // 交差点(分割)
                    };
                    break;
                default:
                    throw new ArgumentException($"Unknown {nameof(RnmLineSeparateType)}");
            }
            var contourMeshList = new RnmContourGenerator(contourGenerators).Generate(model);
            
            // 輪郭線からメッシュとゲームオブジェクトを生成します。
            progressDisplay.SetProgress("輪郭線からゲームオブジェクトを生成中...", 0f, "");

            for (int i = 0; i < contourMeshList.Count; i++)
            {
                progressDisplay.SetProgress("輪郭線からゲームオブジェクトを生成中", (float)i * 100f / contourMeshList.Count, $"{i} / {contourMeshList.Count}");
                var contourMesh = contourMeshList[i];
                var srcObjs = contourMesh.SourceObjects;
                
                var mesh = new ContourToMesh().Generate(contourMesh, out var subMeshIDToMatType);
                if (mesh.vertexCount == 0) continue;
                
                string dstObjName = srcObjs.Length == 0 ? "RoadUnknown" : srcObjs[0].name;
                var dstObj = new GameObject(dstObjName);

                if (DebugMode)
                {
                    var comp = dstObj.AddComponent<PLATEAURoadNetworkToMeshDebug>();
                    comp.Init(contourMesh);
                }
                
                
                var renderer = dstObj.AddComponent<MeshRenderer>();
                var filter = dstObj.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                
                // マテリアルを貼り付けます。
                var dstMats = new Material[mesh.subMeshCount];
                foreach(var (subMeshID, matType) in subMeshIDToMatType)
                {
                    
                    if (subMeshID >= dstMats.Length)
                    {
                        Debug.LogError($"subMeshID is out of range. subMeshID: {subMeshID}, dstMats.Length: {dstMats.Length}, verts count: {mesh.vertexCount}");
                    }
                    else
                    {
                        dstMats[subMeshID] = matType.ToMaterial();
                    }
                    
                }
                renderer.sharedMaterials = dstMats;
                
                if (srcObjs.Length > 0)
                {
                    // UV4をコピーします。
                    new RnmUV4Copier().Copy(srcObjs, dstObj);

                    // 属性情報をコピーします。
                    // FIXME: srcObjsが複数のケースに未対応
                    var srcAttr = srcObjs[0].GetComponent<PLATEAUCityObjectGroup>();
                    if (srcAttr != null)
                    {
                        var dstAttr = dstObj.AddComponent<PLATEAUCityObjectGroup>();
                        dstAttr.CopyFrom(srcAttr);
                        dstObj.AddComponent<MeshCollider>();
                    }
                    
                    // 他のコンポーネントをコピーします。
                    // FIXME: srcObjsが複数のケースに未対応
                    new ComponentCopier(
                        ignoreTypes: new[]
                        { // 上述で作るコンポーネントは除外します。
                            typeof(MeshFilter), typeof(MeshRenderer), typeof(PLATEAUCityObjectGroup), typeof(MeshCollider)
                        })
                        .Copy(srcObjs[0], dstObj);
                    
                }
            }
            
        }
        
    }
    
    internal class ComponentCopier
    {
        private readonly HashSet<Type> ignoreTypes = new();
        private static readonly HashSet<Type> defaultIgnoreTypes = new() { typeof(Transform) };

        public ComponentCopier()
        {
            
        }

        public ComponentCopier(IEnumerable<Type> ignoreTypes)
        {
            foreach (var t in defaultIgnoreTypes.Concat(ignoreTypes))
            {
                this.ignoreTypes.Add(t);
            }
        }
        
        public void Copy(GameObject srcObj, GameObject dstObj)
        {
            foreach (var comp in srcObj.GetComponents<Component>())
            {
                CopyComponent(comp, dstObj);
            }
        }

        /// <summary>
        /// コンポーネントをコピーします。
        /// </summary>
        public void CopyComponent(Component srcComp, GameObject dstObj)
        {
            Type type = srcComp.GetType();
            if (ignoreTypes.Any(t => t.IsAssignableFrom(type)))
            {
                return;
            }
            Component dstComp = dstObj.GetComponent(type);
            try
            {
                if (dstComp == null)
                {
                    dstComp = dstObj.AddComponent(type);
                }
            }
            catch (Exception)
            {
                Debug.LogWarning($"Could not copy component {type.Name} to {dstObj.name}. Skipping.");
                return;
            }

            // フィールドをコピーします。
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                try
                {
                    field.SetValue(dstComp, field.GetValue(srcComp));
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Failed to write field {field.Name} of {type.Name}. Skipping.");
                }
            }

            // プロパティをコピーします。
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    try
                    {
                        property.SetValue(dstComp, property.GetValue(srcComp));
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Failed to write property {property.Name} of {type.Name}. Skipping.");
                    }
                }
            }
        }
    }
}