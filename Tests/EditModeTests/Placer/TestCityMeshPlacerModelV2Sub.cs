using System.IO;
using System.Linq;
using NUnit.Framework;
using PLATEAU.CityGML;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using UnityEngine;
using static PLATEAU.CityMeta.CityMeshPlacerConfig;
using static PLATEAU.Tests.EditModeTests.Placer.TestPlacerUtil;

namespace PLATEAU.Tests.EditModeTests.Placer
{
    /// <summary>
    /// <see cref="CityMeshPlacerModelV2"/> のテストで、
    /// インポート設定が <see cref="IO.MeshGranularity.PerAtomicFeatureObject"/> であるケースをテストします。
    /// </summary>
    [TestFixture]
    public class TestCityMeshPlacerModelV2ForAtomic : TestCityMeshPlacerModelV2Base
    {
        protected override MeshGranularity MeshGranularity => MeshGranularity.PerAtomicFeatureObject;
        // テスト内容は親クラスのものを使い回します。
        
        // Atomic 専用のテストです。
        [Test]
        public void CityObjectTypeMask_Works()
        {
            Place(PlaceMethod.PlaceSelectedLodOrMax, 2, Metadata,
                (placerConf) =>
                {
                    var buildingConf = placerConf.GetPerTypeConfig(GmlType.Building);
                    buildingConf.cityObjectTypeFlags =
                        (ulong)CityObjectType.COT_WallSurface | (ulong)CityObjectType.COT_GroundSurface;
                }
            );
            Debug.Log("Flag = " + Metadata.cityImportConfig.cityMeshPlacerConfig.GetPerTypeConfig(GmlType.Building).cityObjectTypeFlags);
            SceneUtil.AssertGameObjExists("LOD2_wall_HNAP0279_p2471_4"); // wall が存在する
            SceneUtil.AssertGameObjExists("LOD2_gnd_4cc1a87b-838b-4528-89ec-6065d8442251"); // gnd が存在する
            SceneUtil.AssertGameObjNotExists("LOD2_roof_HNAP0279_p2471_0"); // roof が存在しない
        }
    }

    /// <summary>
    /// <see cref="CityMeshPlacerModelV2"/> のテストで、
    /// インポート設定が <see cref="IO.MeshGranularity.PerPrimaryFeatureObject"/> であるケースをテストします。
    /// </summary>
    [TestFixture]
    public class TestCityMeshPlacerModelV2ForPrimary : TestCityMeshPlacerModelV2Base
    {
        protected override MeshGranularity MeshGranularity => MeshGranularity.PerPrimaryFeatureObject;
        // テスト内容は親クラスのものを使い回します。
    }
    
    /// <summary>
    /// <see cref="CityMeshPlacerModelV2"/> のテストで、
    /// インポート設定が <see cref="IO.MeshGranularity.PerCityModelArea"/> であるケースをテストします。
    /// </summary>
    [TestFixture]
    public class TestCityMeshPlacerModelV2ForArea : TestCityMeshPlacerModelV2Base
    {
        protected override MeshGranularity MeshGranularity => MeshGranularity.PerCityModelArea;
        // テスト内容は親クラスのものを使い回します。
    }
}