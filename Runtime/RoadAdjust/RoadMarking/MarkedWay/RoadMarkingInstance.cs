using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.RoadAdjust.RoadMarking
{
    /// <summary>
    /// 路上表示のメッシュ情報（結合前）を保持します。
    /// 具体的には、UnityのCombineInstanceに路上表示マテリアル種別を追加したものです。
    /// </summary>
    internal class RoadMarkingInstance
    {
        public CombineInstance CombineInstance { get; private set; }
        public RoadMarkingMaterial MaterialType { get; }

        public RoadMarkingInstance(Mesh mesh, RoadMarkingMaterial materialType)
        {
            CombineInstance = new CombineInstance{mesh = mesh, transform = Matrix4x4.identity};
            MaterialType = materialType;
        }

        public void Translate(Vector3 moveVector)
        {
            CombineInstance = new CombineInstance
            {
                mesh = CombineInstance.mesh,
                transform = Matrix4x4.Translate(moveVector) * CombineInstance.transform
            };
        }

        public void RotateYAxis(float angle)
        {
            // 度数法からラジアンに変換
            float rad = angle * Mathf.Deg2Rad;
        
            // Y軸周りの回転行列を作成
            Matrix4x4 rotationMatrix = Matrix4x4.identity;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
        
            rotationMatrix.m00 = cos;
            rotationMatrix.m02 = sin;
            rotationMatrix.m20 = -sin;
            rotationMatrix.m22 = cos;
        
            // 既存の行列に回転を適用
            var transform = rotationMatrix * CombineInstance.transform;
            CombineInstance = new CombineInstance { mesh = CombineInstance.mesh, transform = transform };
        }
    }

    /// <summary>
    /// 路上表示のメッシュを結合します。
    /// </summary>
    internal class RoadMarkingCombiner
    {
        /// <summary> 結合対象 </summary>
        private readonly List<RoadMarkingInstance> instances;

        public RoadMarkingCombiner()
        {
            instances = new List<RoadMarkingInstance>();
        }

        /// <summary> 結合対象を追加します。 </summary>
        public void Add(RoadMarkingInstance instance)
        {
            if (instance == null)
            {
                return;
            }
            instances.Add(instance);
        }
        
        public void AddRange(IEnumerable<RoadMarkingInstance> instances)
        {
            foreach (var instance in instances)
            {
                Add(instance);
            }
        }

        /// <summary> 結合してMeshを生成します。 </summary>
        public Mesh Combine(out Material[] outMaterials)
        {
            // マテリアルごとに結合します。
            var mats = ((RoadMarkingMaterial[])Enum.GetValues(typeof(RoadMarkingMaterial))).Where(m => m != RoadMarkingMaterial.None);
            var matCombined = new SortedDictionary<RoadMarkingMaterial, Mesh>();
            foreach (var mat in mats)
            {
                var combineTargets = instances.Where(i => i.MaterialType == mat).Select(i => i.CombineInstance).ToArray();
                if (combineTargets.Length == 0) continue;
                var combined = new Mesh();
                combined.indexFormat = IndexFormat.UInt32;
                combined.CombineMeshes(combineTargets, true);
                matCombined.Add(mat, combined);
            }
            // マテリアルごとに結合したものをさらに結合して1つにします。
            var allCombineTargets = matCombined.Select(
                    mc => new CombineInstance { mesh = mc.Value, transform = Matrix4x4.identity })
                    .ToArray();
            var allCombinedMesh = new Mesh();
            allCombinedMesh.indexFormat = IndexFormat.UInt32;
            allCombinedMesh.CombineMeshes(allCombineTargets, false);
            outMaterials = matCombined.Keys.Select(m => m.ToMaterial()).ToArray();
            return allCombinedMesh;
        }
    }

    /// <summary>
    /// 路上表示に使うマテリアルです。
    /// </summary>
    internal enum RoadMarkingMaterial
    {
        None, White, Yellow
    }

    internal static class RoadMarkingMaterialExtension
    {
        private const string MaterialFolder = "PlateauRoadMarkingMaterials";
        private const string MaterialNameWhite = "PlateauRoadMarkingWhite";
        private const string MaterialNameYellow = "PlateauRoadMarkingYellow";
        private static readonly Material materialWhite = Resources.Load<Material>(MaterialFolder + "/" + MaterialNameWhite);
        private static readonly Material materialYellow = Resources.Load<Material>(MaterialFolder + "/" + MaterialNameYellow);
        
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
                case RoadMarkingMaterial.None:
                    Debug.LogWarning("material is none.");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(material), material, null);
            }
        }

        public static Material[] Materials()
        {
            var mats = (RoadMarkingMaterial[])Enum.GetValues(typeof(RoadMarkingMaterial));
            return mats
                .Where(m => m != RoadMarkingMaterial.None)
                .Select(m => m.ToMaterial()).ToArray();
        }
    }
}