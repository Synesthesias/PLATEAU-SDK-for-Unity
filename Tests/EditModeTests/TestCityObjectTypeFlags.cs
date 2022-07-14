using System;
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
            var expect =
                CityObjectType.COT_WallSurface |
                CityObjectType.COT_RoofSurface;
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
                (_,__) => ~0
            );

            var expect = CityObjectType.COT_All;
            
            Assert.AreEqual(expect, selected);
        }
    }
}