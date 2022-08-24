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
    public class CityMetadata : ScriptableObject
    {
        /// <value>都市オブジェクトのIDから、それに対応する gmlファイル名を検索できる辞書です。</value>
        [SerializeField] internal IdToGmlTable idToGmlTable = new IdToGmlTable();

        // TODO 要検討 : 
        // << 検討事項 >>
        // [ idToGmlTable の必要性は？ ]
        // idToGmlTable とは、GameObject名 と Gmlファイル名を対応付ける辞書です。
        // 実装当初の目的は、ヒエラルキーの GameObject から 対応する CityObject を取得するために、どのGmlファイルを読めばいいのかを求める目的でした。
        // しかし、都市3Dモデルをシーンに自動配置する方法が変わり、 配置した都市3Dモデルの 親GameObject の名前が Gmlファイル名になりました。
        // したがって、わざわざ巨大な辞書を構築しなくても、単に親GameObject の名前を調べれば Gmlファイル名が分かることになります。
        // シーンへ自動配置する時にGmlファイル名を得るにあたっても、辞書は不要です。
        // なぜなら、インポートに利用した各Gmlファイルを覚えておいて、
        // 各Gmlでループして配置するのでその時はGmlファイル名が分かる仕組みだからです。
        // したがって、今のところは idToGmlTableが絶対必要というわけではなく、代わりに親オブジェクトの名前を得る処理に置き換え可能です。
        // idToGmlTable の辞書は容量が大きくなりがちなので、これを削除すると CityMetadata の保存容量と読み込みパフォーマンスが良くなることが見込まれます。
        // 一方で、この辞書が必要となる利用ケースも存在するかもしれません。
        // シーンの自動配置を利用しないとか、ゲームオブジェクトの階層構造を変えてしまったというシチュエーションでは、
        // 「親GameObjectの名前からGmlファイル名を取得する」方法は利用できず、この辞書が必要かもしれません。
        // そのような利用シーンが果たして存在するのか？ という点は要検討です。
        // そうであっても、膨大な数の GameObject の名前を辞書に入れるよりも良い方法があるのではないかとも思います。
        // 例えば、インポート時に生成した3Dモデルファイルの名称には Gmlファイル名が含まれています。
        // これは使えるかもしれませんが、一方で次の懸念点もあります。
        // ・Editモード時はファイル名を読めるが、Runtime時にはそれは動作するか？
        // ・3Dモデルとしての立体形状を伴わない CityObject を扱いたくなるケースもあるかもしれない、その時に困る。
        //
        // まとめ:
        // この辞書のせいで CityMetadata の保存容量が重くなり、削除したい気持ちもあります。
        // 一方で、この機能を削除または別の軽量なやり方に置き換えようとすると、PLATEAU Unity SDK が持つべき仕様、利用ケース、実装の各面で
        // かなりの考察が必要になります。


        /// <value>インポート時の設定です。</value>
        [SerializeField] internal CityImportConfig cityImportConfig = new CityImportConfig();

        [SerializeField] internal string[] gmlRelativePaths = { };

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