using System.Collections;
using System.IO;
using NUnit.Framework;
using PlateauUnitySDK.Editor.CityModelImportWindow;
using PlateauUnitySDK.Tests.TestUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlateauUnitySDK.Tests.EditModeTests.TestCityModelImportWindow
{
    [TestFixture]
    public class TestCityModelImportWindow
    {
        private CityModelImportWindow window;

        private static string testUdxPath = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Packages/PlateauUnitySDK/Tests/TestData/TestDataTokyoMini/udx"));

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            CityModelImportWindow.Open();
            this.window = EditorWindow.GetWindow<CityModelImportWindow>("都市モデルインポート");
            this.window.Repaint();
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            this.window.Close();
        }
        
        [Test]
        public void No_Error_With_Initial_Window()
        {
            // 初期画面でエラーがなければとりあえず良しとします。
        }
    }
}