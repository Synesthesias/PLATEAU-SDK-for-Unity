﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PlateauUnitySDK.Editor.FileConverter.Converters;
using PlateauUnitySDK.Runtime.CityMapMeta;

namespace PlateauUnitySDK.Editor.CityModelImportWindow
{
    /// <summary>
    /// udxフォルダ内のgmlファイルを検索します。
    /// 
    /// 前提:
    /// ・ディレクトリ構造が udx/(地物型)/(複数のgmlファイル) になっていることを前提とします。
    /// ・gmlファイル名は [地域メッシュコード]_[地物型]_[CRS]_[オプション].gml です。
    /// 　詳しくは国交省仕様書 Ver2 の 324ページを参照してください。
    /// </summary>
    public class GmlFileSearcher
    {

        /// <summary>
        /// 地域メッシュコードからファイルリストへの辞書です。
        /// ここでいうファイルリストとは <see cref="udxFolderPath"/> からの相対パスのリストです。
        /// 例: {53394525 => {bldg\53394525_bldg_6697_2_op.gml  brid\53394525_brid_6697_op.gml }}
        /// </summary>
        private Dictionary<string, List<string>> fileTable;

        private string udxFolderPath = "";
        // public string UdxFolderPath
        // {
        //     set => this.udxFolderPath = value;
        // }

        /// <summary> インスタンス化と同時にパスを指定して検索します。 </summary>
        public GmlFileSearcher(string udxFolderPath)
        {
            if (!IsPathUdx(udxFolderPath)) return;
            GenerateFileDictionary(udxFolderPath);
        }

        /// <summary> パスを指定せずにインスタンス化する場合、あとで <see cref="GenerateFileDictionary"/> を実行する必要があります。 </summary>
        public GmlFileSearcher()
        {
        }

        /// <summary>
        /// 地域メッシュコードからgmlファイルリストを検索する辞書を構築します。
        /// </summary>
        public void GenerateFileDictionary(string udxFolderPathArg)
        {
            if (!IsPathUdx(udxFolderPathArg))
            {
                throw new IOException($"Path needs to address udx folder. path: {udxFolderPathArg}");
            }

            this.udxFolderPath = udxFolderPathArg;

            this.fileTable = new Dictionary<string, List<string>>();

            // パス: udx/(地物型)
            foreach (var dirPath in Directory.EnumerateDirectories(Path.GetFullPath(udxFolderPathArg)))
            {
                // パス: udx/(地物型)/(各gmlファイル)
                foreach (var filePath in Directory.EnumerateFiles(dirPath))
                {
                    if (Path.GetExtension(filePath) != ".gml") continue;
                    string fileName = Path.GetFileName(filePath);
                    // ファイル名の最初の '_' までが地域IDであると仮定します。
                    string areaId = fileName.Split('_').First();
                    FileTableAdd(areaId, filePath);
                }
            }
        }

        public static bool IsPathUdx(string path)
        {
            bool ret = Path.GetFileName(path) == "udx";
            ret &= Directory.Exists(path); 
            return ret;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var pair in this.fileTable)
            {
                sb.Append($"{pair.Key} => [");
                foreach (var fileName in pair.Value)
                {
                    sb.Append($"{fileName}  ");
                }

                sb.Append("]\n");
            }

            return sb.ToString();
        }
        
        /// <summary> udx フォルダに含まれる地域メッシュコードを配列で返します。 </summary>
        public string[] AreaIds
        {
            get
            {
                if (this.fileTable == null)
                {
                    throw new Exception("fileTable is null.");
                }
                return this.fileTable.Keys.ToArray();
            }
        }

        /// <summary>
        /// 指定した <paramref name="areaId"/>(地域メッシュコード) に属する gmlファイルのパスのリストを返します。
        /// <paramref name="doAbsolutePath"/> が true ならば絶対パス、 false なら udxFolderPath からの相対パスを返します。
        /// </summary>
        public IEnumerable<string> GetGmlFilePathsForAreaId(string areaId, bool doAbsolutePath)
        {
            if (!this.fileTable.ContainsKey(areaId))
            {
                throw new KeyNotFoundException($"Area id is not found in the file table. area id = {areaId}");
            }

            var pathList = this.fileTable[areaId];
            if (doAbsolutePath)
            {
                return pathList
                    .Select(relativePath => Path.Combine(this.udxFolderPath, relativePath))
                    .Select(Path.GetFullPath);
            }
            return pathList.ToArray();
        }
        
        /// <summary>
        /// gmlファイルのうち、<paramref name="areaId"/> が指定したものであり、かつ
        /// <see cref="GmlType"/> が <paramref name="typeTarget"/> で示されるタイプの1つであるものを返します。
        /// </summary>
        public IEnumerable<string> GetGmlFilePathsForAreaIdAndType(string areaId, GmlTypeTarget typeTarget,
            bool doAbsolutePath)
        {
            var found = new List<string>();
            var gmlsInArea = GetGmlFilePathsForAreaId(areaId, doAbsolutePath);
            foreach (var gml in gmlsInArea)
            {
                if (typeTarget.IsTypeTarget(GetGmlTypeFromPath(gml)))
                {
                    found.Add(gml);
                }
            }

            return found;
        }

        /// <summary>
        /// gmlの親フォルダ名が <see cref="GmlType"/> の接頭辞になっていることを前提とし、
        /// 親フォルダ名に対応する <see cref="GmlType"/> を返します。
        /// </summary>
        private static GmlType GetGmlTypeFromPath(string gmlPath)
        {
            string parentPath = Directory.GetParent(gmlPath)?.ToString();
            string parentFolder = Path.GetFileName(parentPath);
            return GmlTypeConvert.FromPrefix(parentFolder);
        }

        private void FileTableAdd(string areaId, string filePath)
        {
            string relativePath = GetRelativePath(filePath, this.udxFolderPath);
            if (this.fileTable.ContainsKey(areaId))
            {
                this.fileTable[areaId].Add(relativePath);
                return;
            }
            this.fileTable.Add(areaId, new List<string>{relativePath});
        }

        private static string GetRelativePath(string targetPath, string basePath)
        {
            targetPath = Path.GetFullPath(targetPath);
            basePath = Path.GetFullPath(basePath);
            if (!targetPath.StartsWith(basePath))
            {
                throw new Exception("targetPath is not subdirectory of basePath.");
            }

            string relativePath = targetPath.Substring(basePath.Length, targetPath.Length - basePath.Length);
            if (relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
            {
                relativePath = relativePath.Substring(1, relativePath.Length - 1);
            }

            return relativePath;
        }
    }
}