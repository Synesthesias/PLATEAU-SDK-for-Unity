using NUnit.Framework;
using PLATEAU.Util;
using PLATEAU.Util.FileNames;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestGameObjNameParser
    {

        [TestCaseSource(nameof(testTryGetLod))]
        public (int, bool) Test_TryGetLod(string gameObjName)
        {
            bool succeed = GameObjNameParser.TryGetLod(gameObjName, out int lod);
            return (lod, succeed);
        }

        private static TestCaseData[] testTryGetLod =
        {
            new TestCaseData("LOD123_abc123")
                .Returns((123, true))
                .SetName("GameObject名からLODを返します。"),

            new TestCaseData("invalid_format_LOD")
                .Returns((-1, false))
                .SetName("不正な書式には -1, false を返します。"),

            new TestCaseData("LOD_abc")
                .Returns((-1, false))
                .SetName("不正な書式には -1, false を返します。")
        };

        [TestCaseSource(nameof(testTryGetId))]
        public (string, bool) Test_TryGetId(string gameObjName)
        {
            bool succeed = GameObjNameParser.TryGetId(gameObjName, out string id);
            return (id, succeed);
        }

        private static TestCaseData[] testTryGetId =
        {
            new TestCaseData("LOD1_abc_123")
                .Returns(("abc_123", true))
                .SetName("PlateauのID, trueを返します。"),
            
            new TestCaseData("id length is zero_")
                .Returns(("", false))
                .SetName("idの長さが0のとき 空文字, false を返します。"),
            
            new TestCaseData("no underscore")
                .Returns(("", false))
                .SetName("アンダースコアがないとき 空文字, false を返します。")
        };
    }
}