using System.Collections;
using NUnit.Framework;
using PlateauUnitySDK.Editor.CityImport;
using UnityEditor;
using UnityEngine.TestTools;

namespace PlateauUnitySDK.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityModelImportWindow
    {
        private CityImportWindow window;
        

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            CityImportWindow.Open();
            this.window = EditorWindow.GetWindow<CityImportWindow>("都市モデルインポート");
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