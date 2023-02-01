using NUnit.Framework;
using PLATEAU.Tests.TestUtils;
using PLATEAU.Util;
using System.IO;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.EditModeTests
{

    [TestFixture]
    public class TestPathUtil
    {


        [TestCaseSource(nameof(testIsValidInputFilePath))]
        public bool Test_IsValidInputFilePath(string filePath, string extension)
        {
            LogAssert.ignoreFailingMessages = true;
            bool result = PathUtil.IsValidInputFilePath(filePath, extension, false);
            return result;
        }


        private static TestCaseData[] testIsValidInputFilePath =
        {
#if UNITY_STANDALONE_WIN
            
            new TestCaseData("C:\\Windows\\System32\\input.dll", "dll")
                .Returns(true)
                .SetName("[Windowsのみ] 実在するファイルパスは true になります。"),

            new TestCaseData("C:\\Windows\\System32\\input.dll", "間違い拡張子")
                .Returns(false)
                .SetName("[Windowsのみ] 実在するファイルであっても拡張子が違うと false になります。"),

#endif
            
            new TestCaseData("/存在しない/ファイルパス/ダミー.fbx", "fbx")
                .Returns(false)
                .SetName("実在しないファイルパスは false になります。")

        };



        [TestCaseSource(nameof(testIsValidOutputPath))]
        public bool Test_IsValidOutputFilePath(string filePath, string extension)
        {
            LogAssert.ignoreFailingMessages = true;
            bool result = PathUtil.IsValidOutputFilePath(filePath, extension);
            return result;
        }


        private static TestCaseData[] testIsValidOutputPath =
        {
#if UNITY_STANDALONE_WIN

            new TestCaseData("C:\\Program Files\\User_wants_to_save_here.fbx", "fbx")
                .Returns(true)
                .SetName("[Windowsのみ] 実在するフォルダ上で新規のファイル名を指定した時に true になります。"),

            new TestCaseData("C:\\Program Files\\User_wants_to_save_here.fbx", "間違い拡張子")
                .Returns(false)
                .SetName("[Windowsのみ] 実在するフォルダであっても、拡張子が違うと false になります。"),

#else

            new TestCaseData("/etc/User_wants_to_save_here.fbx", "fbx")
                .Returns(true)
                .SetName("[Linuxのみ] 実在するフォルダ上で新規のファイル名を指定した時に true になります"),

            new TestCaseData("/etc/User_wants_to_save_here.fbx", "間違い拡張子")
                .Returns(false)
                .SetName("[Linuxのみ] 実在するフォルダであっても、拡張子が違うと false になります"),

#endif

            new TestCaseData("/存在しないフォルダ/ダミー/新規ファイル.fbx", "fbx")
                .Returns(false)
                .SetName("実在しないフォルダでは false になります。")

        };



        [TestCaseSource(nameof(testFullPathToAssetsPathNormal))]
        public string Test_FullPathToAssetsPath_Normal(string assetsDir, string fullPath)
        {
            // 後でAssetsフォルダのパス設定を戻すために覚えておきます。
            string prevDataPath = ReflectionUtil.GetPrivateStaticFieldVal<string>(typeof(PathUtil), "unityProjectDataPath");

            // Assetsフォルダが引数のパスにあると仮定します。
            ReflectionUtil.SetPrivateStaticFieldVal(assetsDir, typeof(PathUtil), "unityProjectDataPath");

            // テスト対象コードを実行します。 
            string result = PathUtil.FullPathToAssetsPath(fullPath);

            // Assetsフォルダの設定を戻します。
            ReflectionUtil.SetPrivateStaticFieldVal(prevDataPath, typeof(PathUtil), "unityProjectDataPath");
            return result;
        }

        private static TestCaseData[] testFullPathToAssetsPathNormal =
        {
#if UNITY_STANDALONE_WIN

            new TestCaseData("C:/DummyUnityProjects/Assets",
                    "C:\\DummyUnityProjects\\Assets\\FooBar\\FooBarModelFile.fbx")
                .Returns("Assets/FooBar/FooBarModelFile.fbx")
                .SetName("[Windowsのみ] バックスラッシュ区切りのフルパスから、Assetsで始まるパスへ変換します。"),

#endif

            new TestCaseData("/home/linuxUser/DummyUnityProjects/Assets",
                    "/home/linuxUser/DummyUnityProjects/Assets/foobar.obj")
                .Returns("Assets/foobar.obj")
                .SetName("Linux形式のフルパスから、Assetsで始まるパスへ変換します。"),

            new TestCaseData("Assets/Assets", "Assets/Assets/Assets/Assets")
                .Returns("Assets/Assets/Assets")
                .SetName("まぎらわしい名前のパスに対応します。"),

            new TestCaseData("C:/日本語話者の プロジェクト♪🎶/Assets", "C:/日本語話者の プロジェクト♪🎶/Assets/♪ 🎶.wav")
                .Returns("Assets/♪ 🎶.wav")
                .SetName("絵文字やスペースが含まれるパスに対応します。")
        };

        [Test]
        public void FullPathToAssetsPath_Returns_Error_When_Outside_Assets_Folder()
        {
            Assert.That(() =>
                {
                    PathUtil.FullPathToAssetsPath("C:\\dummy\\OutsideAssets\\a.fbx");
                },
                Throws.TypeOf<IOException>(),
                $"Assetsフォルダの外のパスが {nameof(PathUtil.FullPathToAssetsPath)} に渡されたとき、例外を出す"
            );
        }



    }
}
