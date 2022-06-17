using System.Collections;
using NUnit.Framework;
using PlateauUnitySDK.Editor.SingleFileConvert;
using UnityEditor;
using UnityEngine.TestTools;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestModelFileConvertWindow
    {
        private SingleFileConvertWindow window;
        
        [SetUp]
        public void SetUp()
        {
            SingleFileConvertWindow.Open();
            this.window = EditorWindow.GetWindow<SingleFileConvertWindow>();
        }
        
        [TearDown]
        public void TearDown()
        {
            this.window.Close();
        }
        
        [UnityTest]
        public IEnumerator Can_Paint_Window_Of_Initial_State()
        {
            // TODO ウィンドウの機能は何もテストできていませんが、
            // とりあえず初期画面がエラーなく表示できたらまあ良しとしています。
            this.window.Repaint();
            yield break;
        }
    }
}