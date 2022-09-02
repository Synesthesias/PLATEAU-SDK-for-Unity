using NUnit.Framework;
using PLATEAU.Editor.CityImport;
using System.Collections;
using UnityEditor;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestCityModelImportWindow
    {
        private CityImportWindow window;


        [UnitySetUp]
        public IEnumerator SetUp()
        {
            CityImportWindow.Open();
            this.window = EditorWindow.GetWindow<CityImportWindow>("都市モデルをインポート");
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