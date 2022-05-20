using System.Collections;
using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine.TestTools;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestModelFileConvertWindow
    {
        private ModelFileConvertWindow window;
        
        [SetUp]
        public void SetUp()
        {
            ModelFileConvertWindow.Open();
            this.window = EditorWindow.GetWindow<ModelFileConvertWindow>();
        }
        
        [TearDown]
        public void TearDown()
        {
            this.window.Close();
        }
        
        [UnityTest]
        public IEnumerator Can_Paint_Window_Of_Initial_State()
        {
            this.window.Repaint();
            yield break;
        }
    }
}