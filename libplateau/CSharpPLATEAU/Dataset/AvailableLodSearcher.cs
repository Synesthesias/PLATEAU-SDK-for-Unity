using System.Collections.Generic;
using PLATEAU.Interop;

namespace PLATEAU.Dataset
{
    public static class LodSearcher
    {
        /// <summary>
        /// GMLファイルの中に存在するLODを検索してリストで返します。
        /// ファイルの全文を文字列検索するので、ファイル容量が大きいほど処理に時間がかかります。
        /// </summary>
        public static IEnumerable<uint> SearchLodsInFile(string filePath)
        {
            var filePathUtf8 = DLLUtil.StrToUtf8Bytes(filePath);
            NativeMethods.plateau_lod_searcher_search_lods_in_file(filePathUtf8, out uint lodFlags);
            var lods = new List<uint>();
            uint l = 0;
            while (lodFlags != 0)
            {
                if((lodFlags & 1) == 1) lods.Add(l);
                lodFlags >>= 1;
                l++;
            }

            return lods;
        }
    }
}
