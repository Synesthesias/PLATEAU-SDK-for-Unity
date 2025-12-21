using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using PLATEAU.Util;
using System.Text.RegularExpressions;

namespace PLATEAU.Tests.EditModeTests
{
    public class TestPluginSettings
    {
        /// <summary>
        /// MacOS向けのプラグインの設定が正しいかチェックします。
        /// このテストが失敗する場合、Plugins/MacOS以下の各バイナリの設定をインスペクタで見直してください。
        /// </summary>
        [Test]
        public void MacOS_Plugin_Cpu_Settings_Are_Correct()
        {
            // プロジェクトルートからの相対パス
            string pluginsRoot = PathUtil.SdkPathToAssetPath("Plugins/MacOS");
            
            // arm64フォルダのプラグインが、インスペクタでarm64向けになっている
            CheckPluginsInFolder(Path.Combine(pluginsRoot, "arm64"), "ARM64");

            // x86_64フォルダのプラグインが、インスペクタでIntel 64bit向けになっている
            CheckPluginsInFolder(Path.Combine(pluginsRoot, "x86_64"), "x86_64");
        }

        private void CheckPluginsInFolder(string folderPath, string expectedCpu)
        {
            if (!Directory.Exists(folderPath))
            {
                string fullPath = Path.GetFullPath(folderPath);
                if (!Directory.Exists(fullPath))
                {
                    Assert.Fail($"Directory not found: {folderPath} (Full path: {fullPath})");
                }
            }

            // ディレクトリ内の .dylib ファイルを取得
            var files = Directory.GetFiles(folderPath, "*.dylib"); 
            if (files.Length == 0)
            {
                // 何もテストしていないことになるので警告または失敗
                Assert.Fail($"No .dylib files found in {folderPath}");
            }

            foreach (var filePath in files)
            {
                string assetPath = filePath.Replace("\\", "/");
                
                PluginImporter importer = AssetImporter.GetAtPath(assetPath) as PluginImporter;
                if (importer == null)
                {
                    Assert.Fail($"Failed to get PluginImporter for: {assetPath}. Make sure the file is imported as a plugin.");
                }

                // macOS 向けに有効になっているかチェック
                bool isEnabled = importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX);
                Assert.IsTrue(isEnabled, $"Plugin should be enabled for MacOS: {assetPath}");

                // CPU 設定チェック
                string cpuSetting = importer.GetPlatformData(BuildTarget.StandaloneOSX, "CPU");
                Assert.AreEqual(expectedCpu, cpuSetting, $"CPU setting mismatch for: {assetPath}. Expected '{expectedCpu}', but got '{cpuSetting}'");

                // Editor 向けに有効になっているかチェック
                // 注: Windows環境でmacOS用Editor設定を PluginImporter から取得しようとすると、
                // OS不一致のためか値が取得できない(空文字になる)ことがあるため、メタファイルを直接解析してチェックする。
                CheckEditorSettingsInMetaFile(assetPath, expectedCpu);
            }
        }

        /// <summary>
        /// .meta ファイルをテキストとして読み込み、Editor設定(OS: OSX)のCPUアーキテクチャを確認します。
        /// Unity API (PluginImporter) ではクロスプラットフォームなEditor設定の取得が難しいため、この方式をとります。
        /// </summary>
        private void CheckEditorSettingsInMetaFile(string assetPath, string expectedCpu)
        {
            string metaPath = assetPath + ".meta";
            
            string fullMetaPath = Path.GetFullPath(metaPath);
            if (!File.Exists(fullMetaPath))
            {
                Assert.Fail($"Meta file not found: {fullMetaPath} (Original path: {metaPath})");
            }

            var lines = File.ReadAllLines(fullMetaPath);
            bool inEditorBlock = false;
            bool foundOsX = false;
            bool foundCpu = false;

            // YAMLを行ごとに解析
            foreach (var line in lines)
            {
                // 新しいプラットフォーム設定ブロックの開始
                if (line.Contains("- first:"))
                {
                    inEditorBlock = false;
                }

                // Editor設定ブロックの特定
                if (line.Contains("Editor: Editor"))
                {
                    inEditorBlock = true;
                }

                if (inEditorBlock)
                {
                    if (line.Contains("OS: OSX"))
                    {
                        foundOsX = true;
                    }
                    // CPU設定の確認 (CPU: ARM64 など)
                    // 空白を含めて曖昧マッチさせる
                    if (Regex.IsMatch(line, $@"CPU:\s*{Regex.Escape(expectedCpu)}"))
                    {
                        foundCpu = true;
                    }
                }
            }

            if (!foundOsX)
            {
                Assert.Fail($"Editor settings for OSX not found in meta file: {metaPath}.");
            }

            if (!foundCpu)
            {
                Assert.Fail($"Editor CPU setting mismatch in meta file: {metaPath}. Expected 'CPU: {expectedCpu}' in OSX Editor settings.");
            }
        }
    }
}
