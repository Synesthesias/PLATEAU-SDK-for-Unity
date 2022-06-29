using System;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// インポート時の設定について、地物タイプごとの設定項目です。
    /// </summary>
    
    // 補足:
    // 他クラスとの関係は CityImporterConfig -> 保持 -> GmlSearcherConfig -> 保持 -> GmlTypeTarget -> 保持 -> ImportGmlTypeConfig
    // という関係なので、 CityImporterConfig の注意事項に基づいてこのクラスには Serializable属性が付いている必要があります。
    
    // TODO 以前は ここにLOD設定を含めていたけど、分離したのでクラスの中身が bool だけになってしまった。
    //      これでは専用のクラスを用意する必要がない。単なるboolに直すべき。
    [Serializable]
    internal class ImportGmlTypeConfig
    {
        public bool isTarget;

        public ImportGmlTypeConfig()
        {
            this.isTarget = true;
        }
    }
}