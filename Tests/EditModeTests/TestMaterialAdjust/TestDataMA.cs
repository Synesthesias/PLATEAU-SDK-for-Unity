using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Tests.EditModeTests.TestMaterialAdjust
{
    /// <summary>
    /// マテリアル分けと粒度変換のテストデータです。
    /// MAはMaterialAdjustの略です。
    /// </summary>
    public class TestDataMA
    {
        private static readonly string PathBase =
            PathUtil.SdkPathToAssetPath("Tests/EditModeTests/TestMaterialAdjust/TestData");

        private static readonly string pathBldgSrcAtomic = PathBase + "/BldgAtomic.prefab";
        private static readonly string pathBldgSrcPrimary = PathBase + "/BldgPrimary.prefab";
        private static readonly string pathBldgSrcArea = PathBase + "/BldgArea.prefab";
        private static readonly string pathBldgTypeAtomicFromArea = PathBase + "/BldgAtomicFromArea_MAType.prefab";
        private static readonly string pathBldgTypeAtomicFromNonArea = PathBase + "/BldgAtomic_MAType.prefab";
        private static readonly string pathBldgTypePrimary = PathBase + "/BldgPrimary_MAType.prefab";
        private static readonly string pathBldgTypePrimaryFromArea = PathBase + "/BldgPrimaryFromArea_MAType.prefab";
        private static readonly string pathBldgTypeAreaFromArea = PathBase + "/BldgAreaFromArea_MAType.prefab";
        private static readonly string pathBldgTypeNonAreaFromNonArea = PathBase + "/BldgAreaFromNonArea_MAType.prefab";
        private static readonly string pathMaterial1 = PathBase + "/TestMatGreen.mat";
        private static readonly string pathMaterial2 = PathBase + "/TestMatBlue.mat";
        
        /// <summary>
        /// 粒度とそれに対応するインポートデータの辞書です
        /// </summary>
        private readonly Dictionary<MeshGranularity, GameObject> granSrcDict =
            new Dictionary<MeshGranularity, GameObject>{
                {
                    MeshGranularity.PerAtomicFeatureObject,
                    AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcAtomic)
                },
                {
                    MeshGranularity.PerPrimaryFeatureObject,
                    AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcPrimary)
                },
                {
                    MeshGranularity.PerCityModelArea,
                    AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcArea)
                }
            };

        private readonly Material[] testMaterials = new Material[]
        {
            AssetDatabase.LoadAssetAtPath<Material>(pathMaterial1),
            AssetDatabase.LoadAssetAtPath<Material>(pathMaterial2)
        };
        
        /// <summary>
        /// 粒度とそれに対応する地物でのマテリアル分けのデータの辞書です
        /// </summary>
        private readonly Dictionary<MeshGranularity, GameObject[]> granTypeDict =
            new Dictionary<MeshGranularity, GameObject[]>{
                {
                    MeshGranularity.PerAtomicFeatureObject,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypeAtomicFromNonArea),
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypeAtomicFromArea)
                        
                    }
                },
                {
                    MeshGranularity.PerPrimaryFeatureObject,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypePrimary),
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypePrimaryFromArea)
                    }
                },
                {
                    MeshGranularity.PerCityModelArea,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypeAreaFromArea),
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgTypeNonAreaFromNonArea)
                    }
                }
            };

        /// <summary>
        /// 指定粒度でインポートしたときのデータをコピーして返します
        /// </summary>
        public GameObject CopySrcOf(MAGranularity gran)
        {
            return Copy(granSrcDict[gran.ToNativeGranularity()]);
        }

        /// <summary>
        /// 指定粒度であり、マテリアル分け結果としてexpectされるものをコピーして返します。
        /// </summary>
        public GameObject CopyMATypeDstOf(MAGranularity gran, int dataId)
        {
            return Copy(granTypeDict[gran.ToNativeGranularity()][dataId]);
        }

        public Material Material(int id)
        {
            return testMaterials[id];
        }

        private GameObject Copy(GameObject src)
        {
            var obj = Object.Instantiate(src);
            obj.name = obj.name.Replace("(Clone)", "");
            return obj;
        }
    }
}