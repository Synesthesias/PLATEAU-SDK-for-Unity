using System.IO;
using System.Linq;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.IO;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
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
}