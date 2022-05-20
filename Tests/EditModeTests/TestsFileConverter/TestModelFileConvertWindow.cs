using NUnit.Framework;
using PlateauUnitySDK.Editor.FileConverter;
using UnityEditor;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter
{
    [TestFixture]
    public class TestModelFileConvertWindow
    {
        [SetUp]
        public void SetUp()
        {
            ModelFileConvertWindow.Open();
        }
        
        [TearDown]
        public void TearDown()
        {
            EditorWindow.GetWindow<ModelFileConvertWindow>().Close();
        }
        
        [Test]
        public void Show_Window_Without_Error()
        {
            // SetUpが正常に行われれば良しとします
        }
    }
}