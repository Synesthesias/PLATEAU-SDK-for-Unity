using PLATEAU.CityInfo;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.RoadAdjust.RoadNetworkToMesh
{
    /// <summary>
    /// 道路ネットワークをメッシュに変換します。
    /// </summary>
    internal class RoadNetworkToMesh
    {
        private readonly IRrTarget srcTargetBeforeCopy;
        private readonly RnmLineSeparateType lineSeparateType;
        
        /// <summary> デバッグモードがONなら、輪郭線形状を矢印で表示します </summary>
        private static readonly bool DebugMode = false;
        
        
        public RoadNetworkToMesh(IRrTarget target, RnmLineSeparateType lineSeparateType)
        {
            if (target == null)
            {
                Debug.LogError("道路ネットワークがありません。");
            }
            this.srcTargetBeforeCopy = target;
            this.lineSeparateType = lineSeparateType;
        }

        /// <summary>
        /// 道路ネットワークからメッシュを生成します。
        /// 生成されたもののリストを返します。
        /// </summary>
        public List<PLATEAUReproducedRoad> Generate(ISmoothingStrategy smoothingStrategy)
        {
            
            using var progressDisplay = new ProgressDisplayDialogue();
            progressDisplay.SetProgress("道路ネットワークをコピー中", 5f, "");
            var targetBeforeAdjust = srcTargetBeforeCopy.Copy();
            
            progressDisplay.SetProgress("道路ネットワークを調整中", 10f, "");
            var target = new RnmModelAdjuster().Adjust(targetBeforeAdjust);
            
            progressDisplay.SetProgress("道路ネットワークをスムージング中", 20f, "");
            new RoadNetworkLineSmoother().Smooth(target, smoothingStrategy);
            
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
            var contourMeshList = new RnmContourGenerator(contourGenerators).Generate(target);
            if (contourMeshList.Count == 0)
            {
                Dialogue.Display("生成対象がありませんでした。", "OK");
                return null;
            }
            
            // 輪郭線からメッシュとゲームオブジェクトを生成します。
            progressDisplay.SetProgress("輪郭線からゲームオブジェクトを生成中...", 0f, "");

            var dstParent = RoadReproducer.GenerateDstParent();
            var resultList = new List<PLATEAUReproducedRoad>();

            for (int i = 0; i < contourMeshList.Count; i++)
            {
                progressDisplay.SetProgress("輪郭線からゲームオブジェクトを生成中", (float)i * 100f / contourMeshList.Count, $"{i} / {contourMeshList.Count}");
                var contourMesh = contourMeshList[i];
                var srcObjs = contourMesh.SourceObjects;
                
                // 輪郭線をテッセレートしてメッシュ化
                var mesh = new ContourToMesh().Generate(contourMesh, out var subMeshIDToMatType);
                if (mesh.vertexCount == 0) continue;

                // オブジェクトの生成
                var dstObj = GenerateDstGameObj(dstParent, srcObjs);
                resultList.Add(dstObj.GetComponent<PLATEAUReproducedRoad>());

                if (DebugMode)
                {
                    var comp = dstObj.GetOrAddComponent<PLATEAURoadNetworkToMeshDebug>();
                    comp.Init(contourMesh);
                }
                
                var renderer = dstObj.GetOrAddComponent<MeshRenderer>();
                var filter = dstObj.GetOrAddComponent<MeshFilter>();
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
                    var srcAttr = srcObjs[0].Transform == null ? null : srcObjs[0].Transform.GetComponent<PLATEAUCityObjectGroup>();
                    if (srcAttr != null)
                    {
                        var dstAttr = dstObj.GetOrAddComponent<PLATEAUCityObjectGroup>();
                        dstAttr.CopyFrom(srcAttr);
                        dstObj.GetOrAddComponent<MeshCollider>();
                    } else
                    {
                        dstObj.GetOrAddComponent<PLATEAUCityObjectGroup>();
                        dstObj.GetOrAddComponent<MeshCollider>();
                    }

                    // 他のコンポーネントをコピーします。
                    // FIXME: srcObjsが複数のケースに未対応
                    if (srcObjs[0].Transform != null)
                    {
                        var componentCopier =
                            new ComponentCopier(
                                ignoreTypes: new[]
                                {
                                    // 上述で作るコンポーネントは除外します。
                                    typeof(MeshFilter), typeof(MeshRenderer), typeof(PLATEAUCityObjectGroup),
                                    typeof(MeshCollider)
                                });
                        componentCopier.Copy(srcObjs[0].Transform.gameObject, dstObj);
                    }
                    
                }
            }
            
#if UNITY_EDITOR
            // 生成物を選択状態に
            // Selection.objects = new []{dstParent.gameObject};
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif

            return resultList;
        }

        /// <summary>
        /// シーン中にゲームオブジェクトとして配置します。
        /// 同じものがシーンにあれば置き換え、なければ生成します。
        /// </summary>
        private GameObject GenerateDstGameObj(Transform dstParent, RoadReproduceSource[] srcObjs)
        {
            var srcObj = srcObjs.Length == 0 ? null : srcObjs[0];
            string srcObjName = srcObj == null? "UnknownRoad" : srcObj.GetName();
            string dstObjName = $"{ReproducedRoadType.RoadMesh.ToGameObjName()}-{srcObjName}";

            GameObject dstObj = null;
            if (srcObj != null)
            {
                dstObj = PLATEAUReproducedRoad.Find(ReproducedRoadType.RoadMesh, srcObj, ReproducedRoadDirection.None);
            }

            if (dstObj == null)
            {
                dstObj = new GameObject(dstObjName);
            }
            dstObj.transform.SetParent(dstParent);
            var comp = dstObj.GetOrAddComponent<PLATEAUReproducedRoad>();
            comp.Init(ReproducedRoadType.RoadMesh, srcObj == null ? null : srcObj, ReproducedRoadDirection.None);
            return dstObj;
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