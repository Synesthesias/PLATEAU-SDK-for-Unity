using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PLATEAU.CityMeta;
using PLATEAU.Util.FileNames;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// udxフォルダ内のgmlファイルを検索します。
    /// 地域IDや地物タイプの条件に合致する gml をファイルシステムから検索します。
    /// 
    /// 前提:
    /// ・ディレクトリ構造が udx/(地物型)/(複数のgmlファイル) になっていることを前提とします。
    /// 　詳しくは国交省仕様書 Ver2 の 324ページを参照してください。
    /// </summary>
    internal class GmlSearcherModel
    {

        /// <summary>
        /// 地域メッシュコード（地域ID）からファイルリストへの辞書です。
        /// ここでいうファイルリストとは (<see cref="srcRootFolderPath"/>)/udx からの相対パスのリストです。
        /// 例: {53394525 => {bldg\53394525_bldg_6697_2_op.gml  brid\53394525_brid_6697_op.gml }}
        /// </summary>
        private Dictionary<int, List<string>> fileTable;

        private string srcRootFolderPath = "";

        /// <summary> インスタンス化と同時にパスを指定して検索します。 </summary>
        public GmlSearcherModel(string srcRootFolderPath)
        {
            if (!IsPathPlateauRoot(srcRootFolderPath)) return;
            GenerateFileDictionary(srcRootFolderPath);
        }
        

        /// <summary>
        /// 地域メッシュコードからgmlファイルリストを検索する辞書を構築します。
        /// </summary>
        public void GenerateFileDictionary(string plateauSrcRootPathArg)
        {
            if (!IsPathPlateauRoot(plateauSrcRootPathArg))
            {
                throw new IOException($"Path needs to address plateau folder. path: {plateauSrcRootPathArg}");
            }

            this.srcRootFolderPath = plateauSrcRootPathArg;

            this.fileTable = new Dictionary<int, List<string>>();

            // パス: udx/(地物型)
            foreach (var dirPath in Directory.EnumerateDirectories(Path.GetFullPath(Path.Combine(plateauSrcRootPathArg, "udx"))))
            {
                // パス: udx/(地物型)/(各gmlファイル)
                foreach (var filePath in Directory.EnumerateFiles(dirPath))
                {
                    if (Path.GetExtension(filePath) != ".gml") continue;
                    string fileName = Path.GetFileName(filePath);
                    int areaId = GmlFileNameParser.GetAreaId(fileName);
                    FileTableAdd(areaId, filePath);
                }
            }
        }

        /// <summary>
        /// 与えられたパスが Plateau元データのRootフォルダ であるかどうか判別します。
        /// Root直下に udx という名前のフォルダがあればOKとみなします。
        /// </summary>
        public static bool IsPathPlateauRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string udxPath = Path.Combine(path, "udx");
            return Directory.Exists(udxPath);
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
        public int[] AreaIds
        {
            get
            {
                if (this.fileTable == null)
                {
                    return new int[] { };
                }
                return this.fileTable.Keys.ToArray();
            }
        }

        /// <summary>
        /// 指定した <paramref name="areaId"/>(地域メッシュコード) に属する gmlファイルのパスのリストを返します。
        /// <paramref name="doAbsolutePath"/> が true ならば絶対パス、 false なら udxFolderPath からの相対パスを返します。
        /// </summary>
        private IEnumerable<string> GetGmlFilePathsForAreaId(int areaId, bool doAbsolutePath)
        {
            if (!this.fileTable.ContainsKey(areaId))
            {
                // AreaTree の木構造の都合で生成された 第2地域区画のIDのノードは、実際には存在しないことがあるので空を返します。
                return new string[]{};
            }

            var pathList = this.fileTable[areaId];
            if (doAbsolutePath)
            {
                return pathList
                    .Select(relativePath => Path.Combine(this.srcRootFolderPath, "udx",  relativePath))
                    .Select(Path.GetFullPath);
            }
            return pathList.ToArray();
        }
        
        /// <summary>
        /// gmlファイルのうち、<paramref name="areaId"/> が指定したものであり、かつ
        /// <see cref="GmlType"/> が <paramref name="searcherConfig"/> で対象とするタイプリストに含まれるものを0個以上返します。
        /// </summary>
        public IEnumerable<string> GetGmlFilePathsForAreaIdAndType(int areaId, GmlSearcherConfig searcherConfig,
            bool doAbsolutePath)
        {
            var found = new List<string>();
            var gmlsInArea = GetGmlFilePathsForAreaId(areaId, doAbsolutePath);
            foreach (var gml in gmlsInArea)
            {
                if (searcherConfig.GetIsTypeTarget(GetGmlTypeFromPath(gml)))
                {
                    found.Add(gml);
                }
            }

            return found;
        }
        
        public static List<string> ListTargetGmlFiles(GmlSearcherModel gmlSearcherModel, GmlSearcherConfig searcherConfig)
        {
            var gmlFiles = new List<string>();
            AreaTree areaTree = searcherConfig.AreaTree;

            var nodes = areaTree.IterateDfs();
            // 地域ツリーの各地域について
            foreach (var tuple in nodes)
            {
                var area = tuple.node.Value;
                if (!area.IsTarget) continue;
                // 対象地域に対応する gmlファイルのうち 、選択したgmlタイプに合致するものをすべて追加します。
                gmlFiles.AddRange(gmlSearcherModel.GetGmlFilePathsForAreaIdAndType(area.Id, searcherConfig, false));
            }

            return gmlFiles;
        }

        /// <summary>
        /// 与えられた <paramref name="areaIds"/> のリストの中に、どのような <see cref="GmlType"/> が含まれているかを返します。
        /// 戻り値は辞書形式で、 key(各 GmlType ) に対して value は存在すれば true, しなければ false になります。
        /// </summary>
        public Dictionary<GmlType, bool> ExistingTypesForAreaIds(IEnumerable<int> areaIds)
        {
            var typeExistingDict = GmlTypeConvert.ComposeTypeDict(false);
            foreach (int areaId in areaIds)
            {
                if (!this.fileTable.ContainsKey(areaId))
                {
                    // AreaTree の木構造の都合で生成された 第2地域区画のIDのノードは、実際には存在しないことがあるので無視します。
                    continue;
                }
                List<string> gmlPaths = this.fileTable[areaId];
                foreach (string gmlPath in gmlPaths)
                {
                    var type = GmlFileNameParser.GetGmlTypeEnum(gmlPath);
                    typeExistingDict[type] = true;
                }
            }

            return typeExistingDict;
        }

        /// <summary>
        /// gmlの親フォルダ名が <see cref="GmlType"/> の接頭辞になっていることを前提とし、
        /// 親フォルダ名に対応する <see cref="GmlType"/> を返します。
        /// </summary>
        private static GmlType GetGmlTypeFromPath(string gmlPath)
        {
            string parentPath = Directory.GetParent(gmlPath)?.ToString();
            string parentFolder = Path.GetFileName(parentPath);
            return GmlTypeConvert.ToEnum(parentFolder);
        }

        private void FileTableAdd(int areaId, string filePath)
        {
            string relativePath = GetRelativePath(filePath, Path.Combine(this.srcRootFolderPath, "udx"));
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