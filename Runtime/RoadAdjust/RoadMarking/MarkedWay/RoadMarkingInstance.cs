using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 路上表示のメッシュ情報（結合前）を保持します。
    /// 具体的には、UnityのCombineInstanceに路上表示マテリアル種別を追加したものです。
    /// </summary>
    internal class RoadMarkingInstance
    {
        public CombineInstance CombineInstance { get; }
        public RoadMarkingMaterial MaterialType { get; }

        public RoadMarkingInstance(Mesh mesh, RoadMarkingMaterial materialType)
        {
            CombineInstance = new CombineInstance{mesh = mesh, transform = Matrix4x4.identity};
            MaterialType = materialType;
        }
    }

    /// <summary>
    /// 路上表示のメッシュを結合します。
    /// </summary>
    internal class RoadMarkingCombiner
    {
        /// <summary> 結合対象 </summary>
        private readonly List<RoadMarkingInstance> instances;

        public RoadMarkingCombiner(int capacity)
        {
            instances = new List<RoadMarkingInstance>(capacity);
        }

        /// <summary> 結合対象を追加します。 </summary>
        public void Add(RoadMarkingInstance instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("RoadMarkingInstance is null");
                return;
            }
            instances.Add(instance);
        }

        /// <summary> 結合します。 </summary>
        public Mesh Combine()
        {
            // マテリアルごとに結合します。
            var mats = (RoadMarkingMaterial[])Enum.GetValues(typeof(RoadMarkingMaterial));
            var matCombined = new SortedDictionary<RoadMarkingMaterial, Mesh>();
            foreach (var mat in mats)
            {
                var combinees = instances.Where(i => i.MaterialType == mat).Select(i => i.CombineInstance).ToArray();
                var combined = new Mesh();
                combined.CombineMeshes(combinees, true);
                matCombined.Add(mat, combined);
            }
            // マテリアルごとに結合したものをさらに結合して1つにします。
            var allCombinees = matCombined.Select(
                    mc => new CombineInstance { mesh = mc.Value, transform = Matrix4x4.identity })
                    .ToArray();
            var allCombinedMesh = new Mesh();
            allCombinedMesh.CombineMeshes(allCombinees, false);
            return allCombinedMesh;
        }
    }

    /// <summary>
    /// 路上表示に使うマテリアルです。
    /// </summary>
    internal enum RoadMarkingMaterial
    {
        White, Yellow
    }

    internal static class RoadMarkingMaterialExtension
    {
        private static string materialFolder = "PlateauRoadMarkingMaterials";
        private static string materialNameWhite = "PlateauRoadMarkingWhite";
        private static string materialNameYellow = "PlateauRoadMarkingYellow";
        private static Material materialWhite = Resources.Load<Material>(materialFolder + "/" + materialNameWhite);
        private static Material materialYellow = Resources.Load<Material>(materialFolder + "/" + materialNameYellow);
        
        /// <summary>
        /// 路上表示に使うマテリアルenumから実際のマテリアルを取得します。
        /// </summary>
        public static Material ToMaterial(this RoadMarkingMaterial material)
        {
            switch (material)
            {
                case RoadMarkingMaterial.White:
                    return new Material(materialWhite);
                case RoadMarkingMaterial.Yellow:
                    return new Material(materialYellow);
                default:
                    throw new ArgumentOutOfRangeException(nameof(material), material, null);
            }
        }

        public static Material[] Materials()
        {
            var mats = (RoadMarkingMaterial[])Enum.GetValues(typeof(RoadMarkingMaterial));
            return mats.Select(m => m.ToMaterial()).ToArray();
        }
    }
}