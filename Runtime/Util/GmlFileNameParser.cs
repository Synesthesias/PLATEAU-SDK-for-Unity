using System;
using System.Text;

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
        public static void Parse(string gmlFileName, out int meshCode, out string objTypeStr, out int crs, out string option)
        {
            // 末尾に .gml があれば取り除きます。
            gmlFileName = RemoveGmlExtension(gmlFileName);

            string[] tokens = gmlFileName.Split('_');
            meshCode = int.Parse(tokens[0]);
            objTypeStr = tokens[1];
            crs = int.Parse(tokens[2]);
            option = tokens.Length >= 4 ? tokens[3] : null;
        }
        
        public static string NameWithoutOption(string gmlFileName)
        {
            gmlFileName = RemoveGmlExtension(gmlFileName);
            string[] tokens = gmlFileName.Split('_');
            return $"{tokens[0]}_{tokens[1]}_{tokens[2]}";
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