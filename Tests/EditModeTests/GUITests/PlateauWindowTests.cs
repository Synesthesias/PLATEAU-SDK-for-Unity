using System.Collections;
using NUnit.Framework;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;
using PLATEAU.Tests.TestUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests.GUITests
{
    [TestFixture]
    public class PlateauWindowTests
    {
        private PlateauWindow window;
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            this.window = EditorWindow.GetWindow<PlateauWindow>();
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            this.window.Close();
        }

        [UnityTest]
        public IEnumerator CanOpenEachMainTabs()
        {
            OpenImportTab();
            this.window.Repaint();
            yield return null;
            OpenAdjustTab();
            this.window.Repaint();
            yield return null;
            OpenExportTab();
            this.window.Repaint();
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestCityAdjustGui()
        {
            // テストデータをインポートします。
            // TODO このデータは別に空でも良いのに、時間がかかるテストデータを使ってしまっている。
            yield return null;
            var cityDefinition = TestCityDefinition.MiniTokyo;
            LogAssert.ignoreFailingMessages = true;
            yield return cityDefinition.ImportLocal().AsIEnumerator();
            LogAssert.ignoreFailingMessages = false;
            yield return null;
            
            var adjustGui = OpenAdjustTab();
            var cityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            Assert.IsNotNull(cityModel, "インポート後に PLATEAUInstancedCityModelが存在する");
            // GUIの設定対象にインポートした都市モデルを設定
            ReflectionUtil.SetPrivateFieldVal(typeof(CityAdjustGUI), adjustGui, CityAdjustGUI.NameOfAdjustTarget, cityModel);
            // 対象が変わったことを通知
            ReflectionUtil.InvokePrivateMethod(typeof(CityAdjustGUI), adjustGui,
                CityAdjustGUI.NameOfOnChangeTargetCityModel, cityModel);
            yield return null;
            // 都市の調整GUIがエラーなく描画されることを確認
            this.window.Repaint();
            yield return null;
            
            
        }

        private void OpenImportTab() => SetMainTabIndex(0);

        private CityAdjustGUI OpenAdjustTab() => (CityAdjustGUI)SetMainTabIndex(1);
        private void OpenExportTab() => SetMainTabIndex(2);
        private IEditorDrawable SetMainTabIndex(int index)
        {
            // PlateauWindow.tabIndex をリフレクションで設定します。
            var innerGui = ReflectionUtil.GetPrivateFieldVal<PlateauWindowGUI>(
                typeof(PlateauWindow), this.window, PlateauWindow.NameOfInnerGuiField);
            ReflectionUtil.SetPrivateFieldVal(typeof(PlateauWindowGUI), innerGui, PlateauWindowGUI.NameOfTabIndex, index);
            // タブの中身を描画するクラスを取得して返します。
            var tabDrawables = ReflectionUtil.GetPrivateFieldVal<IEditorDrawable[]>(typeof(PlateauWindowGUI), innerGui,
                PlateauWindowGUI.NameOfTabGUIArray);
            return tabDrawables[index];
        }
        
    }
}
