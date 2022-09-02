using NUnit.Framework;
using PLATEAU.CityGML;
using PLATEAU.Util.CityObjectTypeExtensions;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityObjectTypeFlags
    {
        [Test]
        public void Test_FlagFieldInner_When_2_Of_3_Selected()
        {
            var candidates =
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface |
                CityObjectType.COT_GroundSurface;

            // 選択肢のうち WallSurface, RoofSurface を選択
            var selected = CityObjectTypeFlagsEditor.FlagFieldInner(candidates, 0,
                (_, __) => 3
            );

            // 期待すること :
            // ・Wall, Roof は選択されているので ビットは 1 です。
            // ・Ground は選択されていないので ビットは 0 です。
            // ・選択候補にないものは ビットは 1 です。
            // まとめると、 Ground のビットが 0 で、それ以外は 1 です。
            var expect = ~CityObjectType.COT_GroundSurface;

            Assert.AreEqual(expect, selected);
        }

        [Test]
        public void Test_FlagFieldInner_When_Everything_Is_Selected()
        {
            var candidates =
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface |
                CityObjectType.COT_GroundSurface;

            var selected = CityObjectTypeFlagsEditor.FlagFieldInner(candidates, 0,
                // GUI で Everything を選択すると全フラグ1になります。
                (_, __) => ~0
            );

            var expect = CityObjectType.COT_All;

            Assert.AreEqual(expect, selected);
        }
    }
}