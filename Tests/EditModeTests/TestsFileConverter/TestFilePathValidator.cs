using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;
using PlateauUnitySDK.Editor.FileConverter;
using UnityEditor.VersionControl;

namespace PlateauUnitySDK.Tests.EditModeTests.TestsFileConverter {
    
    public class TestFilePathValidator {

        /// <summary>
        /// 入力ファイル用のパスとして正しいかどうかを判定できるか確認します。
        /// </summary>
        
        // 実在するファイルが与えられたときのテストケース
        #if UNITY_STANDALONE_WIN
        // このコードを動かしているWindows PCであればおそらく存在するであろうファイルのパスを例にとり、存在するファイルが与えられたときに有効判定が出ることをチェックします。
        [TestCase("C:\\Program Files\\Unity\\Hub\\Editor\\2020.3.32f1\\Editor\\Unity.exe", "exe", true)]
        // 拡張子が合わないときに false になることもチェックします。
        [TestCase("C:\\Program Files\\Unity\\Hub\\Editor\\2020.3.32f1\\Editor\\Unity.exe", "wrongExtension", false)]
        
        // Windows以外のOSをお使いの方は、お手数ですがここに次のような記述を追加してください:
        // #elif そのOS固有のキーワード
        // [TestCase("そのOSの利用者はみんな持っているであろうファイルのパス.何かの拡張子", "何かの拡張子", true]
        // [TestCase("同上.何かの拡張子", "合わない拡張子", false]
        #else
        このコードは処理されないはずです
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
        // このコードを動かしているWindows PCであればおそらく存在するであろうフォルダを例にとり、存在するフォルダが与えられたときに有効判定が出ることをチェックします。
        [TestCase("C:\\Program Files\\User_wants_to_save_here.fbx", "fbx", true)]
        // 拡張子が合わないときに false になることもチェックします。
        [TestCase("C:\\Program Files\\User_wants_to_save_here.fbx", "wrongExtension", false)]
        
        // Windows以外のOSをお使いの方は、お手数ですがここに次のような記述を追加してください:
        // #elif そのOS固有のキーワード
        // [TestCase("そのOSの利用者はみんな持っているであろうフォルダのパス/foo.何かの拡張子", "何かの拡張子", true]
        // [TestCase("同上/foo.何かの拡張子", "合わない拡張子", false]
        #else
        このコードは処理されないはずです
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


        [Test] // Assetsフォルダ以下は未対応なので例外を出すことを確認します。
        public void Test_FullPathToAssetsPath_Error() {
            Assert.That(()=> {
                    FilePathValidator.FullPathToAssetsPath("C:\\dummy\\OutsideAssets\\a.fbx");
                },
                Throws.TypeOf<IOException>());
        }
    }
}
