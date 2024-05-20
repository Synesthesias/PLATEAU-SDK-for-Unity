using PLATEAU.CityAdjust.MaterialAdjust;
using PLATEAU.CityAdjust.MaterialAdjust.Executor.Process;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Material = UnityEngine.Material;
using Object = UnityEngine.Object;

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
        private static readonly string pathTranSrcAtomic = PathBase + "/TranAtomic.prefab";
        private static readonly string pathTranSrcPrimary = PathBase + "/TranPrimary.prefab";
        private static readonly string pathTranAttrAtomic = PathBase + "/TranAtomic_MAAttr.prefab";
        private static readonly string pathTranAttrPrimary = PathBase + "/TranPrimary_MAAttr.prefab";
        private static readonly string pathTranAttrArea = PathBase + "/TranArea_MAAttr.prefab";
        private static readonly string pathTranAttrMaterialInPrimary =
            PathBase + "/TranMaterialInPrimary_MAAttr.prefab";
        private static readonly string pathTranSrcArea = PathBase + "/TranArea.prefab";
        private static readonly string pathMaterial1 = PathBase + "/TestMatGreen.mat";
        private static readonly string pathMaterial2 = PathBase + "/TestMatBlue.mat";

        /// <summary>
        /// 粒度とそれに対応するインポートデータの辞書です（建物）
        /// </summary>
        private readonly Dictionary<MeshGranularity, GameObject> granSrcDictBldg =
            new Dictionary<MeshGranularity, GameObject>
            {
                {
                    MeshGranularity.PerAtomicFeatureObject, AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcAtomic)
                },
                {
                    MeshGranularity.PerPrimaryFeatureObject,
                    AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcPrimary)
                },
                { MeshGranularity.PerCityModelArea, AssetDatabase.LoadAssetAtPath<GameObject>(pathBldgSrcArea) }
            };

        /// <summary>
        /// 粒度とそれに対応するインポートデータの辞書です（道路）
        /// </summary>
        private readonly Dictionary<MeshGranularity, GameObject> granSrcDictTran =
            new Dictionary<MeshGranularity, GameObject>()
            {
                {
                    MeshGranularity.PerAtomicFeatureObject, AssetDatabase.LoadAssetAtPath<GameObject>(pathTranSrcAtomic)
                },
                {
                    MeshGranularity.PerPrimaryFeatureObject, AssetDatabase.LoadAssetAtPath<GameObject>(pathTranSrcPrimary)
                },
                {
                    MeshGranularity.PerCityModelArea, AssetDatabase.LoadAssetAtPath<GameObject>(pathTranSrcArea)
                }
            };

        private readonly Material[] testMaterials = new Material[]
        {
            AssetDatabase.LoadAssetAtPath<Material>(pathMaterial1),
            AssetDatabase.LoadAssetAtPath<Material>(pathMaterial2)
        };
        
        /// <summary>
        /// 粒度とそれに対応する地物のマテリアル分けのデータの辞書です
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
        /// 粒度とそれに対応する属性情報でのマテリアル分けのデータの辞書です
        /// </summary>
        private readonly Dictionary<MAGranularity, GameObject[]> granAttrDict =
            new Dictionary<MAGranularity, GameObject[]>{
                {
                    MAGranularity.PerAtomicFeatureObject,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathTranAttrAtomic),
                        
                    }
                },
                {
                    MAGranularity.PerMaterialInPrimary,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathTranAttrMaterialInPrimary)
                    }
                },
                {
                    MAGranularity.PerPrimaryFeatureObject,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathTranAttrPrimary),
                    }
                },
                {
                    MAGranularity.CombineAll,
                    new[]
                    {
                        AssetDatabase.LoadAssetAtPath<GameObject>(pathTranAttrArea),
                    }
                }
            };

        /// <summary>
        /// 指定粒度でインポートしたときのデータをコピーして返します（建物）
        /// </summary>
        public GameObject CopyBldgSrcOf(MAGranularity gran)
        {
            return Copy(granSrcDictBldg[gran.ToNativeGranularity()]);
        }
        
        /// <summary>
        /// 指定粒度でインポートしたときのデータをコピーして返します（道路）
        /// </summary>
        public GameObject CopyTranSrcOf(MAGranularity gran)
        {
            return Copy(granSrcDictTran[gran.ToNativeGranularity()]);
        }
        

        /// <summary>
        /// 指定粒度であり、地物でのマテリアル分け結果としてexpectされるものをコピーして返します。
        /// </summary>
        public GameObject CopyMATypeDstOf(MAGranularity gran, int dataId)
        {
            return Copy(granTypeDict[gran.ToNativeGranularity()][dataId]);
        }
        
        /// <summary>
        /// 指定粒度であり、属性情報でのマテリアル分け結果としてexpectされるものをコピーして返します。
        /// </summary>
        public GameObject CopyMAAttrDstOf(MAGranularity gran, int dataId)
        {
            return Copy(granAttrDict[gran][dataId]);
        }

        public Material Material(int id)
        {
            return testMaterials[id];
        }
        
        public MAMaterialConfig<CityObjectTypeHierarchy.Node> MaterialConfigByType()
        {
            // テストデータに含まれる地物型を記述
            var types = new CityObjectType[]
                {
                    CityObjectType.COT_Building,
                    CityObjectType.COT_WallSurface,
                    CityObjectType.COT_RoofSurface,
                    CityObjectType.COT_GroundSurface,
                    CityObjectType.COT_OuterCeilingSurface
                }
                .Select(type => type.ToTypeNode())
                .ToArray();
            
            // 地物型ををもとに壁面を緑、屋根面を青とする設定にします
            var matConf = new MAMaterialConfig<CityObjectTypeHierarchy.Node>(types);
            var wall = matConf.GetConfFor(CityObjectType.COT_WallSurface.ToTypeNode());
            var roof = matConf.GetConfFor(CityObjectType.COT_RoofSurface.ToTypeNode());
            wall.ChangeMaterial = true;
            roof.ChangeMaterial = true;
            wall.Material = Material(0); // green 
            roof.Material = Material(1); // blue
            return matConf;
        }

        public MAMaterialConfig<string> MaterialConfigByAttr()
        {
            var attrKeys = new string[] { "車道交差部", "車道部", "都市高速道路", "都道府県道", "島", "歩道部" };
            var matConf = new MAMaterialConfig<string>(attrKeys);
            var driveWay = matConf.GetConfFor("車道部");
            var sideWalk = matConf.GetConfFor("歩道部");
            driveWay.ChangeMaterial = true;
            sideWalk.ChangeMaterial = true;
            driveWay.Material = Material(0); // green
            sideWalk.Material = Material(1); // blue
            return matConf;
        }

        private GameObject Copy(GameObject src)
        {
            var obj = Object.Instantiate(src);
            obj.name = obj.name.Replace("(Clone)", "");
            return obj;
        }
    }
}