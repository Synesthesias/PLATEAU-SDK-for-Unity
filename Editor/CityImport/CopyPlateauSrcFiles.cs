﻿using System.IO;
using PLATEAU.CityMeta;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{

    /// <summary>
    /// Plateauの元データをコピーします。
    /// インポート時に <see cref="CityImporter"/> が利用します。
    /// </summary>
    internal static class CopyPlateauSrcFiles
    {

        /// <summary>
        /// Plateau元データのうち、 <paramref name="gmlRelativePaths"/> のリストにある gmlファイルとそれに関連するもののみをコピーします。
        /// 変換先 <paramref name="copyDest"/> は Assets フォルダ内であることが前提です。
        /// </summary>
        public static void SelectCopy(string udxPathBeforeImport, string copyDest,
            string[] gmlRelativePaths)
        {
            int numGml = gmlRelativePaths.Length;
            ProgressBar("コピー中", 0, numGml);
            string rootDirName = PlateauSourcePath.RootDirName(udxPathBeforeImport);
            if (rootDirName == null) throw new FileNotFoundException($"{nameof(rootDirName)} is null.");

            // コピー先のルートフォルダを作成します。
            // 例: Tokyoをコピーする場合のパスの例を以下に示します。
            //     Assets/StreamingAssets/PLATEAU/Tokyo　フォルダを作ります。
            string dstRootFolder = Path.Combine(copyDest, rootDirName);
            Mkdir(dstRootFolder);

            // codelists をコピーします。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/codelists/****.xml をコピーにより作成します。
            ProgressBar("コピー中 : codelists", 0, numGml);
            const string codelistsFolderName = "codelists";
            PathUtil.CloneDirectory(Path.Combine(PlateauSourcePath.RootFullPath(udxPathBeforeImport), codelistsFolderName), dstRootFolder);

            // udxのパスです。
            // 例: Assets/StreamingAssets/PLATEAU/Tokyo/udx
            string dstUdxFolder = Path.Combine(dstRootFolder, "udx");

            // udxフォルダのうち対象のgmlファイルをコピーします。
            int loopCnt = 0;
            foreach (string gml in gmlRelativePaths)
            {
                loopCnt++;
                ProgressBar($"コピー中 : [{loopCnt}/{numGml}] {gml}", loopCnt, numGml);
                string gmlType = GmlFileNameParser.GetGmlTypeStr(gml);
                // 地物タイプのディレクトリを作ります。
                // 例: gml のタイプが bldg なら、
                //     Assets/StreamingAssets/PLATEAU/Tokyo/udx/bldg　ができます。
                string dstObjTypeFolder = Path.Combine(dstUdxFolder, gmlType);
                Mkdir(dstObjTypeFolder);

                // gmlファイルをコピーします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234.gml　ができます。
                string gmlName = Path.GetFileName(gml);
                string srcObjTypeFolder = Path.Combine(udxPathBeforeImport, gmlType);
                File.Copy(Path.Combine(srcObjTypeFolder, gmlName), Path.Combine(dstObjTypeFolder, gmlName), true);

                // gmlファイルに関連するフォルダをコピーします。
                // gmlの名称からオプションと拡張子を除いた文字列がフォルダ名に含まれていれば、コピー対象のディレクトリとみなします。
                // 例: Assets/StreamingAssets/PLATEAU/Tokyo/bldg/1234_appearance/texture_number.jpg　などがコピーされます。 
                string gmlIdentity = GmlFileNameParser.NameWithoutOption(gml);
                foreach (var srcDir in Directory.GetDirectories(srcObjTypeFolder))
                {
                    string srcDirName = new DirectoryInfo(srcDir).Name;
                    if (srcDirName.Contains(gmlIdentity))
                    {
                        PathUtil.CloneDirectory(srcDir, dstObjTypeFolder);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// <paramref name="fullPath"/> にフォルダを作ります。
        /// すでにある場合は何もしません。
        /// <paramref name="fullPath"/> は Assets フォルダ内を指すことを前提とします。
        /// パスの途中のフォルダが存在しないなら、それも合わせて作ります。
        /// </summary>
        private static void Mkdir(string fullPath)
        {
            // すでにフォルダが存在するなら何もしません。
            if (Directory.Exists(fullPath)) return;
            
            string assetPath = PathUtil.FullPathToAssetsPath(fullPath);
            string newDirName = new DirectoryInfo(assetPath).Name;
            var parentDirInfo = new DirectoryInfo(fullPath).Parent;
            string parentDirFullPath = parentDirInfo == null ? "" : parentDirInfo.FullName;
            
            // 指定パスの親ディレクトリも存在しなければ、パスを親へさかのぼり、親のあるパスから再帰的にフォルダを作ります。
            if (parentDirFullPath != "" && !Directory.Exists(parentDirFullPath))
            {
                Mkdir(parentDirFullPath);
            }
            
            // フォルダを作ります。
            string parentDirAssetPath = PathUtil.FullPathToAssetsPath(parentDirFullPath);
            AssetDatabase.CreateFolder(parentDirAssetPath, newDirName);
        }
        

        private static void ProgressBar(string info, int currentCount, int maxCount)
        {
            EditorUtility.DisplayProgressBar("コピー中", info, ((float)currentCount)/((float)maxCount));
        }
        
    }
}