using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// <para>
    /// Plateau元データのインポート時に ScriptableObject として保存されるメタデータです。</para>
    /// ・PlateauオブジェクトのIDから、対応する gml ファイルを検索する辞書である <see cref="idToGmlTable"/> を保持します。
    ///   この情報の用途は、Unityシーン内のオブジェクトから対応する gml をロードして Plateau の情報を取得する処理の起点となります。<br/>
    /// ・インポート時の設定を保持します。
    ///   これにより、この ScriptableObject をプロジェクトビューから選択したときに「再変換」画面を出すことができます。
    ///
    /// </summary>
    public class CityMetaData : ScriptableObject
    {
        /// <value>都市オブジェクトのIDから、それに対応する gmlファイル名を検索できる辞書です。</value>
        [SerializeField] internal IdToGmlTable idToGmlTable = new IdToGmlTable();
        /// <value>インポート時の設定です。</value>
        [SerializeField] internal CityImportConfig cityImportConfig = new CityImportConfig();

        [SerializeField] internal string[] gmlRelativePaths = {};

        /// <summary>
        /// 辞書 <see cref="idToGmlTable"/> の中に引数をキーとするものがあるかどうか返します。
        /// </summary>
        /// <param name="cityObjId">キー(都市オブジェクトID)</param>
        /// <returns>あればtrue, なければfalse</returns>
        internal bool DoGmlTableContainsKey(string cityObjId)
        {
            return this.idToGmlTable.ContainsKey(cityObjId);
        }

        /// <summary>
        /// <see cref="idToGmlTable"/> に項目を追加します。
        /// </summary>
        internal void AddToGmlTable(string cityObjId, string gmlName)
        {
            this.idToGmlTable.Add(cityObjId, gmlName);
        }

        /// <summary>
        /// 都市オブジェクトのIDから、それに対応するgmlファイル名の取得を試みます。
        /// 取得できたかどうかを bool で返し、取得内容を out引数で返します。
        /// </summary>
        internal bool TryGetValueFromGmlTable(string cityObjId, out string gmlFileName)
        {
            return this.idToGmlTable.TryGetValue(cityObjId, out gmlFileName);
        }

        /// <summary> <see cref="idToGmlTable"/> のデータを削除します。 </summary>
        internal void ClearIdToGmlTable()
        {
            this.idToGmlTable?.Clear();
        }
    }
}