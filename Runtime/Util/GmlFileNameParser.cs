﻿using System;
using System.IO;
using System.Text;
using PlateauUnitySDK.Runtime.CityMeta;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    /// <summary>
    /// gmlファイルのファイル名は
    /// [メッシュコード]_[地物型]_[CRS]_[オプション].gml
    /// になっています。
    /// そこで、gmlのファイル名から情報を取り出します。
    /// 
    /// 詳しくは国交省の仕様書 ver02 の 7.2.3 ファイル名称 をご覧ください。
    /// <see href="https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_doc_0001_ver02.pdf">仕様書 ver02</see>
    /// </summary>
    internal static class GmlFileNameParser
    {
        /// <summary>
        /// gmlのファイル名から情報を取り出します。
        /// </summary>
        public static void Parse(string gmlFileName, out int areaId, out GmlType gmlType, out int crs, out string option)
        {
            gmlFileName = Preprocess(gmlFileName);
            
            string[] tokens = gmlFileName.Split('_');
            areaId = int.Parse(tokens[0]);
            string objTypeStr = tokens[1];
            gmlType = GmlTypeConvert.ToEnum(objTypeStr);
            crs = int.Parse(tokens[2]);
            option = tokens.Length >= 4 ? tokens[3] : null;
        }
        
        public static string NameWithoutOption(string gmlFileName)
        {
            gmlFileName = Preprocess(gmlFileName);
            string[] tokens = gmlFileName.Split('_');
            return $"{tokens[0]}_{tokens[1]}_{tokens[2]}";
        }

        /// <summary>
        /// このクラスでファイル名を受け取る時の前処理です。
        /// </summary>
        private static string Preprocess(string fileName)
        {
            // fileName に "フォルダ名/" が含まれていれば、それを削除してファイル名のみにします。
            fileName = Path.GetFileName(fileName);
            // 末尾に .gml が含まれていれば、それを削除します。
            return RemoveGmlExtension(fileName);
        }

        private static string RemoveGmlExtension(string fileName)
        {
            if (fileName.EndsWith(".gml"))
            {
                fileName = fileName.Substring(0, fileName.Length - ".gml".Length);
            }

            return fileName;
        }
    }
}