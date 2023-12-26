using System;
using System.Collections;
using NUnit.Framework;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.ImportGUIParts;
using PLATEAU.Tests.TestUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

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
        public IEnumerator OpenCityImportRemoteGuiAndWaitForFewSecondsThenDatasetFetchComplete()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var gui = OpenImportTab();
            // 初期画面を描画します。
            this.window.Repaint();
            yield return null;
            // サーバーインポートに切り替え、そのGUIを取得します。
            const int remoteImportTabIndex = 1;
            ReflectionUtil.SetPrivateFieldVal(typeof(CityAddGUI), gui, CityAddGUI.NameOfImportTabIndex, remoteImportTabIndex);
            var confGUI =
                (CityImportConfigGUI)ReflectionUtil.GetPrivateFieldVal<IEditorDrawable[]>(typeof(CityAddGUI), gui,
                    CityAddGUI.NameOfImportTabGUIArray)[remoteImportTabIndex];
            var remoteGUI =
                (ConfigGUIBeforeAreaSelectRemote)ReflectionUtil.GetPrivateFieldVal<IConfigGUIBeforeAreaSelect>(
                    typeof(CityImportConfigGUI), confGUI,
                    CityImportConfigGUI.NameOfConfigGUIBeforeAreaSelect);
            yield return null;
            this.window.Repaint();

            // GUIを開いてから数秒以内にサーバーからデータセットの一覧をダウンロードできることを確認します。
            var startTime = DateTime.Now;
            bool isDatasetFetchSucceed = false;
            while ((DateTime.Now - startTime).TotalMilliseconds < 4000)
            {
                yield return null;
                if (remoteGUI.DatasetFetchStatus == ServerDatasetFetchGUI.LoadStatusEnum.Success)
                {
                    isDatasetFetchSucceed = true;
                    break;
                }
            }
            Assert.IsTrue(isDatasetFetchSucceed, "リモートインポートの画面を開いてから数秒以内にサーバーからのデータ一覧の取得が完了します。");

            EditorSceneManager.CloseScene(testScene, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        

        private CityAddGUI OpenImportTab() => (CityAddGUI)SetMainTabIndex(0);

        private CityModificationFrameGUI OpenAdjustTab() => (CityModificationFrameGUI)SetMainTabIndex(1);
        private void OpenExportTab() => SetMainTabIndex(2);
        private IEditorDrawable SetMainTabIndex(int index)
        {
            // PlateauWindow.tabIndex をリフレクションを用いて設定します。
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
