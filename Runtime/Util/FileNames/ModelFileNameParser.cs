using System;
using System.IO;

namespace PLATEAU.Util.FileNames
{
    /// <summary>
    /// 3Dモデルのファイル名に関する規約を表現します。
    /// 
    /// ファイル名は
    /// LOD{num}_{gmlファイル名}.obj
    /// であることが前提です。
    /// </summary>
    internal static class ModelFileNameParser
    {
        /// <summary>
        /// <paramref name="lod"/>, <paramref name="gmlFilePath"/> から
        /// 3Dモデルファイル名(拡張子抜き)を返します。
        /// </summary>
        public static string FileNameWithoutExtension(int lod, string gmlFilePath)
        {
            string gmlName = GmlFileNameParser.FileNameWithoutExtension(gmlFilePath);
            return $"LOD{lod}_{gmlName}";
        }

        /// <summary>
        /// <paramref name="lod"/>, <paramref name="gmlFilePath"/> から
        /// 3Dモデルファイル名（拡張子付き）を返します。
        /// </summary>
        public static string FileName(int lod, string gmlFilePath)
        {
            return $"{FileNameWithoutExtension(lod, gmlFilePath)}.obj";
        }

        /// <summary>
        /// ファイル名からLODの数値を取り出します。
        /// </summary>
        public static int GetLod(string modelFilePath)
        {
            string fileName = Path.GetFileName(modelFilePath);
            if (!fileName.StartsWith("LOD"))
            {
                throw new ArgumentException($"modelFileName should starts with 'LOD', but actual is {fileName}.");
            }

            int underScoreIndex = fileName.IndexOf('_');
            int lodLen = "LOD".Length;
            string lodStr = fileName.Substring(lodLen, underScoreIndex - lodLen);
            return Convert.ToInt32(lodStr);
        }
    }
}