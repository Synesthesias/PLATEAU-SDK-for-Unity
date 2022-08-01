using System;

namespace PLATEAU.Util.FileNames
{
    /// <summary>
    /// 都市オブジェクトである GameObject の名称をパースします。
    /// 
    /// 名称は例えば LOD1_abc123 のように、 (LOD)_(PlateauObjectId) という書式になっていることを前提とし、
    /// 名称から LOD, ID を取得します。
    /// 逆にLOD, ID から名称を構築するメソッドもあります。
    /// </summary>

    internal static class GameObjNameParser
    {
        /// <summary>
        /// ゲームオブジェクト名から LOD の数値を取り出します。
        /// 成否を bool で返し、数値を out引数で返します。
        /// </summary>
        public static bool TryGetLod(string objName, out int lod)
        {
            // objName の部分文字列として、 "LOD" と 最初の "_" に挟まれたものを求めます。 
            lod = -1;
            var sc = StringComparison.Ordinal;
            int lodIndex = objName.IndexOf("LOD", sc);
            int underScoreIndex = objName.IndexOf("_", sc);
            if (lodIndex < 0 || underScoreIndex < 0) return false;
            int digitsLen = underScoreIndex - lodIndex - 3;
            if (digitsLen <= 0) return false;
            string numStr = objName.Substring(lodIndex + 3, digitsLen);
            // 部分文字列を数値に直して lod とします。
            bool succeed = int.TryParse(numStr, out lod);
            return succeed;
        }

        /// <summary>
        /// ゲームオブジェクト名から 都市モデルのID を取り出します。
        /// 成否を bool で返し、IDをout引数で返します。
        /// </summary>
        public static bool TryGetId(string objName, out string id)
        {
            // オブジェクト名から先頭の LOD(num)_ を取り除いた部分がIDです。 
            id = "";
            int underScoreIndex = objName.IndexOf("_", StringComparison.Ordinal);
            if (underScoreIndex < 0 || underScoreIndex >= objName.Length-1) return false;
            id = objName.Substring(underScoreIndex + 1);
            return true;
        }

        /// <summary>
        /// LOD, id から ゲームオブジェクト名を構築します。
        /// </summary>
        public static string ComposeName(int lod, string id)
        {
            return $"LOD{lod}_{id}";
        }
    }
}