using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlateauUnitySDK.Editor.FileConverter;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter {
    
    public class TestFilePathValidator {
        
        /// <summary>
        /// フルパスから Assets で始まるパスへの変換ができることを確認します。
        /// プロジェクトのAssetsフォルダが assetsDir にあると仮定して、 フルパス を変換したら expected になることを確認します。
        /// </summary>
        
        // Windowsのパス表記への対応をチェックします。
        [TestCase("C:/DummyUnityProjects/Assets", "C:\\DummyUnityProjects\\Assets\\FooBar\\FooBarModelFile.fbx", "Assets/FooBar/FooBarModelFile.fbx")]
        // Linuxのパス表記への対応をチェックします。
        [TestCase("/home/linuxUser/DummyUnityProjects/Assets", "/home/linuxUser/DummyUnityProjects/Assets/foobar.obj", "Assets/foobar.obj")]
        // 紛らわしい名前への対応をチェックします。
        [TestCase("Assets/Assets", "Assets/Assets/Assets/Assets", "Assets/Assets/Assets")]
        // 日本語名、絵文字、スペースへの対応をチェックします。
        [TestCase("C:/日本語話者の プロジェクト♪🎶/Assets", "C:/日本語話者の プロジェクト♪🎶/Assets/♪ 🎶.wav", "Assets/♪ 🎶.wav" )]
        
        public void Test_FullPathToAssetsPath_Normal(string assetsDir, string fullPath, string expectedAssetsPath) {
            // 後でAssetsフォルダのパス設定を戻すために覚えておきます。
            string prevDataPath = FilePathValidator.TestOnly_GetUnityProjectDataPath();
            
            // Assetsフォルダがこのような場所にあると仮定します。
            FilePathValidator.TestOnly_SetUnityProjectDataPath(assetsDir);
            
            // テストケースをチェックします。 
            Assert.AreEqual(expectedAssetsPath, FilePathValidator.FullPathToAssetsPath(fullPath));
            
            // Assetsフォルダの設定を戻します。
            FilePathValidator.TestOnly_SetUnityProjectDataPath(prevDataPath);
        }
    }
}
