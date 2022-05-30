using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlateauUnitySDK.Editor.FileConverter;
using PlateauUnitySDK.Tests.TestUtils;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter {
    
    [TestFixture]
    public class TestFilePathValidator {

        /// <summary>
        /// 入力ファイル用のパスとして正しいかどうかを判定できるか確認します。
        /// </summary>
        
        // 実在するファイルが与えられたときのテストケース
        #if UNITY_STANDALONE_WIN
        // このコードを動かしているWindows PCであればおそらく存在するであろうファイルのパスを例にとり、存在するファイルが与えられたときに有効判定が出ることをチェックします。
        [TestCase("C:\\Windows\\System32\\input.dll", "dll", true)]
        // 拡張子が合わないときに false になることもチェックします。
        [TestCase("C:\\Windows\\System32\\input.dll", "wrongExtension", false)]
        #else
        // Linuxで上と同様のことをテストします。
        [TestCase("/etc/issue.net", "net", true)]
        [TestCase("/etc/issue.net", "wrongExtension", false)]
        #endif
        
        // 実在しないファイルが与えられたときのテストケース
        [TestCase("/NotFound/Dummy/Missing.fbx", "fbx", false)]
        public void Test_IsValidInputFilePath(string filePath, string extension, bool expected) {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(FilePathValidator.IsValidInputFilePath(filePath, extension, false), expected);
        }
        
        
        
        /// <summary>
        /// 出力ファイル用のパスとして正しいかどうかを判定できるか確認します。
        /// </summary>
        
        // 実在するファイルが与えられたときのテストケース
        #if UNITY_STANDALONE_WIN
        // このコードを動かしているWindows PCであればおそらく存在するであろうフォルダ(Program Files)に例にとり、
        // 存在するフォルダでの新規ファイル作成(fbx)を想定したときに有効判定が出ることをチェックします。
        [TestCase("C:\\Program Files\\User_wants_to_save_here.fbx", "fbx", true)]
        // 拡張子が合わないときに false になることもチェックします。
        [TestCase("C:\\Program Files\\User_wants_to_save_here.fbx", "wrongExtension", false)]
        #else
        // Linuxで上と同様のことをテストします。
        [TestCase("/etc/User_wants_to_save_here.fbx", "fbx", true)]
        [TestCase("/etc/User_wants_to_save_here.fbx", "wrongExtension", false)]
        #endif
        
        // 実在しないファイルが与えられたときのテストケース
        [TestCase("/NotFound/Dummy/Missing.fbx", "fbx", false)]
        public void Test_IsValidOutputFilePath(string filePath, string extension, bool expected) {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(FilePathValidator.IsValidOutputFilePath(filePath, extension), expected);
        }
        
        
        
        /// <summary>
        /// フルパスから Assets で始まるパスへの変換ができることを確認します。
        /// プロジェクトのAssetsフォルダが assetsDir にあると仮定して、 フルパス を変換したら expected になることを確認します。
        /// </summary>
        
        // Windowsのパス表記への対応をチェックします。
        #if UNITY_STANDALONE_WIN
        [TestCase("C:/DummyUnityProjects/Assets", "C:\\DummyUnityProjects\\Assets\\FooBar\\FooBarModelFile.fbx", "Assets/FooBar/FooBarModelFile.fbx")]
        #endif
        // Linuxのパス表記への対応をチェックします。
        [TestCase("/home/linuxUser/DummyUnityProjects/Assets", "/home/linuxUser/DummyUnityProjects/Assets/foobar.obj", "Assets/foobar.obj")]
        // 紛らわしい名前への対応をチェックします。
        [TestCase("Assets/Assets", "Assets/Assets/Assets/Assets", "Assets/Assets/Assets")]
        // 日本語名、絵文字、スペースへの対応をチェックします。
        [TestCase("C:/日本語話者の プロジェクト♪🎶/Assets", "C:/日本語話者の プロジェクト♪🎶/Assets/♪ 🎶.wav", "Assets/♪ 🎶.wav" )]
        
        public void Test_FullPathToAssetsPath_Normal(string assetsDir, string fullPath, string expectedAssetsPath) {
            // 後でAssetsフォルダのパス設定を戻すために覚えておきます。
            string prevDataPath = ReflectionUtil.GetPrivateStaticFieldVal<string>(typeof(FilePathValidator), "unityProjectDataPath");
            
            // Assetsフォルダがこのような場所にあると仮定します。
            ReflectionUtil.SetPrivateStaticFieldVal(assetsDir, typeof(FilePathValidator), "unityProjectDataPath");
            
            // テストケースをチェックします。 
            Assert.AreEqual(expectedAssetsPath, FilePathValidator.FullPathToAssetsPath(fullPath));
            
            // Assetsフォルダの設定を戻します。
            ReflectionUtil.SetPrivateStaticFieldVal(prevDataPath, typeof(FilePathValidator), "unityProjectDataPath");
        }
        
        [Test] 
        public void FullPathToAssetsPath_Returns_Error_When_Outside_Assets_Folder() {
            Assert.That(()=> {
                    FilePathValidator.FullPathToAssetsPath("C:\\dummy\\OutsideAssets\\a.fbx");
                },
                Throws.TypeOf<IOException>());
        }


        
    }
}
