using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlateauUnitySDK.Editor.EditorWindowCommon
{
    /// <summary>
    /// udxフォルダ内のgmlファイルを検索します。
    /// 
    /// 前提:
    /// ・ディレクトリ構造が udx/(地物型)/(複数のgmlファイル) になっていることを前提とします。
    /// ・gmlファイル名は ファイル名は [地域メッシュコード]_[地物型]_[CRS]_[オプション].gml です。
    /// 　詳しくは国交省仕様書 Ver2 の 324ページを参照してください。
    /// </summary>
    public class GmlFileSearcher
    {

        /// <summary> 地域メッシュコードからファイル名リストへの辞書です。 </summary>
        private Dictionary<string, List<string>> fileTable;

        /// <summary> インスタンス化と同時にパスを指定して検索します。 </summary>
        public GmlFileSearcher(string udxFolderPath)
        {
            GenerateFileDictionary(udxFolderPath);
        }

        /// <summary> パスを指定せずにインスタンス化する場合、あとで <see cref="GenerateFileDictionary"/> を実行する必要があります。 </summary>
        public GmlFileSearcher()
        {
            
        }

        /// <summary>
        /// 地域メッシュコードからgmlファイルリストを検索する辞書を構築します。
        /// </summary>
        public void GenerateFileDictionary(string udxFolderPath)
        {
            if (!IsPathUdx(udxFolderPath))
            {
                throw new IOException($"Path needs to address udx folder. path: {udxFolderPath}");
            }

            this.fileTable = new Dictionary<string, List<string>>();

            // パス: udx/(地物型)
            foreach (var dirPath in Directory.EnumerateDirectories(Path.GetFullPath(udxFolderPath)))
            {
                // パス: udx/(地物型)/(各gmlファイル)
                foreach (var filePath in Directory.EnumerateFiles(dirPath))
                {
                    if (Path.GetExtension(filePath) != ".gml") continue;
                    string fileName = Path.GetFileName(filePath);
                    string areaId = fileName.Split('_').First();
                    FileTableAdd(areaId, fileName);
                }
            }
        }

        public static bool IsPathUdx(string path)
        {
            return Path.GetFileName(path) == "udx";
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

        private void FileTableAdd(string areaId, string fileName)
        {
            if (this.fileTable.ContainsKey(areaId))
            {
                this.fileTable[areaId].Add(fileName);
                return;
            }
            this.fileTable.Add(areaId, new List<string>{fileName});
        }
    }
}