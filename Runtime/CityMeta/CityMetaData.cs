using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMeta
{
    /// <summary>
    /// Plateau元データのインポート時に ScriptableObject として保存されるメタデータです。<br/>
    /// ・PlateauオブジェクトのIDから、対応する gml ファイルを検索する辞書である <see cref="idToGmlTable"/> を保持します。
    ///   この情報の用途は、Unityシーン内のオブジェクトから対応する gml をロードして Plateau の情報を取得する処理の起点となります。<br/>
    /// ・インポート時の設定を保持します。
    ///   これにより、この ScriptableObject をプロジェクトビューから選択したときに「再変換」画面を出すことができます。
    ///
    /// </summary>
    public class CityMetaData : ScriptableObject
    {
        public IdToGmlTable idToGmlTable = new IdToGmlTable();
        public CityImporterConfig cityImporterConfig = new CityImporterConfig();

        public bool DoGmlTableContainsKey(string cityObjId)
        {
            return this.idToGmlTable.ContainsKey(cityObjId);
        }

        public void AddToGmlTable(string cityObjId, string gmlName)
        {
            this.idToGmlTable.Add(cityObjId, gmlName);
        }

        public bool TryGetValueFromGmlTable(string cityObjId, out string gmlFileName)
        {
            return this.idToGmlTable.TryGetValue(cityObjId, out gmlFileName);
        }

        public void DoClearIdToGmlTable()
        {
            this.idToGmlTable?.Clear();
        }
    }
}