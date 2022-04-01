using System;
using System.IO;
using UnityEngine;

namespace PlateauUnitySDK.Editor.FileConverter {
    
    /// <summary>
    /// ファイルパスが正しいかどうか検証します。
    /// </summary>
    public static class FilePathValidator {

        /// <summary>
        /// 入力ファイル用のパスとして正しければtrue,不適切であればfalseを返します。
        /// </summary>
        public static bool IsValidInputFilePath(string filePath, string expectedExtension) {
            try {
                CheckFileExist(filePath);
                CheckExtension(filePath, expectedExtension);
            }
            catch (Exception e) {
                Debug.LogError($"Input {expectedExtension} file path is invalid:\n{e}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 出力用のファイルパスとして正しければtrue,不適切であればfalseを返します。
        /// </summary>
        public static bool IsValidOutputFilePath(string filePath, string expectedExtension) {
            try {
                CheckDirectoryExist(filePath);
                CheckExtension(filePath, expectedExtension);
            }
            catch (Exception e) {
                Debug.LogError($"Output {expectedExtension} file path is invalid:\n{e}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定パスにファイルが存在するかチェックします。
        /// 存在しなければ例外を投げます。
        /// </summary>
        private static void CheckFileExist(string filePath) {
            if (File.Exists(filePath)) return;
            throw new IOException($"File is not found on the path :\n{filePath}");
        }

        /// <summary>
        /// 指定パスのうちディレクトリ部分が存在するかチェックします。
        /// 存在しなければ例外を投げます。
        /// </summary>
        private static void CheckDirectoryExist(string filePath) {
            string directory = Path.GetDirectoryName(filePath);
            if (Directory.Exists(directory)) return;
            throw new IOException($"Directory is not found on the path :\n{directory}.");
        }

        /// <summary>
        /// ファイルパスの拡張子が expectedExtension と同一かチェックします。
        /// 異なる場合は例外を投げます。
        /// </summary>
        private static void CheckExtension(string filePath, string expectedExtension) {
            string actualExtension = Path.GetExtension(filePath).ToLower().Replace(".", "");
            if (actualExtension == expectedExtension) return;
            throw new IOException(
                $"The file extension should be '{expectedExtension}', but actual is '{actualExtension}'"
                );
        }
    }
}
