using System.IO;
using PLATEAU.Runtime.CityMeta;

namespace PLATEAU.Runtime.Util
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
            
            areaId = GetAreaId(gmlFileName);
            gmlType = GetGmlTypeEnum(gmlFileName);
            string[] tokens = gmlFileName.Split('_');
            crs = int.Parse(tokens[2]);
            option = tokens.Length >= 4 ? tokens[3] : null;
        }

        /// <summary> gmlファイル名から地域ID（メッシュコード）を取得します。 </summary>
        public static int GetAreaId(string gmlFileName)
        {
            gmlFileName = Preprocess(gmlFileName);
            return int.Parse(gmlFileName.Split('_')[0]);
        }

        /// <summary> gmlファイル名から地物タイプを取得します。Enum型なので未知の接頭辞には対応できず Etc が返ります。 </summary>
        public static GmlType GetGmlTypeEnum(string gmlFileName)
        {
            return GmlTypeConvert.ToEnum(GetGmlTypeStr(gmlFileName));
        }

        /// <summary> gmlファイル名から地物タイプを取得します。文字列型なので未知の接頭辞でも反映されます。 </summary>
        public static string GetGmlTypeStr(string gmlFileName)
        {
            gmlFileName = Preprocess(gmlFileName);
            return gmlFileName.Split('_')[1];
        }
        
        /// <summary> gmlファイル名からオプションと拡張子を除いて返します。 </summary>
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